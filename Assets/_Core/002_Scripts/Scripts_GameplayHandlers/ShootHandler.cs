using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class ShootHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShooterData _playerShooterData;
    [SerializeField] private ShooterData _aiShooterData;
    
    // Settings used for the shooting system
    [Header("Shoot Settings")]
    [SerializeField] private ShootSettings _shootSettings;
    
    private void Awake()
    {
        GameModeEvents.OnShootAttempt += OnShootAttempt;
        GameModeEvents.OnShootPositionUpdated += OnShootPositionUpdated;
    }

    private void Start()
    {
        ResetBall(true);
        ResetBall(false);
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShootAttempt -= OnShootAttempt;
        GameModeEvents.OnShootPositionUpdated -= OnShootPositionUpdated;
    }

    /// <summary>
    /// Take as input the value of the shooting slider (or the generated number if the shooter is an ai) and performs the shot
    /// </summary>
    /// <param name="shootVelocity"></param>
    /// <param name="isHumanPlayer"></param>
    private void OnShootAttempt(float shootVelocity, bool isHumanPlayer)
    {
        ShootResult result = GetShootResult(shootVelocity, isHumanPlayer);
        Debug.Log($"ShootResult | Type: {result.Type} | Accuracy: {result.Accuracy} | Strength: {result.Strength}");
        
        ShooterData shooterData = isHumanPlayer ? _playerShooterData : _aiShooterData;

        ShootContext context = new ShootContext()
        {
            IsHuman = isHumanPlayer,
            Path = GetShootPath(result.Type, result.Accuracy, result.Strength, result.IsHumanPlayer),
            Result = result,
            Shooter = shooterData,
        };
        
        // Play audio
        AudioEvents.TriggerPlayAudioFX(AudioFXId.Shoot);
        
        StartCoroutine(StartShoot(context));
    }

    /// <summary>
    /// Generates a result from the shooting attempt
    /// </summary>
    /// <param name="shootVelocity"></param>
    /// <param name="isHumanPlayer"></param>
    /// <returns></returns>
    private ShootResult GetShootResult(float shootVelocity, bool isHumanPlayer)
    {
        ShootType type = ShootType.Direct;
        ShootAccuracy accuracy = ShootAccuracy.Fail;

        // velocity configs (limits on slider) from direct and backboard shot types
        ShootVelocityConfigByType directVelocityConfig = RuntimeServices.GameModeService.GameModeSettings.GetShootVelocityConfig(ShootType.Direct);
        ShootVelocityConfigByType backboardVelocityConfig = RuntimeServices.GameModeService.GameModeSettings.GetShootVelocityConfig(ShootType.Backboard);
        
        // Debug.Log($"GetShootResult | v {shootVelocity} | direct min {directVelocityConfig.Min} max {directVelocityConfig.Max} | backboard min {backboardVelocityConfig.Min} max {backboardVelocityConfig.Max}");

        // --- PERFECT SHOT (if shoot velocity is exactly inside the config limits
        if (shootVelocity >= directVelocityConfig.Min && shootVelocity <= directVelocityConfig.Max)
        {
            type = ShootType.Direct;
            accuracy = ShootAccuracy.Perfect;
        }
        else if (shootVelocity >= backboardVelocityConfig.Min && shootVelocity <= backboardVelocityConfig.Max)
        {
            type = ShootType.Backboard;
            accuracy = ShootAccuracy.Perfect;
        }
        // --- ACCURATE SHOT (if the shoot velocity is outside limits but close enough to the threshold
        else
        {
            if (Mathf.Abs(directVelocityConfig.Min - shootVelocity) <= _shootSettings.DirectAccuracyThreshold
                || Mathf.Abs(directVelocityConfig.Max - shootVelocity) <= _shootSettings.DirectAccuracyThreshold)
            {
                type = ShootType.Direct;
                accuracy = ShootAccuracy.Accurate;
            }
            else if (Mathf.Abs(backboardVelocityConfig.Min - shootVelocity) <= _shootSettings.BackboardAccuracyThreshold
                     || Mathf.Abs(backboardVelocityConfig.Max - shootVelocity) <= _shootSettings.BackboardAccuracyThreshold)
            {
                type = ShootType.Backboard;
                accuracy = ShootAccuracy.Accurate;
            }
            else
            {
                // --- FAILED SHOT (if outside any previous limit checks
                if (shootVelocity > directVelocityConfig.Max)
                {
                    type = ShootType.Backboard;
                    accuracy = ShootAccuracy.Fail;
                }
                else if (shootVelocity < directVelocityConfig.Min)
                {
                    type = ShootType.Direct;
                    accuracy = ShootAccuracy.Fail;
                }
            }
        }
        
        return new ShootResult(type, accuracy, GetShootVelocityType(shootVelocity), isHumanPlayer);;
    }

    /// <summary>
    /// Generate a velocity type based by dividing the max shoot velocity by three thirds and checking the one the value is into
    /// </summary>
    /// <param name="shootVelocity"></param>
    /// <returns></returns>
    private ShootVelocityType GetShootVelocityType(float shootVelocity)
    {
        float lowThreshold = GameModeEnv.MAX_SHOOT_VELOCITY / 3.0f;
        float midThreshold = GameModeEnv.MAX_SHOOT_VELOCITY - lowThreshold;
        if (shootVelocity < lowThreshold) return ShootVelocityType.Weak;
        if (shootVelocity < midThreshold) return ShootVelocityType.Medium;
        return ShootVelocityType.Strong;
    }

    /// <summary>
    /// Starts the shoot curve path
    /// </summary>
    /// <param name="shootContext"></param>
    private IEnumerator StartShoot(ShootContext shootContext)
    {
        yield return new WaitForSeconds(_shootSettings.ShootWaitTime);
        
        if (shootContext.Path.Steps == null || shootContext.Path.Steps.Count == 0)
            yield break;

        // Call the event passing the target of the first step of the curve path
        GameModeEvents.TriggerFirstShootTargetSet(shootContext.Path.Steps[0].Target, shootContext.Shooter.IsHumanPlayer);

        // remove ball from start position parent
        shootContext.Shooter.Ball.transform.SetParent(null);
        
        // Execute the first curve step
        ExecuteStep(shootContext, 0);
        
        // Get the current total time to consider the shot completed, based on path time given by the current shot result
        float shotValidateTime = _shootSettings.GetShotValidateTime(shootContext.Result);
        // Debug.Log($"shotValidateTime: {shotValidateTime}");
        
        StartCoroutine(CallShotCompleted(shootContext, shotValidateTime));
    }

    /// <summary>
    /// Execute the current path step (based on index)
    /// </summary>
    /// <param name="context"></param>
    /// <param name="index"></param>
    private void ExecuteStep(ShootContext context, int index)
    {
        if (!context.LastBounceFound)
        {
            context.LastBouncePosition = context.Shooter.CharacterTransform.position;
        }

        // if every curve path step is handled, call the shot completed 
        if (index >= context.Path.Steps.Count)
        {
            //CallShotCompleted();
            return;
        }

        ShootPathStep step = context.Path.Steps[index];

        HandleShootStep(step, () =>
            {
                ExecuteStep(context, index + 1);
                step.OnStepCompleted?.Invoke();
            },
            index == 0,
            context);
    }

    /// <summary>
    /// Handle the single curve path step
    /// </summary>
    /// <param name="step"></param>
    /// <param name="onComplete"></param>
    /// <param name="firstStep"></param>
    /// <param name="context"></param>
    private void HandleShootStep(ShootPathStep step, Action onComplete, bool firstStep, ShootContext context)
    {
        if (step.LastBounce)
        {
            context.LastBouncePosition = step.Target;
            context.LastBounceFound = true;
        }

        Ease ease = firstStep ? _shootSettings.ShootEase : _shootSettings.BounceEase;

        context.Shooter.Ball.transform.DOKill();
        context.Shooter.Ball.transform.DOJump(step.Target, step.Power, 1, step.Duration).SetEase(ease).OnComplete(() => onComplete?.Invoke());
    }

    private IEnumerator CallShotCompleted(ShootContext context, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        
        EnablePhysics(context.LastBouncePosition, context.Shooter);

        // Update the runtime shoot phase value
        if (context.IsHuman)
        {
            RuntimeServices.GameModeService.HumanPlayerState.ShootPhase = PlayerShootPhase.Completed;
        }
        else
        {
            RuntimeServices.GameModeService.AIPlayerState.ShootPhase = PlayerShootPhase.Completed;
        }
        
        GameModeEvents.TriggerShootCompleted(context.Result);
    }

    private void EnablePhysics(Vector3 lastBouncePosition, ShooterData shooterData)
    {
        shooterData.Ball.isKinematic = false;

        // direction from last bounce position
        Vector3 bounceDirection = (shooterData.Ball.position - lastBouncePosition).normalized;

        Vector3 force = bounceDirection + Vector3.down * _shootSettings.FinalSimulatedBounceMultiplier;

        shooterData.Ball.velocity = Vector3.zero;
        // _ballBody.angularVelocity = Vector3.zero;

        shooterData.Ball.AddForce(force, ForceMode.Impulse);
    }

    /// <summary>
    /// Generate a path getting the info from the TargetHandler, creating a step for every valid target 
    /// </summary>
    /// <param name="shootType"></param>
    /// <param name="accuracyType"></param>
    /// <param name="shootVelocityType"></param>
    /// <param name="isHumanPlayer"></param>
    /// <returns></returns>
    private ShootPath GetShootPath(ShootType shootType, ShootAccuracy accuracyType, ShootVelocityType shootVelocityType, bool isHumanPlayer)
    {
        ShootPath path = new ShootPath();

        Vector3 groundFailAreaTargetPos = Vector3.zero;

        RuntimeServices.TargetState targetState = isHumanPlayer ? RuntimeServices.TargetService.PlayerTargetState : RuntimeServices.TargetService.AITargetState;

        // the position from the backboard target adding a small randomized offset inside a circe
        Vector3 backboardTargetPos = GetRandomOffsetInsideCircle(targetState.BackboardTargetPos, false);

        // the position from the frame target adding a small randomized offset inside a circe
        Vector3 frameTargetPos = GetRandomOffsetInsideCircle(targetState.FrameTargetPos);

        // the position from the frame fail target adding a small randomized offset inside a circe
        Vector3 frameFailTargetPos = GetRandomOffsetInsideCircle(targetState.FrameFailTargetPos);

        switch (shootVelocityType)
        {
            // if shot was strong, add a y offset to backboard target to set the ball position higher
            case ShootVelocityType.Strong:
                backboardTargetPos += new Vector3(0, _shootSettings.StrongShootBackboardTargetYOffset, 0);
                break;
        }

        if (accuracyType == ShootAccuracy.Fail)
        {
            switch (shootType)
            {
                // in case of failing the curve is set to end to a ground target that adapts its position based on player position
                case ShootType.Direct:
                    groundFailAreaTargetPos = GetRandomOffsetInsideCircle(targetState.DirectFailGroundTargetPos);
                    break;

                // in case of failing the curve is set to end to a ground target that adapts its position based on a sequence of positions that simulate bouncing
                case ShootType.Backboard:
                    groundFailAreaTargetPos = GetRandomOffsetInsideCircle(targetState.BackboardFailGroundTargetPos);
                    break;
            }
        }

        switch (shootType)
        {
            // create curve steps in case of direct shot
            case ShootType.Direct:
                switch (accuracyType)
                {
                    // directly to shoot scoring
                    case ShootAccuracy.Perfect:
                        ShootPathStep shootToScore = new ShootPathStep(
                            _shootSettings.ShootForce, 
                            _shootSettings.ShootDuration, 
                            RuntimeServices.TargetService.ScoreTargetPos,
                            onStepCompleted: ()=> AudioEvents.TriggerPlayAudioFX(AudioFXId.Perfect));
                        
                        path.Steps.Add(shootToScore);
                        break;

                    // shoot -> loop rim -> score
                    case ShootAccuracy.Accurate:
                        ShootPathStep shootToRim = new ShootPathStep(
                            _shootSettings.ShootForce, 
                            _shootSettings.ShootDuration, 
                            frameTargetPos, 
                            true, 
                            onStepCompleted: ()=>
                            {
                                AnimationEvents.TriggerRimTouched();
                                AudioEvents.TriggerPlayAudioFX(AudioFXId.BounceRim);
                                
                            });
                        path.Steps.Add(shootToRim);
                        
                        ShootPathStep rimToScore = new ShootPathStep(
                            _shootSettings.RimToScoreForce,
                            _shootSettings.RimToScoreDuration,  
                            RuntimeServices.TargetService.ScoreTargetPos,
                            onStepCompleted: ()=> AudioEvents.TriggerPlayAudioFX(AudioFXId.Accurate));
                        
                        path.Steps.Add(rimToScore);
                        break;

                    // shoot to ground position
                    case ShootAccuracy.Fail:
                        ShootPathStep shootToGround = new ShootPathStep(_shootSettings.ShootToGroundForce, _shootSettings.ShootToGroundDuration, groundFailAreaTargetPos);
                        path.Steps.Add(shootToGround);
                        break;
                }
                break;
            
            // create curve steps in case of backboard shot
            case ShootType.Backboard:
                
                // add always a first step with target the adaptive backboard position
                ShootPathStep baseStepToBackboard = new ShootPathStep(
                    _shootSettings.ShootForce,
                    _shootSettings.ShootDuration,
                    backboardTargetPos, 
                    lastBounce: accuracyType == ShootAccuracy.Perfect,
                    onStepCompleted: ()=> AudioEvents.TriggerPlayAudioFX(AudioFXId.BounceBackboard));
                
                path.Steps.Add(baseStepToBackboard);

                switch (accuracyType)
                {
                    // from backboard to score target
                    case ShootAccuracy.Perfect:
                        ShootPathStep shootToScore = new ShootPathStep(
                            _shootSettings.BounceForce,
                            _shootSettings.BounceDuration,
                            RuntimeServices.TargetService.ScoreTargetPos,
                            onStepCompleted: ()=> AudioEvents.TriggerPlayAudioFX(AudioFXId.Perfect));
                        
                        path.Steps.Add(shootToScore);
                        break;

                    // backboard -> rim -> score target
                    case ShootAccuracy.Accurate:
                        ShootPathStep shootToRim = new ShootPathStep(
                            _shootSettings.BounceForce, 
                            _shootSettings.BounceDuration, 
                            frameTargetPos, true, 
                            onStepCompleted: ()=>
                            {
                                AnimationEvents.TriggerRimTouched();
                                AudioEvents.TriggerPlayAudioFX(AudioFXId.BounceRim);
                            });
                        
                        path.Steps.Add(shootToRim);
                        
                        ShootPathStep rimToScore = new ShootPathStep(
                            _shootSettings.RimToScoreForce, 
                            _shootSettings.RimToScoreDuration, 
                            RuntimeServices.TargetService.ScoreTargetPos,
                            onStepCompleted: ()=> AudioEvents.TriggerPlayAudioFX(AudioFXId.Accurate));
                        
                        path.Steps.Add(rimToScore);
                        break;

                    // backboard -> rim (fail, outside) -> adaptive ground position
                    case ShootAccuracy.Fail:
                        ShootPathStep shootToFailRim = new ShootPathStep(
                            _shootSettings.BounceForce, 
                            _shootSettings.BounceDuration, 
                            frameFailTargetPos, 
                            onStepCompleted: ()=>
                            {
                                AnimationEvents.TriggerRimTouched();
                                AudioEvents.TriggerPlayAudioFX(AudioFXId.BounceRim);
                            });
                        
                        path.Steps.Add(shootToFailRim);
                        
                        ShootPathStep backboardToGround = new ShootPathStep(_shootSettings.BounceToGroundForce, _shootSettings.BounceToGroundDuration, groundFailAreaTargetPos);
                        path.Steps.Add(backboardToGround);
                        break;
                }

                break;
        }

        return path;
    }

    private Vector3 GetRandomOffsetInsideCircle(Vector3 originalPos, bool yUp = true)
    {
        Vector2 randomInsideCircle = UnityEngine.Random.insideUnitCircle * _shootSettings.RandomOffsetInsideCircleRadius;
        Vector3 newPosInRadius = new Vector3(originalPos.x + randomInsideCircle.x, originalPos.y + randomInsideCircle.y, originalPos.z);
        if (yUp)
        {
            newPosInRadius = new Vector3(originalPos.x + randomInsideCircle.x, originalPos.y, originalPos.z + randomInsideCircle.y);
        }

        return newPosInRadius;
    }

    private void OnShootPositionUpdated(bool isHumanPlayer)
    {
        ResetBall(isHumanPlayer);
    }

    private void ResetBall(bool isHumanPlayer)
    {
        if (isHumanPlayer)
        {
            _playerShooterData.Ball.transform.DOKill();
            _playerShooterData.Ball.velocity = Vector3.zero;
            _playerShooterData.Ball.isKinematic = true;
            _playerShooterData.Ball.transform.SetParent(_playerShooterData.BallStartPosition);
            _playerShooterData.Ball.transform.localPosition = Vector3.zero;
        }
        else
        {
            _aiShooterData.Ball.transform.DOKill();
            _aiShooterData.Ball.velocity = Vector3.zero;
            _aiShooterData.Ball.isKinematic = true;
            _aiShooterData.Ball.transform.SetParent(_aiShooterData.BallStartPosition);
            _aiShooterData.Ball.transform.localPosition = Vector3.zero;
        }
    }
}

