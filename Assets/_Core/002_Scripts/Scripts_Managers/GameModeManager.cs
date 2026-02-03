using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    // [Header("General config")]
    // [SerializeField] private int _minScoresDistance;
    
    [Header("Score config")]
    [SerializeField] private GameModeSettings _gameModeSettings;
    
    [Header("Gameplay config")]
    [SerializeField] private Transform _shootRangeCenter;
    [SerializeField, NonReorderable] private List<ShootRange> _shootRangesByPhase; //NonReorderable attribute added to fix the editor serialized class visualization but

    private ShootRange _currentShootRange;

    private int _currentStartCountdownTimer;
    private Coroutine _countdownRoutine;

    private bool _gameModeTimerStarted;
    private float _currentGameModeTimer;

    private GameModePhase _currentGameModePhase;
    
    private void Awake()
    {
        Application.targetFrameRate = 60;
        
        RuntimeServices.GameModeService.GameModeSettings = _gameModeSettings;

        GameModeEvents.OnShootCompleted += OnShootCompleted;
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShootCompleted -= OnShootCompleted;
    }

    private void Start()
    {
        StartGameMode();
    }

    private void Update()
    {
        UpdateGameModeTimer();
    }

    private void StartGameMode()
    {
        UpdateGameModePhase(GameModePhase.Early);
        UpdateGameModeState(GameModeState.Startup);
        UpdateShootPosition();
        StartCountdown();
    }

    /// <summary>
    /// Initialize the start game countdown
    /// </summary>
    private void StartCountdown()
    {
        StopCountdown();

        _currentStartCountdownTimer = _gameModeSettings.StartGameCountdown;
        GameModeEvents.TriggerCountdownTick(_currentStartCountdownTimer); // Call the first countdown tick 
        _countdownRoutine = StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        while (_currentStartCountdownTimer > 0)
        {
            Debug.Log($"Countdown: {_currentStartCountdownTimer}");
            GameModeEvents.TriggerCountdownTick(_currentStartCountdownTimer);
            
            yield return new WaitForSeconds(1f);

            _currentStartCountdownTimer--;
        }
        
        GameModeEvents.TriggerCountdownTick(_currentStartCountdownTimer);
        UpdateGameModeState(GameModeState.Playing);
        Debug.Log("START");
    }

    private void StopCountdown()
    {
        if (_countdownRoutine != null)
        {
            StopCoroutine(_countdownRoutine);
            _countdownRoutine = null;
        }
    }
    

    /// <summary>
    /// Updates the timer and calls the end game phase when conditions are met (shot still in progress or performed)
    /// </summary>
    private void UpdateGameModeTimer()
    {
        if(!_gameModeTimerStarted)
            return;
        
        _currentGameModeTimer -= Time.deltaTime;

        RuntimeServices.GameModeService.Timer = _currentGameModeTimer;

        if (_currentGameModeTimer <= 0)
        {
            switch (RuntimeServices.GameModeService.ShootPhase)
            {
                case ShootPhase.Completed:
                    UpdateGameModeState(GameModeState.End);
                    break;
                
                case ShootPhase.Started:
                    UpdateGameModeState(GameModeState.WaitForEnd);
                    break;
            }
        }

        // Call phase update if changed
        GameModePhase previousPhase = _currentGameModePhase;
        
        _currentGameModePhase = GetUpdatedGameModePhase();
        
        if(_currentGameModePhase != previousPhase)
            UpdateGameModePhase(_currentGameModePhase);
    }
    
    /// <summary>
    /// Get the game mode phase based on current timer
    /// </summary>
    /// <returns></returns>
    private GameModePhase GetUpdatedGameModePhase()
    {
        float lateThreshold = RuntimeServices.GameModeService.GameModeSettings.GameModeDuration / 3.0f;
        float midThreshold = RuntimeServices.GameModeService.GameModeSettings.GameModeDuration - lateThreshold;
        
        if (_currentGameModeTimer < lateThreshold) 
            return GameModePhase.Late;
        
        if (_currentGameModeTimer < midThreshold) 
            return GameModePhase.Mid;
        
        return GameModePhase.Early;
    }

    private void UpdateGameModeState(GameModeState gameModeState)
    {
        if (gameModeState == GameModeState.Playing)
        {
            _currentGameModeTimer = RuntimeServices.GameModeService.GameModeSettings.GameModeDuration;
            _gameModeTimerStarted = true;
        }
        
        GameModeEvents.TriggerGameModeStateUpdated(gameModeState);
    }
    
    private void UpdateGameModePhase(GameModePhase gameModePhase)
    {
        RuntimeServices.GameModeService.GameModePhase = gameModePhase;
        Debug.Log($"UpdateGameModePhase: {RuntimeServices.GameModeService.GameModePhase}");

        GameModeEvents.TriggerGamePhaseUpdated(gameModePhase);
    }

    private void OnShootCompleted(ShootResult result)
    {
        if(RuntimeServices.GameModeService.GameModeState == GameModeState.WaitForEnd)
        {
            UpdateGameModeState(GameModeState.End);
            return;
        }        
        
        // Update game phase based on current timer

        // after a fixed wait time, update next shoot position
        StartCoroutine(CallNextShootPosition());
    }

    private IEnumerator CallNextShootPosition()
    {
        yield return new WaitForSeconds(_gameModeSettings.NextShootWaitTime);
        
        // Update new shoot position
        UpdateShootPosition();
    }

    private void UpdateShootPosition()
    {
        GameModeEvents.TriggerCallNewPosition();
    }
}



