using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIHandler : MonoBehaviour
{
    [SerializeField] private float _minShotVelocity;
    [SerializeField] private float _maxShotVelocity;
    [SerializeField] private float _nextShotWaitTime;

    private Coroutine _shootAttemptCoroutine;
    
    private void Awake()
    {
        GameModeEvents.OnGameModeStateUpdated += OnGameModeStateUpdated;
        GameModeEvents.OnShootPositionUpdated += OnShootPositionUpdated;
    }

    private void OnDestroy()
    {
        GameModeEvents.OnGameModeStateUpdated -= OnGameModeStateUpdated;
        GameModeEvents.OnShootPositionUpdated -= OnShootPositionUpdated;
    }

    private void OnGameModeStateUpdated(GameModeState gameModeState)
    {
        switch (gameModeState)
        {
            case GameModeState.Playing:
                _shootAttemptCoroutine = StartCoroutine(ShootAttempt(_nextShotWaitTime));
                break;
        }
    }

    private void OnShootPositionUpdated(bool isHumanPlayer)
    {
        if(isHumanPlayer)
            return;

        if(RuntimeServices.GameModeService.GameModeState != GameModeState.Playing)
            return;
        
        if (RuntimeServices.GameModeService.GameModeState == GameModeState.End)
        {
            StopCoroutine(_shootAttemptCoroutine);
            return;
        }
        
        _shootAttemptCoroutine = StartCoroutine(ShootAttempt(_nextShotWaitTime));
    }
    
    private IEnumerator ShootAttempt(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        
        GameModeEvents.TriggerShootAttempt(UnityEngine.Random.Range(_minShotVelocity, _maxShotVelocity), false);
    }
}
