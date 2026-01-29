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
    [SerializeField] private Transform _shootStartPos;
    
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
    
    private void Shoot(float shootVelocity, bool isHumanPlayer)
    {
        ShootResult result = GetShootResult(shootVelocity, isHumanPlayer);
        Debug.Log($"ShootResult | Type: {result.Type} | Accuracy: {result.Accuracy} | Strength: {result.Strength}");
        ComputeShoot(GetShootPath(result.Type, _position, result.Accuracy, result.Strength));
    }

    private ShootResult GetShootResult(float shootVelocity, bool isHumanPlayer)
    {
        ShootType type = ShootType.Direct;
        ShootAccuracy accuracy = ShootAccuracy.Fail;

        ShootVelocityConfigByType directVelocityConfig = RuntimeServices.GameModeService.GameModeSettings.GetShootVelocityConfig(ShootType.Direct);
        ShootVelocityConfigByType backboardVelocityConfig = RuntimeServices.GameModeService.GameModeSettings.GetShootVelocityConfig(ShootType.Backboard);
        
        if (shootVelocity >= directVelocityConfig.Min && shootVelocity <= directVelocityConfig.Max)
        {
            type = ShootType.Direct;
            accuracy = ShootAccuracy.Perfect;
        }
        else if (shootVelocity >= backboardVelocityConfig.Min && shootVelocity <= backboardVelocityConfig.Max)
        {
            type = ShootType.Backboard;
            accuracy = ShootAccuracy.Accurate;
        }
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

    private ShootVelocityType GetShootVelocityType(float shootVelocity)
    {
        float lowThreshold = GameModeEnv.MAX_SHOOT_VELOCITY / 3.0f;
        float midThreshold = GameModeEnv.MAX_SHOOT_VELOCITY - lowThreshold;
        if (shootVelocity < lowThreshold) return ShootVelocityType.Weak;
        if (shootVelocity < midThreshold) return ShootVelocityType.Medium;
        return ShootVelocityType.Strong;
    }

    private void ComputeShoot(ShootPath path)
    {
        if (path.Steps == null || path.Steps.Count == 0)
            return;

        GameModeEvents.TriggerShootTargetSet(path.Steps[0].Target);

        ExecuteStep(path, 0);
    }

    private void ExecuteStep(ShootPath path, int index)
    {
        if (!_lastBounceFound)
        {
            _lastBouncePosition = _playerTransform.transform.position;
        }
        
        if (index >= path.Steps.Count)
        {
            EnablePhysics(_lastBouncePosition);
            
            GameModeEvents.TriggerShootCompleted(_currentShootResult);
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

    private void HandleShootStep(ShootPathStep step, Action onComplete, bool _firstStep)
    {
        if (step.LastBounce)
        {
            _lastBouncePosition = step.Target;
            _lastBounceFound = true;
        }
        
        Ease ease = _firstStep ? _shootEase : _bounceEase;  
        
        _ballBody.transform.DOJump(step.Target, step.Power, 1, step.Duration).SetEase(ease).OnComplete(()=>onComplete?.Invoke());
    }
    
    private void EnablePhysics(Vector3 lastBounceDirection)
    {
        _ballBody.isKinematic = false;
        _ballBody.AddForce((_ballBody.transform.position - lastBounceDirection).normalized + Vector3.down * _bounceForceMultiplier, ForceMode.Impulse);
    }
    
    private ShootPath GetShootPath(ShootType shootType, ShootDirection shootDirection, ShootAccuracy accuracyType, ShootVelocityType shootVelocityType)
    {
        ShootPath path = new ShootPath();

        Vector3 groundFailAreaTargetPos = Vector3.zero;

        Vector3 backboardTargetPos = GetRandomPointInMesh(RuntimeServices.TargetService.BackboardTarget.GetComponent<MeshFilter>().mesh, RuntimeServices.TargetService.BackboardTarget);
        Vector3 frameTargetPos = GetRandomPointInMesh(RuntimeServices.TargetService.FrameTarget.GetComponent<MeshFilter>().mesh, RuntimeServices.TargetService.FrameTarget);

        switch (shootVelocityType)
        {
            case ShootVelocityType.Strong:
                backboardTargetPos += new Vector3(0, _strongShootBackboardTargetYOffset, 0);
                break;
            
            case ShootVelocityType.Weak:
                backboardTargetPos -= new Vector3(0, _weakShootBackboardTargetYOffset, 0);
                break;
        }

        if (accuracyType == ShootAccuracy.Fail)
        {
            switch (shootType)
            {
                case ShootType.Direct:
                    switch (shootDirection)
                    {
                        case ShootDirection.Right:
                            groundFailAreaTargetPos = GetRandomPointInMesh(RuntimeServices.TargetService.RightDirectFailGroundTarget.GetComponent<MeshFilter>().mesh, RuntimeServices.TargetService.RightDirectFailGroundTarget);
                            break;
                
                        case ShootDirection.Left:
                            groundFailAreaTargetPos = GetRandomPointInMesh(RuntimeServices.TargetService.LeftDirectFailGroundTarget.GetComponent<MeshFilter>().mesh, RuntimeServices.TargetService.LeftDirectFailGroundTarget);
                            break;
                    }
                    break;
                
                case ShootType.Backboard:
                    groundFailAreaTargetPos = GetRandomPointInMesh(RuntimeServices.TargetService.BackboardFailGroundTarget.GetComponent<MeshFilter>().mesh, RuntimeServices.TargetService.BackboardFailGroundTarget);
                    break;
            }
        }
        
        switch (shootType)
        {
            case ShootType.Backboard:
                ShootPathStep baseStepToBackboard = new ShootPathStep(_shootSettings.ShootForce, _shootSettings.ShootDuration, backboardTargetPos, lastBounce: accuracyType == ShootAccuracy.Perfect);
                path.Steps.Add(baseStepToBackboard);
                
                switch (accuracyType)
                {
                    case ShootAccuracy.Perfect:
                        ShootPathStep shootToScore = new ShootPathStep(_shootSettings.BounceForce, _shootSettings.BounceDuration, RuntimeServices.TargetService.ScoreTarget.position);
                        path.Steps.Add(shootToScore);
                        break;
                    
                    case ShootAccuracy.Accurate:
                        ShootPathStep shootToRim = new ShootPathStep(_shootSettings.BounceForce, _shootSettings.BounceDuration, frameTargetPos, true, AnimationEvents.TriggerRimTouched);
                        path.Steps.Add(shootToRim);
                        ShootPathStep rimToScore = new ShootPathStep(_shootSettings.RimToScoreForce, _shootSettings.RimToScoreDuration, RuntimeServices.TargetService.ScoreTarget.position);
                        path.Steps.Add(rimToScore);
                        break;
                    
                    case ShootAccuracy.Fail:
                        ShootPathStep backboardToGround = new ShootPathStep(_shootSettings.BounceToGroundForce, _shootSettings.BounceToGroundDuration, groundFailAreaTargetPos);
                        path.Steps.Add(backboardToGround);
                        break;
                }
                break;
            
            case ShootType.Direct:
                switch (accuracyType)
                {
                    case ShootAccuracy.Perfect:
                        ShootPathStep shootToScore = new ShootPathStep(_shootSettings.ShootForce, _shootSettings.ShootDuration, RuntimeServices.TargetService.ScoreTarget.position);
                        path.Steps.Add(shootToScore);
                        break;
            
                    case ShootAccuracy.Accurate:
                        ShootPathStep shootToRim = new ShootPathStep(_shootSettings.ShootForce, _shootSettings.ShootDuration, frameTargetPos, true, AnimationEvents.TriggerRimTouched);
                        path.Steps.Add(shootToRim);
                        ShootPathStep rimToScore = new ShootPathStep(_shootSettings.RimToScoreForce, _shootSettings.RimToScoreDuration, RuntimeServices.TargetService.ScoreTarget.position);
                        path.Steps.Add(rimToScore);
                        break;
                    
                    case ShootAccuracy.Fail:
                        ShootPathStep shootToGround = new ShootPathStep(_shootSettings.ShootToGroundForce, _shootSettings.ShootToGroundDuration, groundFailAreaTargetPos);
                        path.Steps.Add(shootToGround);
                        break;
                }
                break;
        }
 
        return path;
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
    
    private Vector3 GetRandomPointInMesh(Mesh mesh, Transform owner)
    {
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;

        float[] cumulativeAreas = new float[triangles.Length / 3];
        float totalArea = 0f;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 a = vertices[triangles[i]];
            Vector3 b = vertices[triangles[i + 1]];
            Vector3 c = vertices[triangles[i + 2]];

            float area = Vector3.Cross(b - a, c - a).magnitude * 0.5f;
            totalArea += area;
            cumulativeAreas[i / 3] = totalArea;
        }

        float r = UnityEngine.Random.value * totalArea;
        int triIndex = Array.FindIndex(cumulativeAreas, a => a >= r) * 3;

        Vector3 v0 = vertices[triangles[triIndex]];
        Vector3 v1 = vertices[triangles[triIndex + 1]];
        Vector3 v2 = vertices[triangles[triIndex + 2]];

        return owner.TransformPoint(RandomPointInTriangle(v0, v1, v2));
    }
    
    private static Vector3 RandomPointInTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        float r1 = Mathf.Sqrt(UnityEngine.Random.value);
        float r2 = UnityEngine.Random.value;

        return (1 - r1) * a + r1 * (1 - r2) * b + r1 * r2 * c;
    }
}

public class ShootPath
{
    public List<ShootPathStep> Steps = new List<ShootPathStep>();
}

public class ShootPathStep
{
    public float Power;
    public float Duration;
    public Vector3 Target;
    public Action OnStepStarted;
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

public class ShootResult
{
    public ShootType Type;
    public ShootAccuracy Accuracy;
    public ShootVelocityType Strength;
    public bool IsHumanPlayer;
    
    public ShootResult(ShootType type, ShootAccuracy accuracy, ShootVelocityType strength, bool isHumanPlayer)
    {
        Type = type;
        Accuracy = accuracy;
        Strength = strength;
        IsHumanPlayer = isHumanPlayer;
    }
}
