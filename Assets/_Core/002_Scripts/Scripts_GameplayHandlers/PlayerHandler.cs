using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{
    [SerializeField] private Transform _bodyTransform;
    [SerializeField] private bool _isHumanPlayer;
    
    [Header("Jump anim config")]
    [SerializeField] private float _jumpForce;
    [SerializeField] private Ease _jumpEase;

    private void Awake()
    {
        GameModeEvents.OnShootAttempt += OnShootAttempt;
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShootAttempt -= OnShootAttempt;
    }

    private void OnShootAttempt(float shootVelocity, bool isHumanPlayer)
    {
        if(_isHumanPlayer != isHumanPlayer)
            return;
        
        Jump();
    }

    [Button]
    public void Jump()
    {
        _bodyTransform.DOLocalJump(
            Vector3.zero, 
            _jumpForce, 
            1, 
            RuntimeServices.GameModeService.GameModeSettings.ShootSettings.ShootWaitTime * 2)
            .SetEase(_jumpEase);;
    }
}
