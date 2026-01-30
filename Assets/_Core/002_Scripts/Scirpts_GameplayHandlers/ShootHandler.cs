using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class ShootHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Rigidbody _ballBody;
    // Where the ball will be repositioned after each shooting
    [SerializeField] private Transform _shootStartPos;

    // Settings used for the shooting system
    [Header("Shoot Settings")]
    [SerializeField] private ShootSettings _shootSettings;

    [Header("Bounce physics settings")]
    [SerializeField] private float _bounceForceMultiplier = 1;

    [Header("Ease settings")]
    [SerializeField] private Ease _shootEase = Ease.Linear;
    [SerializeField] private Ease _bounceEase = Ease.Linear;

    [Header("Target Settings")]
    [SerializeField] private float _strongShootBackboardTargetYOffset = 0.25f;
    [SerializeField] private float _weakShootBackboardTargetYOffset = 0.13f;

    [Header("Runtime Settings")]
    [SerializeField] private ShootDirection _position;

    // Cache the current shoot result to call the shoot completed event
    private ShootResult _currentShootResult;
    private Vector3 _lastBouncePosition;
    private bool _lastBounceFound = false;

    private void Awake()
    {
        GameModeEvents.OnShootAttempt += Shoot;
        GameModeEvents.OnShootPositionUpdated += OnShootPositionUpdated;
    }

    private void Start()
    {
        ResetBall();
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShootAttempt -= Shoot;
        GameModeEvents.OnShootPositionUpdated -= OnShootPositionUpdated;
    }

    /// <summary>
    /// Take as input the value of the shooting slider (or the generated number if the shooter is an ai) and performs the shot
    /// </summary>
    /// <param name="shootVelocity"></param>
    /// <param name="isHumanPlayer"></param>
    private void Shoot(float shootVelocity, bool isHumanPlayer)
    {
        ShootResult result = GetShootResult(shootVelocity, isHumanPlayer);
        Debug.Log($"ShootResult | Type: {result.Type} | Accuracy: {result.Accuracy} | Strength: {result.Strength}");
        StartShoot(GetShootPath(result.Type, _position, result.Accuracy, result.Strength));
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

        _currentShootResult = new ShootResult(type, accuracy, GetShootVelocityType(shootVelocity), isHumanPlayer);

        return _currentShootResult;
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
    /// <param name="path"></param>
    private void StartShoot(ShootPath path)
    {
        if (path.Steps == null || path.Steps.Count == 0)
            return;

        // Call the event passing the target of the first step of the curve path
        GameModeEvents.TriggerFirstShootTargetSet(path.Steps[0].Target);

        // Execute the first curve step
        ExecuteStep(path, 0);
    }

    /// <summary>
    /// Execute the current path step (based on index)
    /// </summary>
    /// <param name="path"></param>
    /// <param name="index"></param>
    private void ExecuteStep(ShootPath path, int index)
    {
        if (!_lastBounceFound)
        {
            _lastBouncePosition = _playerTransform.transform.position;
        }

        // if every curve path step is handled, call the shot completed 
        if (index >= path.Steps.Count)
        {
            ShotCompleted();
            return;
        }

        // Sequence seq = DOTween.Sequence();
        //
        // for (int i = 0; i < path.Steps.Count; i++)
        // {
        //     Ease ease = i == 0 ? shootEase : bounceEase;
        //
        //     Tween t = _ballTransform
        //         .DOJump(path.Steps[i].Target, path.Steps[i].Power, 1, path.Steps[i].Duration)
        //         .SetEase(ease);
        //
        //     seq.Append(t);
        //     seq.AppendInterval(-path.Steps[i].Duration * 0.25f);
        // }
        //
        // seq.OnComplete(() =>
        // {
        //     EnablePhysics();
        //     GameModeEvents.TriggerShootCompleted(_currentShootResult);
        // });

        ShootPathStep step = path.Steps[index];

        HandleShootStep(step, () =>
            {
                ExecuteStep(path, index + 1);
                step.OnStepStarted?.Invoke();
            },
            index == 0);
    }

    /// <summary>
    /// Handle the single curve path step
    /// </summary>
    /// <param name="step"></param>
    /// <param name="onComplete"></param>
    /// <param name="_firstStep"></param>
    private void HandleShootStep(ShootPathStep step, Action onComplete, bool _firstStep)
    {
        if (step.LastBounce)
        {
            _lastBouncePosition = step.Target;
            _lastBounceFound = true;
        }

        Ease ease = _firstStep ? _shootEase : _bounceEase;

        _ballBody.transform.DOJump(step.Target, step.Power, 1, step.Duration).SetEase(ease).OnComplete(() => onComplete?.Invoke());
    }

    private void ShotCompleted()
    {
        EnablePhysics(_lastBouncePosition);

        GameModeEvents.TriggerShootCompleted(_currentShootResult);
    }

    private void EnablePhysics(Vector3 lastBounceDirection)
    {
        _ballBody.isKinematic = false;
        _ballBody.AddForce((_ballBody.transform.position - lastBounceDirection).normalized + Vector3.down * _bounceForceMultiplier, ForceMode.Impulse);
    }

    /// <summary>
    /// Generate a path getting the info from the TargetHandler, creating a step for every valid target 
    /// </summary>
    /// <param name="shootType"></param>
    /// <param name="shootDirection"></param>
    /// <param name="accuracyType"></param>
    /// <param name="shootVelocityType"></param>
    /// <returns></returns>
    private ShootPath GetShootPath(ShootType shootType, ShootDirection shootDirection, ShootAccuracy accuracyType, ShootVelocityType shootVelocityType)
    {
        ShootPath path = new ShootPath();

        Vector3 groundFailAreaTargetPos = Vector3.zero;

        // the position from the backboard target adding a small randomized offset inside a circe
        Vector3 backboardTargetPos = GetRandomOffsetInsideCircle(RuntimeServices.TargetService.BackboardTarget.position, _shootSettings.RandomOffsetInsideCircleRadius);

        // the position from the frame target adding a small randomized offset inside a circe
        Vector3 frameTargetPos = GetRandomOffsetInsideCircle(RuntimeServices.TargetService.FrameTarget.position, _shootSettings.RandomOffsetInsideCircleRadius);

        // the position from the frame fail target adding a small randomized offset inside a circe
        Vector3 frameFailTargetPos = GetRandomOffsetInsideCircle(RuntimeServices.TargetService.FrameFailTarget.position, _shootSettings.RandomOffsetInsideCircleRadius);

        switch (shootVelocityType)
        {
            // if shot was strong, add a y offset to backboard target to set the ball position higher
            case ShootVelocityType.Strong:
                backboardTargetPos += new Vector3(0, _strongShootBackboardTargetYOffset, 0);
                break;
        }

        if (accuracyType == ShootAccuracy.Fail)
        {
            switch (shootType)
            {
                // in case of failing during a direct shot, the curve is set to a fixed position on the ground, left or right based on player position relative to the hoop
                case ShootType.Direct:
                    switch (shootDirection)
                    {
                        case ShootDirection.Right:
                            groundFailAreaTargetPos = GetRandomOffsetInsideCircle(RuntimeServices.TargetService.RightDirectFailGroundTarget.position, _shootSettings.RandomOffsetInsideCircleRadius);
                            break;

                        case ShootDirection.Left:
                            groundFailAreaTargetPos = GetRandomOffsetInsideCircle(RuntimeServices.TargetService.LeftDirectFailGroundTarget.position, _shootSettings.RandomOffsetInsideCircleRadius);
                            break;
                    }

                    break;

                // in case of failing during a backboard shot, the curve is set to end to a ground target that adapts its position based on a sequence of positions that simulate bouncing
                case ShootType.Backboard:
                    groundFailAreaTargetPos = GetRandomOffsetInsideCircle(RuntimeServices.TargetService.BackboardFailGroundTarget.position, _shootSettings.RandomOffsetInsideCircleRadius);
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
                        ShootPathStep shootToScore = new ShootPathStep(_shootSettings.ShootForce, _shootSettings.ShootDuration, RuntimeServices.TargetService.ScoreTarget.position);
                        path.Steps.Add(shootToScore);
                        break;

                    // shoot -> loop rim -> score
                    case ShootAccuracy.Accurate:
                        ShootPathStep shootToRim = new ShootPathStep(_shootSettings.ShootForce, _shootSettings.ShootDuration, frameTargetPos, true, AnimationEvents.TriggerRimTouched);
                        path.Steps.Add(shootToRim);
                        ShootPathStep rimToScore = new ShootPathStep(_shootSettings.RimToScoreForce, _shootSettings.RimToScoreDuration, RuntimeServices.TargetService.ScoreTarget.position);
                        path.Steps.Add(rimToScore);
                        break;

                    // shoot to fixed ground position
                    case ShootAccuracy.Fail:
                        ShootPathStep shootToGround = new ShootPathStep(_shootSettings.ShootToGroundForce, _shootSettings.ShootToGroundDuration, groundFailAreaTargetPos);
                        path.Steps.Add(shootToGround);
                        break;
                }
                break;
            
            // create curve steps in case of backboard shot
            case ShootType.Backboard:
                
                // add always a first step with target the adaptive backboard position
                ShootPathStep baseStepToBackboard = new ShootPathStep(_shootSettings.ShootForce, _shootSettings.ShootDuration, backboardTargetPos, lastBounce: accuracyType == ShootAccuracy.Perfect);
                path.Steps.Add(baseStepToBackboard);

                switch (accuracyType)
                {
                    // from backboard to score target
                    case ShootAccuracy.Perfect:
                        ShootPathStep shootToScore = new ShootPathStep(_shootSettings.BounceForce, _shootSettings.BounceDuration, RuntimeServices.TargetService.ScoreTarget.position);
                        path.Steps.Add(shootToScore);
                        break;

                    // backboard -> rim -> score target
                    case ShootAccuracy.Accurate:
                        ShootPathStep shootToRim = new ShootPathStep(_shootSettings.BounceForce, _shootSettings.BounceDuration, frameTargetPos, true, AnimationEvents.TriggerRimTouched);
                        path.Steps.Add(shootToRim);
                        ShootPathStep rimToScore = new ShootPathStep(_shootSettings.RimToScoreForce, _shootSettings.RimToScoreDuration, RuntimeServices.TargetService.ScoreTarget.position);
                        path.Steps.Add(rimToScore);
                        break;

                    // backboard -> rim (fail, outside) -> adaptive ground position
                    case ShootAccuracy.Fail:
                        ShootPathStep shootToFrailFrame = new ShootPathStep(_shootSettings.BounceForce, _shootSettings.BounceDuration, frameFailTargetPos, onStepStarted: AnimationEvents.TriggerRimTouched);
                        path.Steps.Add(shootToFrailFrame);
                        ShootPathStep backboardToGround = new ShootPathStep(_shootSettings.BounceToGroundForce, _shootSettings.BounceToGroundDuration, groundFailAreaTargetPos);
                        path.Steps.Add(backboardToGround);
                        break;
                }

                break;
        }

        return path;
    }

    private Vector3 GetRandomOffsetInsideCircle(Vector3 originalPos, float radius, bool yUp = true)
    {
        Vector2 randomInsideCircle = UnityEngine.Random.insideUnitCircle * radius;
        Vector3 newPosInRadius = new Vector3(originalPos.x + randomInsideCircle.x, originalPos.y + randomInsideCircle.y, originalPos.z);
        if (yUp)
        {
            newPosInRadius = new Vector3(originalPos.x + randomInsideCircle.x, originalPos.y, originalPos.z + randomInsideCircle.y);
        }

        return newPosInRadius;
    }

    private void OnShootPositionUpdated()
    {
        Invoke(nameof(ResetBall), 0.01f);
    }

    private void ResetBall()
    {
        _ballBody.velocity = Vector3.zero;
        _ballBody.isKinematic = true;
        _ballBody.transform.position = _shootStartPos.position;
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

    // If there's any event to call when steps starts (i.e. loop frame bounce animation when hit)
    public Action OnStepStarted;

    // Check if this was the last step the let the next shoot timer know when to start and to handle extra ball physics
    public bool LastBounce;

    public ShootPathStep(float power, float duration, Vector3 target, bool lastBounce = false, Action onStepStarted = null)
    {
        Power = power;
        Duration = duration;
        Target = target;
        LastBounce = lastBounce;
        OnStepStarted = onStepStarted;
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