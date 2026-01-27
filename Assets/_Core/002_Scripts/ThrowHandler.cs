using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class ThrowHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _ballTransform;
    [SerializeField] private Rigidbody _ballBody;
    [SerializeField] private Transform _start;
    [SerializeField] private Transform _rim;
    
    [Header("Throw Settings")]
    [Header("Base - Throw")]
    [SerializeField] private float _throwForce = 1.2f;
    [SerializeField] private float _throwDuration = 0.8f;
    [Header("Accurate - Backboard to frame")]
    [SerializeField] private float _bounceForce = 0.3f;
    [SerializeField] private float _bounceDuration = 0.4f;
    [Header("Accurate - Frame to loop")]
    [SerializeField] private float _rimToScoreForce = 0.2f;
    [SerializeField] private float _rimToScoreDuration = 0.4f;
    [SerializeField] private float _rimBounceOffset = 0.12f;
    [Header("Fail - Backboard to ground")]
    [SerializeField] private float _bounceToGroundForce = 1.6f;
    [SerializeField] private float _bounceToGroundDuration = 0.6f;
    [Header("Fail - Throw to ground")]
    [SerializeField] private float _throwToGroundForce;
    [SerializeField] private float _throwToGroundDuration;

    [Header("Target Settings")]
    [SerializeField] private float _strongThrowBackboardTargetYOffset;
    [SerializeField] private float _weakThrowBackboardTargetYOffset;
    
    [Header("Runtime Settings")]
    [SerializeField] private ThrowPosition _position;
    [SerializeField] private float _accurateThreshold;

    private ThrowResult _currentThrowResult;
    
    private void OnEnable()
    {
        GameModeEvents.OnThrowAttempt += Throw;
    }

    private void OnDisable()
    {
        GameModeEvents.OnThrowAttempt -= Throw;
    }
    
    private void Throw(float score)
    {
        _ballBody.velocity = Vector3.zero;
        _ballBody.isKinematic = true;
        _ballBody.gameObject.transform.localPosition = Vector3.zero;
        
        _ballTransform.position = _start.position;

        ThrowResult result = GetThrowResult(score);
        Debug.Log($"Throw result | Type: {result.Type} | Accuracy: {result.Accuracy} | Strength: {result.Strength}");
        ComputeThrow(GetThrowPath(result.Type, _position, result.Accuracy, result.Strength));
    }

    private ThrowResult GetThrowResult(float throwScore)
    {
        ThrowType type = ThrowType.Direct;
        ThrowAccuracy accuracy = ThrowAccuracy.Fail;

        ScoreData currentScoreData = RuntimeServices.GameModeService.GameModeSettings.GetScoreData(RuntimeServices.GameModeService.CurrentPhase);
        
        if (throwScore >= currentScoreData.DirectScoreInfo.Min && throwScore <= currentScoreData.DirectScoreInfo.Max)
        {
            type = ThrowType.Direct;
            accuracy = ThrowAccuracy.Perfect;
        }
        else if (throwScore >= currentScoreData.BackboardScoreInfo.Min && throwScore <= currentScoreData.BackboardScoreInfo.Max)
        {
            type = ThrowType.Backboard;
            accuracy = ThrowAccuracy.Perfect;
        }
        else
        {
            if (Mathf.Abs(currentScoreData.DirectScoreInfo.Min - throwScore) <= _accurateThreshold
                || Mathf.Abs(currentScoreData.DirectScoreInfo.Max - throwScore) <= _accurateThreshold)
            {                    
                type = ThrowType.Direct;
                accuracy = ThrowAccuracy.Accurate;
            }
            else if (Mathf.Abs(currentScoreData.BackboardScoreInfo.Min - throwScore) <= _accurateThreshold
                     || Mathf.Abs(currentScoreData.BackboardScoreInfo.Max - throwScore) <= _accurateThreshold)
            {
                type = ThrowType.Backboard;
                accuracy = ThrowAccuracy.Accurate;
            }
            else
            {
                if (throwScore > currentScoreData.DirectScoreInfo.Max)
                {
                    type = ThrowType.Backboard;
                    accuracy = ThrowAccuracy.Fail;
                }
                else if (throwScore < currentScoreData.DirectScoreInfo.Min)
                {
                    type = ThrowType.Direct;
                    accuracy = ThrowAccuracy.Fail;
                }
            }
        }

        _currentThrowResult = new ThrowResult(type, accuracy, GetThrowStrength(throwScore));

        return _currentThrowResult;
    }

    private ThrowStrength GetThrowStrength(float throwScore)
    {
        float lowThreshold = GameModeEnv.MAX_THROW_SCORE / 3.0f;
        float midThreshold = GameModeEnv.MAX_THROW_SCORE - lowThreshold;
        if (throwScore < lowThreshold) return ThrowStrength.Weak;
        if (throwScore < midThreshold) return ThrowStrength.Medium;
        return ThrowStrength.Strong;
    }

    private void ComputeThrow(ThrowPath path)
    {
        if (path.Steps == null || path.Steps.Count == 0)
            return;

        ExecuteStep(path, 0);
    }

    private void ExecuteStep(ThrowPath path, int index)
    {
        if (index >= path.Steps.Count)
        {
            // Score
            GameModeEvents.TriggerScore(10, _currentThrowResult);
            
            EnablePhysics();
            return;
        }

        ThrowPathStep step = path.Steps[index];

        HandleThrowStep(step, () =>
        {
            ExecuteStep(path, index + 1);
        });

        float totalTime = 0.0f;

        foreach (var s in path.Steps)
        {
            totalTime += s.Duration;
        }

        //StartCoroutine(EnablePhysicsRoutine(totalTime));
    }

    private void HandleThrowStep(ThrowPathStep step, Action onComplete)
    {
        _ballTransform.DOJump(step.Target, step.Force, 1, step.Duration, false).SetEase(Ease.Linear).OnComplete(()=>onComplete?.Invoke());
    }

    private IEnumerator EnablePhysicsRoutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        
        EnablePhysics();
    }

    private void EnablePhysics()
    {
        _ballBody.isKinematic = false;
    }

    // private ThrowPath GetThrowPath(ThrowType throwType, ThrowPosition throwPosition, ThrowAccuracy accuracyType)
    // {
    //     ThrowPath path = new ThrowPath();
    //
    //     Vector3 backboardTargetPos = Vector3.zero;
    //     Vector3 groundFailAreaTargetPos = Vector3.zero;
    //     
    //     Vector3 randomLoopFramePos = GetRandomPointInMesh(_rim.GetComponent<MeshFilter>().mesh, _rim);
    //     randomLoopFramePos.y += _rimBounceOffset; // ball radius
    //
    //     switch (throwPosition)
    //     {
    //         case ThrowPosition.Right:
    //             if (accuracyType == ThrowAccuracy.Fail)
    //             {
    //                 backboardTargetPos = GetRandomPointInMesh(_rightBackboardFailArea.GetComponent<MeshFilter>().mesh, _rightBackboardFailArea);
    //                 groundFailAreaTargetPos = GetRandomPointInMesh(_rightGroundFailArea.GetComponent<MeshFilter>().mesh, _rightGroundFailArea);
    //             }
    //             else
    //             {
    //                 backboardTargetPos = GetRandomPointInMesh(_rightBackboardArea.GetComponent<MeshFilter>().mesh, _rightBackboardArea);
    //             }
    //             break;
    //             
    //         case ThrowPosition.Left:
    //             if (accuracyType == ThrowAccuracy.Fail)
    //             {
    //                 backboardTargetPos = GetRandomPointInMesh(_leftBackboardFailArea.GetComponent<MeshFilter>().mesh, _leftBackboardFailArea);
    //                 groundFailAreaTargetPos = GetRandomPointInMesh(_leftGroundFailArea.GetComponent<MeshFilter>().mesh, _leftGroundFailArea);
    //             }
    //             else
    //             {
    //                 backboardTargetPos = GetRandomPointInMesh(_leftBackboardArea.GetComponent<MeshFilter>().mesh, _leftBackboardArea);
    //             }
    //             break;
    //     }
    //
    //     switch (throwType)
    //     {
    //         case ThrowType.Backboard:
    //             ThrowPathStep baseStepToBackboard = new ThrowPathStep(_throwForce, _throwDuration, backboardTargetPos);
    //             path.Steps.Add(baseStepToBackboard);
    //             
    //             switch (accuracyType)
    //             {
    //                 case ThrowAccuracy.Perfect:
    //                     ThrowPathStep throwToScore = new ThrowPathStep(_bounceForce, _bounceDuration, _scoreTarget.position);
    //                     path.Steps.Add(throwToScore);
    //                     break;
    //         
    //                 case ThrowAccuracy.Accurate:
    //                     ThrowPathStep throwToRim = new ThrowPathStep(_bounceForce, _bounceDuration, randomLoopFramePos);
    //                     path.Steps.Add(throwToRim);
    //                     ThrowPathStep rimToScore = new ThrowPathStep(_rimToScoreForce, _rimToScoreDuration, _scoreTarget.position);
    //                     path.Steps.Add(rimToScore);
    //                     break;
    //                 
    //                 case ThrowAccuracy.Fail:
    //                     ThrowPathStep backboardToGround = new ThrowPathStep(_bounceToGroundForce, _bounceToGroundDuration, groundFailAreaTargetPos);
    //                     path.Steps.Add(backboardToGround);
    //                     break;
    //             }
    //             break;
    //         
    //         case ThrowType.Direct:
    //             switch (accuracyType)
    //             {
    //                 case ThrowAccuracy.Perfect:
    //                     ThrowPathStep throwToScore = new ThrowPathStep(_throwForce, _throwDuration, _scoreTarget.position);
    //                     path.Steps.Add(throwToScore);
    //                     break;
    //         
    //                 case ThrowAccuracy.Accurate:
    //                     ThrowPathStep throwToRim = new ThrowPathStep(_throwForce, _throwDuration, randomLoopFramePos);
    //                     path.Steps.Add(throwToRim);
    //                     ThrowPathStep rimToScore = new ThrowPathStep(_rimToScoreForce, _rimToScoreDuration, _scoreTarget.position);
    //                     path.Steps.Add(rimToScore);
    //                     break;
    //                 
    //                 case ThrowAccuracy.Fail:
    //                     ThrowPathStep throwToGround = new ThrowPathStep(_throwToGroundForce, _throwToGroundDuration, groundFailAreaTargetPos);
    //                     path.Steps.Add(throwToGround);
    //                     break;
    //             }
    //             break;
    //     }
    //
    //  
    //     
    //
    //     return path;
    // }
    
     private ThrowPath GetThrowPath(ThrowType throwType, ThrowPosition throwPosition, ThrowAccuracy accuracyType, ThrowStrength throwStrength)
    {
        ThrowPath path = new ThrowPath();

        Vector3 groundFailAreaTargetPos = Vector3.zero;

        Vector3 backboardTargetPos = GetRandomPointInMesh(RuntimeServices.TargetService.BackboardTarget.GetComponent<MeshFilter>().mesh, RuntimeServices.TargetService.BackboardTarget);
        Vector3 frameTargetPos = GetRandomPointInMesh(RuntimeServices.TargetService.FrameTarget.GetComponent<MeshFilter>().mesh, RuntimeServices.TargetService.FrameTarget);

        switch (throwStrength)
        {
            case ThrowStrength.Strong:
                backboardTargetPos += new Vector3(0, _strongThrowBackboardTargetYOffset, 0);
                break;
            
            case ThrowStrength.Weak:
                backboardTargetPos -= new Vector3(0, _weakThrowBackboardTargetYOffset, 0);
                break;
        }

        if (accuracyType == ThrowAccuracy.Fail)
        {
            switch (throwType)
            {
                case ThrowType.Direct:
                    switch (throwPosition)
                    {
                        case ThrowPosition.Right:
                            groundFailAreaTargetPos = GetRandomPointInMesh(RuntimeServices.TargetService.RightSideGroundTarget.GetComponent<MeshFilter>().mesh, RuntimeServices.TargetService.RightSideGroundTarget);
                            break;
                
                        case ThrowPosition.Left:
                            groundFailAreaTargetPos = GetRandomPointInMesh(RuntimeServices.TargetService.LeftSideGroundTarget.GetComponent<MeshFilter>().mesh, RuntimeServices.TargetService.LeftSideGroundTarget);
                            break;
                    }
                    break;
                
                case ThrowType.Backboard:
                    groundFailAreaTargetPos = GetRandomPointInMesh(RuntimeServices.TargetService.AdaptableGroundTarget.GetComponent<MeshFilter>().mesh, RuntimeServices.TargetService.AdaptableGroundTarget);
                    break;
            }
        }
        
        switch (throwType)
        {
            case ThrowType.Backboard:
                ThrowPathStep baseStepToBackboard = new ThrowPathStep(_throwForce, _throwDuration, backboardTargetPos);
                path.Steps.Add(baseStepToBackboard);
                
                switch (accuracyType)
                {
                    case ThrowAccuracy.Perfect:
                        ThrowPathStep throwToScore = new ThrowPathStep(_bounceForce, _bounceDuration, RuntimeServices.TargetService.ScoreTarget.position);
                        path.Steps.Add(throwToScore);
                        break;
            
                    case ThrowAccuracy.Accurate:
                        ThrowPathStep throwToRim = new ThrowPathStep(_bounceForce, _bounceDuration, frameTargetPos);
                        path.Steps.Add(throwToRim);
                        ThrowPathStep rimToScore = new ThrowPathStep(_rimToScoreForce, _rimToScoreDuration, RuntimeServices.TargetService.ScoreTarget.position);
                        path.Steps.Add(rimToScore);
                        break;
                    
                    case ThrowAccuracy.Fail:
                        ThrowPathStep backboardToGround = new ThrowPathStep(_bounceToGroundForce, _bounceToGroundDuration, groundFailAreaTargetPos);
                        path.Steps.Add(backboardToGround);
                        break;
                }
                break;
            
            case ThrowType.Direct:
                switch (accuracyType)
                {
                    case ThrowAccuracy.Perfect:
                        ThrowPathStep throwToScore = new ThrowPathStep(_throwForce, _throwDuration, RuntimeServices.TargetService.ScoreTarget.position);
                        path.Steps.Add(throwToScore);
                        break;
            
                    case ThrowAccuracy.Accurate:
                        ThrowPathStep throwToRim = new ThrowPathStep(_throwForce, _throwDuration, frameTargetPos);
                        path.Steps.Add(throwToRim);
                        ThrowPathStep rimToScore = new ThrowPathStep(_rimToScoreForce, _rimToScoreDuration, RuntimeServices.TargetService.ScoreTarget.position);
                        path.Steps.Add(rimToScore);
                        break;
                    
                    case ThrowAccuracy.Fail:
                        ThrowPathStep throwToGround = new ThrowPathStep(_throwToGroundForce, _throwToGroundDuration, groundFailAreaTargetPos);
                        path.Steps.Add(throwToGround);
                        break;
                }
                break;
        }

     
        
 
        return path;
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

public class ThrowPath
{
    public List<ThrowPathStep> Steps = new List<ThrowPathStep>();
}

public class ThrowPathStep
{
    public float Force;
    public float Duration;
    public Vector3 Target;

    public ThrowPathStep(float force, float duration, Vector3 target)
    {
        Force = force;
        Duration = duration;
        Target = target;
    }
}

public class ThrowResult
{
    public ThrowType Type;
    public ThrowAccuracy Accuracy;
    public ThrowStrength Strength;

    public ThrowResult(ThrowType type, ThrowAccuracy accuracy, ThrowStrength strength)
    {
        Type = type;
        Accuracy = accuracy;
        Strength = strength;
    }
}

[System.Serializable]
public class ThrowScoreRange
{
    public float Min;
    public float Max;
}

