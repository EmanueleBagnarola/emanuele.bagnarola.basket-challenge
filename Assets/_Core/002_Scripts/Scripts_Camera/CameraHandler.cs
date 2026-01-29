using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private CinemachineVirtualCamera _baseCamera;
    [SerializeField] private CinemachineVirtualCamera _shootCamera;
    
    [Header("Shake settings")]
    [SerializeField] private CinemachineShake _shakeController;
    [SerializeField] private float _shakeIntensity;
    [SerializeField] private float _shakeDuration;

    private void OnEnable()
    {
        GameModeEvents.OnShootAttempt += OnShootAttempt;
        GameModeEvents.OnShootCompleted += OnShootCompleted;
        GameModeEvents.OnShootPositionUpdated += OnShootPositionUpdated;
    }

    private void OnDisable()
    {
        GameModeEvents.OnShootAttempt -= OnShootAttempt;
        GameModeEvents.OnShootCompleted -= OnShootCompleted;
        GameModeEvents.OnShootPositionUpdated -= OnShootPositionUpdated;
    }

    private void HandleShootCamera()
    {
        _baseCamera.Priority = 0;
        _shootCamera.Priority = 1;
    }

    private void ResetCameras()
    {
        _baseCamera.Priority = 1;
        _shootCamera.Priority = 0;
    }

    private void OnShootAttempt(float shootVelocity, bool isHumanPlayer)
    {
        if(!isHumanPlayer)
            return;
        
        HandleShootCamera();
    }

    private void OnShootPositionUpdated()
    {
        ResetCameras();
    }

    private void OnShootCompleted(ShootResult result)
    {
        if(result.Accuracy != ShootAccuracy.Perfect)
            return;
        
        _shakeController.Shake(_shakeIntensity, _shakeDuration);
    }
}