/// <summary>
/// The generated path containing every step
/// </summary>
public class ShootPath
{
    public List<ShootPathStep> Steps = new List<ShootPathStep>();
}

/// <summary>
/// The single step of the shooting animation path
/// </summary>
public class ShootPathStep
{
    // The height of the jump curve
    public float Power;

    // How much the step lasts
    public float Duration;

    // The final target of the curve step
    public Vector3 Target;

    // If there's any event to call when steps  ends (i.e. loop frame bounce animation when hit)
    public Action OnStepCompleted;
    
    // Check if this was the last step the let the next shoot timer know when to start and to handle extra ball physics
    public bool LastBounce;

    public ShootPathStep(float power, float duration, Vector3 target, bool lastBounce = false, Action onStepCompleted = null)
    {
        Power = power;
        Duration = duration;
        Target = target;
        LastBounce = lastBounce;
        OnStepCompleted = onStepCompleted;
    }
}

/// <summary>
/// The result class used in the shoot system
/// </summary>
public class ShootResult
{
    // Type is selected taking in consideration the shoot slider value and comparing it to the current velocity bars.
    // If alings perfectly or it's close enough the type is generated based on the specific bar 
    public ShootType Type;

    // Accuracy is selected comparing the shoot slider value to the current velocity limits. 
    // If the value is inside the limits is Perfect shot, if close is Accurarate, else is a Fail
    public ShootAccuracy Accuracy;

    // Strength is based on which third of the slider the "velocity" slider value is in, (low, middle, hight)
    public ShootVelocityType Strength;

    // Checks if the shoot is handled by a human player or by an ai player
    public bool IsHumanPlayer;

    public ShootResult(ShootType type, ShootAccuracy accuracy, ShootVelocityType strength, bool isHumanPlayer)
    {
        Type = type;
        Accuracy = accuracy;
        Strength = strength;
        IsHumanPlayer = isHumanPlayer;
    }
}

[System.Serializable]
public class ShooterData
{
    public Transform CharacterTransform;
    public Rigidbody Ball;
    
    // Where the ball will be repositioned after each shooting
    public Transform BallStartPosition;

    public bool IsHumanPlayer;
}

/// <summary>
/// Create a separate context for the current shoot attempt handler, separated between player and AI
/// </summary>
public class ShootContext
{
    public bool IsHuman;
    public ShooterData Shooter;
    public ShootResult Result;
    public ShootPath Path;
    
    public Vector3 LastBouncePosition;
    public bool LastBounceFound;
}