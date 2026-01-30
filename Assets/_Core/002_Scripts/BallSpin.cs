using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BallSpin : MonoBehaviour
{
    [SerializeField] private Transform parentTransform;
    [SerializeField] private Vector3 _localSpinAxis;
    [SerializeField] private float degreesPerSecond = 0.5f;

    private Tween spinTween;

    private void Awake()
    {
        GameModeEvents.OnShootPositionUpdated += OnShootPositionUpdated;
        GameModeEvents.OnFirstShootTargetSet += OnFirstShootTargetSet;
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShootPositionUpdated -= OnShootPositionUpdated;
        GameModeEvents.OnFirstShootTargetSet -= OnFirstShootTargetSet;
    }

    private void OnShootPositionUpdated()
    {
        StopSpin();
    }

    private void OnFirstShootTargetSet(Vector3 shootTarget)
    {
        SetParentRotationTarget(shootTarget);
    }

    private void SetParentRotationTarget(Vector3 target)
    {
        parentTransform.DOLookAt(target, 0.15f, AxisConstraint.Y);
        StartSpin();
    }

    [Button]
    public void StartSpin()
    {
        StopSpin();
        
        spinTween = DOVirtual.Float(
                0f,
                1f,
                1f,
                t =>
                {
                    transform.Rotate(_localSpinAxis.normalized, degreesPerSecond * Time.deltaTime, Space.Self);
                }
            )
            .SetLoops(-1)
            .SetEase(Ease.Linear);
    }

    public void StopSpin()
    {
        spinTween?.Kill();
        spinTween = null;
    }
}
