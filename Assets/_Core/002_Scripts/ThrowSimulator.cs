using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class ThrowSimulator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _ballTransform;
    [SerializeField] private Rigidbody _ballBody;
    [SerializeField] private Transform _start;
    [SerializeField] private Transform _rim;
    
    [Header("Config")]
    [Header("Throw")]
    [SerializeField] private float _throwForce;
    [SerializeField] private float _throwDuration;
    [Header("Backboard to frame")]
    [SerializeField] private float _bounceForce;
    [SerializeField] private float _bounceDuration;
    [Header("Frame to loop")]
    [SerializeField] private float _rimToScoreForce = 0.1f;
    [SerializeField] private float _rimToScoreDuration = 0.4f;
    [SerializeField] private float _rimBounceOffset = 0.1f;
    [Header("Backboard to ground")]
    [SerializeField] private float _bounceToGroundForce;
    [SerializeField] private float _bounceToGroundDuration;
    
    [Header("Targets")]
    [SerializeField] private Transform _leftBackboardArea;
    [SerializeField] private Transform _rightBackboardArea;
    [SerializeField] private Transform _scoreTarget;
    [SerializeField] private Transform _leftBackboardFailArea;
    [SerializeField] private Transform _rightBackboardFailArea;
    [SerializeField] private Transform _leftGroundFailArea;
    [SerializeField] private Transform _rightGroundFailArea;
    
    [Header("Runtime Settings")]
    [SerializeField] private ThrowPosition _position;
    [SerializeField] private float _currentThrowScore;
    [SerializeField] private ThrowScoreRange _directScoreRange;
    [SerializeField] private ThrowScoreRange _backboardScoreRange;
    [SerializeField] private float _accurateThreshold;
    
    [Button]
    public void Throw()
    {
        _ballBody.velocity = Vector3.zero;
        _ballBody.isKinematic = true;
        _ballBody.gameObject.transform.localPosition = Vector3.zero;
        
        _ballTransform.position = _start.position;

        ThrowResult result = GetThrowResult(_currentThrowScore);
        Debug.Log($"Throw result | Type: {result.Type} | Accuracy: {result.Accuracy}");
        ComputeThrow(GetThrowPath(result.Type, _position, result.Accuracy));
    }

    private ThrowResult GetThrowResult(float _throwScore)
    {
        ThrowType type = ThrowType.Direct;
        ThrowAccuracy accuracy = ThrowAccuracy.Fail;

        if (_throwScore >= _directScoreRange.Min && _throwScore <= _directScoreRange.Max)
        {
            type = ThrowType.Direct;
            accuracy = ThrowAccuracy.Perfect;
        }
        else if (_throwScore >= _backboardScoreRange.Min && _throwScore <= _backboardScoreRange.Max)
        {
            type = ThrowType.Backboard;
            accuracy = ThrowAccuracy.Perfect;
        }
        else
        {
            if (Mathf.Abs(_directScoreRange.Min - _throwScore) <= _accurateThreshold
                || Mathf.Abs(_directScoreRange.Max - _throwScore) <= _accurateThreshold)
            {                    
                type = ThrowType.Direct;
                accuracy = ThrowAccuracy.Accurate;
            }
            else if (Mathf.Abs(_backboardScoreRange.Min - _throwScore) <= _accurateThreshold
                     || Mathf.Abs(_backboardScoreRange.Max - _throwScore) <= _accurateThreshold)
            {
                type = ThrowType.Backboard;
                accuracy = ThrowAccuracy.Accurate;
            }
            else
            {
                if (_throwScore > _directScoreRange.Max)
                {
                    type = ThrowType.Backboard;
                    accuracy = ThrowAccuracy.Fail;
                }
                else if (_throwScore < _directScoreRange.Min)
                {
                    type = ThrowType.Direct;
                    accuracy = ThrowAccuracy.Fail;
                }
            }
        }
        
        ThrowResult result = new ThrowResult(type, accuracy);

        return result;
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

    private ThrowPath GetThrowPath(ThrowType throwType, ThrowPosition throwPosition, ThrowAccuracy accuracyType)
    {
        ThrowPath path = new ThrowPath();

        Vector3 backboardTargetPos = Vector3.zero;
        Vector3 groundFailAreaTargetPos = Vector3.zero;
        
        Vector3 randomLoopFramePos = GetRandomPointInMesh(_rim.GetComponent<MeshFilter>().mesh, _rim);
        randomLoopFramePos.y += _rimBounceOffset; // ball radius

        switch (throwPosition)
        {
            case ThrowPosition.Right:
                if (accuracyType == ThrowAccuracy.Fail)
                {
                    backboardTargetPos = GetRandomPointInMesh(_rightBackboardFailArea.GetComponent<MeshFilter>().mesh, _rightBackboardFailArea);
                    groundFailAreaTargetPos = GetRandomPointInMesh(_rightGroundFailArea.GetComponent<MeshFilter>().mesh, _rightGroundFailArea);
                }
                else
                {
                    backboardTargetPos = GetRandomPointInMesh(_rightBackboardArea.GetComponent<MeshFilter>().mesh, _rightBackboardArea);
                }
                break;
                
            case ThrowPosition.Left:
                if (accuracyType == ThrowAccuracy.Fail)
                {
                    backboardTargetPos = GetRandomPointInMesh(_leftBackboardFailArea.GetComponent<MeshFilter>().mesh, _leftBackboardFailArea);
                    groundFailAreaTargetPos = GetRandomPointInMesh(_leftGroundFailArea.GetComponent<MeshFilter>().mesh, _leftGroundFailArea);
                }
                else
                {
                    backboardTargetPos = GetRandomPointInMesh(_leftBackboardArea.GetComponent<MeshFilter>().mesh, _leftBackboardArea);
                }
                break;
        }

        switch (throwType)
        {
            case ThrowType.Backboard:
                ThrowPathStep baseStepToBackboard = new ThrowPathStep(_throwForce, _throwDuration, backboardTargetPos);
                path.Steps.Add(baseStepToBackboard);
                
                switch (accuracyType)
                {
                    case ThrowAccuracy.Perfect:
                        ThrowPathStep throwToScore = new ThrowPathStep(_bounceForce, _bounceDuration, _scoreTarget.position);
                        path.Steps.Add(throwToScore);
                        break;
            
                    case ThrowAccuracy.Accurate:
                        ThrowPathStep throwToRim = new ThrowPathStep(_bounceForce, _bounceDuration, randomLoopFramePos);
                        path.Steps.Add(throwToRim);
                        ThrowPathStep rimToScore = new ThrowPathStep(_rimToScoreForce, _rimToScoreDuration, _scoreTarget.position);
                        path.Steps.Add(rimToScore);
                        break;
                    
                    case ThrowAccuracy.Fail:
                        ThrowPathStep throwToGround = new ThrowPathStep(_bounceToGroundForce, _bounceToGroundDuration, groundFailAreaTargetPos);
                        path.Steps.Add(throwToGround);
                        break;
                }
                break;
            
            case ThrowType.Direct:
                switch (accuracyType)
                {
                    case ThrowAccuracy.Perfect:
                        ThrowPathStep throwToScore = new ThrowPathStep(_throwForce, _throwDuration, _scoreTarget.position);
                        path.Steps.Add(throwToScore);
                        break;
            
                    case ThrowAccuracy.Accurate:
                        ThrowPathStep throwToRim = new ThrowPathStep(_bounceForce, _bounceDuration, randomLoopFramePos);
                        path.Steps.Add(throwToRim);
                        ThrowPathStep rimToScore = new ThrowPathStep(_rimToScoreForce, _rimToScoreDuration, _scoreTarget.position);
                        path.Steps.Add(rimToScore);
                        break;
                    
                    case ThrowAccuracy.Fail:
                        ThrowPathStep throwToGround = new ThrowPathStep(_throwForce, _throwDuration, groundFailAreaTargetPos);
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

    public ThrowResult(ThrowType type, ThrowAccuracy accuracy)
    {
        Type = type;
        Accuracy = accuracy;
    }
}

[System.Serializable]
public class ThrowScoreRange
{
    public float Min;
    public float Max;
}

public enum ThrowType
{
    Direct,
    Backboard,
}

public enum ThrowPosition
{
    Left,
    Right
}

public enum ThrowAccuracy
{
    Perfect,
    Accurate,
    Fail,
}
