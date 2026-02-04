using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BallSpin : MonoBehaviour
{
    [SerializeField] private Transform parentTransform;
    [SerializeField] private Vector3 _localSpinAxis;
    [SerializeField] private float _degreesPerSecond = 400f;
    [SerializeField] private bool _isHumanPlayerBall;

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

    private void OnShootPositionUpdated(bool isHumanPlayer)
    {
        if(_isHumanPlayerBall != isHumanPlayer)
            return;
        
        StopSpin();
    }

    private void OnFirstShootTargetSet(Vector3 shootTarget, bool isHumanPlayer)
    {
        if(_isHumanPlayerBall != isHumanPlayer)
            return;
        
        SetParentRotationTarget(shootTarget);
    }

    /// <summary>
    /// Set the parent relative rotation to face the first shoot target, in order to let the spin rotate on the correct axis
    /// </summary>
    /// <param name="target"></param>
    private void SetParentRotationTarget(Vector3 target)
    {
        parentTransform.DOLookAt(target, 0.15f, AxisConstraint.Y);
        StartSpin();
    }

    /// <summary>
    /// Simulate ball shot spinning
    /// </summary>
    private void StartSpin()
    {
        StopSpin();
        
        spinTween = DOVirtual.Float(
                0f,
                1f,
                1f,
                t =>
                {
                    transform.Rotate(_localSpinAxis.normalized, _degreesPerSecond * Time.deltaTime, Space.Self);
                }
            )
            .SetLoops(-1)
            .SetEase(Ease.Linear);
    }

    private void StopSpin()
    {
        spinTween?.Kill();
        spinTween = null;
    }
}
