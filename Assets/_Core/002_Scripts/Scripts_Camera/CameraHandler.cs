using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private CinemachineVirtualCamera _baseCamera;
    [SerializeField] private CinemachineVirtualCamera _throwCamera;
    
    [Header("Shake settings")]
    [SerializeField] private CinemachineShake _shakeController;
    [SerializeField] private float _shakeIntensity;
    [SerializeField] private float _shakeDuration;

    private void OnEnable()
    {
        GameModeEvents.OnThrowAttempt += OnThrowAttempt;
        GameModeEvents.OnScore += OnScore;
    }

    private void OnDisable()
    {
        GameModeEvents.OnThrowAttempt -= OnThrowAttempt;
        GameModeEvents.OnScore -= OnScore;
    }

    [Button]
    public void OnThrowCamera()
    {
        _baseCamera.Priority = 0;
        _throwCamera.Priority = 1;
    }

    [Button]
    public void ResetCameras()
    {
        _baseCamera.Priority = 1;
        _throwCamera.Priority = 0;
    }

    private void OnThrowAttempt(float throwScore)
    {
        OnThrowCamera();
    }

    private void OnScore(int score, ThrowResult result)
    {
        if(result.Accuracy != ThrowAccuracy.Perfect)
            return;
        
        _shakeController.Shake(_shakeIntensity, _shakeDuration);
    }
}
