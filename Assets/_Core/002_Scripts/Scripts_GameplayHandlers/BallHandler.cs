using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BallHandler : MonoBehaviour
{
    [SerializeField] private bool _isHumanPlayerBall;

    [Header("Anim config")]
    [SerializeField] private Transform _spinContainer;
    [SerializeField] private Vector3 _localSpinAxis;
    [SerializeField] private float _degreesPerSecond = 400f;

    [Header("Fireball config")]
    [SerializeField] private GameObject _fireballFx;

    private Tween spinTween;

    private void Awake()
    {
        GameModeEvents.OnShootPositionUpdated += OnShootPositionUpdated;
        GameModeEvents.OnFirstShootTargetSet += OnFirstShootTargetSet;
        GameModeEvents.OnSetFireballScoreActive += OnSetFireballScoreActive;
    }

    private void Start()
    {
        ShowFireballFx(false);
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShootPositionUpdated -= OnShootPositionUpdated;
        GameModeEvents.OnFirstShootTargetSet -= OnFirstShootTargetSet;
        GameModeEvents.OnSetFireballScoreActive -= OnSetFireballScoreActive;
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
        transform.DOLookAt(target, 0.15f, AxisConstraint.Y);
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
                    _spinContainer.Rotate(_localSpinAxis.normalized, _degreesPerSecond * Time.deltaTime, Space.Self);
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

    private void OnSetFireballScoreActive(bool active, bool isHumanPlayer)
    {
        if(_isHumanPlayerBall != isHumanPlayer)
            return;

        ShowFireballFx(active);
    }

    private void ShowFireballFx(bool show)
    {
        _fireballFx.SetActive(show);
    }
}
