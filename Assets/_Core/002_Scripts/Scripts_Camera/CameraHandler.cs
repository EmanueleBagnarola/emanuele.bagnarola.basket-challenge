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
    [SerializeField] private CinemachineVirtualCamera _endCamera;
    
    [Header("Shake settings")]
    [SerializeField] private CinemachineShake _shakeController;
    [SerializeField] private float _shakeIntensity;
    [SerializeField] private float _shakeDuration;

    private void Start()
    {
        ResetCameras();
    }

    private void Awake()
    {
        GameModeEvents.OnShootAttempt += OnShootAttempt;
        GameModeEvents.OnShootCompleted += OnShootCompleted;
        GameModeEvents.OnShootPositionUpdated += OnShootPositionUpdated;
        GameModeEvents.OnGameModeStateUpdated += OnGameStateUpdated;
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShootAttempt -= OnShootAttempt;
        GameModeEvents.OnShootCompleted -= OnShootCompleted;
        GameModeEvents.OnShootPositionUpdated -= OnShootPositionUpdated;
        GameModeEvents.OnGameModeStateUpdated -= OnGameStateUpdated;
    }

    /// <summary>
    /// Change cameras priority to enable blending from base camera to shot camera
    /// </summary>
    private void HandleShootCamera()
    {
        _baseCamera.Priority = 0;
        _shootCamera.Priority = 1;
    }

    private void HandleEndCamera()
    {
        _baseCamera.Priority = 0;
        _shootCamera.Priority = 0;
        _endCamera.Priority = 1;
    }

    /// <summary>
    /// Reset cameras priority 
    /// </summary>
    private void ResetCameras()
    {
        _baseCamera.Priority = 1;
        _shootCamera.Priority = 0;
        _endCamera.Priority = 0;
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

    private void OnGameStateUpdated(GameModeState gameModeState)
    {
        switch (gameModeState)
        {
            case GameModeState.End:
                HandleEndCamera();
                break;
        }
    }

    /// <summary>
    /// When the shot is completed, decide if shake can be triggered
    /// </summary>
    /// <param name="result"></param>
    private void OnShootCompleted(ShootResult result)
    {
        if(result.Accuracy != ShootAccuracy.Perfect || result.Type == ShootType.Backboard)
            return;
        
        _shakeController.Shake(_shakeIntensity, _shakeDuration);
    }
}
