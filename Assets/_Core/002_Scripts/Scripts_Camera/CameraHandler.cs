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
    [SerializeField] private CinemachineBrain _brainCamera;
    [SerializeField] private ShootSettings _shootSettings;
    
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

        InitializeShootBlendTime();
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShootAttempt -= OnShootAttempt;
        GameModeEvents.OnShootCompleted -= OnShootCompleted;
        GameModeEvents.OnShootPositionUpdated -= OnShootPositionUpdated;
        GameModeEvents.OnGameModeStateUpdated -= OnGameStateUpdated;
    }

    [Button]
    public void InitializeShootBlendTime()
    {
        _brainCamera.m_CustomBlends.m_CustomBlends[0].m_Blend.m_Time = _shootSettings.ShootDuration;
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
        
        Invoke(nameof(HandleShootCamera), RuntimeServices.GameModeService.GameModeSettings.ShootSettings.ShootWaitTime);
    }

    private void OnShootPositionUpdated(bool _isHumanPlayer)
    {
        if(RuntimeServices.GameModeService.GameModeState != GameModeState.Playing || !_isHumanPlayer)
            return;
        
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
        if(!result.IsHumanPlayer)
            return;
        
        if(result.Accuracy != ShootAccuracy.Perfect || result.Type == ShootType.Backboard)
            return;
        
        _shakeController.Shake(_shakeIntensity, _shakeDuration);
    }
}
