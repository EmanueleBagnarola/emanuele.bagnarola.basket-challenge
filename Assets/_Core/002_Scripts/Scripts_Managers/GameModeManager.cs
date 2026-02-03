using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

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

    private bool _backboardBonusIsOn;
    
    private void Awake()
    {
        Application.targetFrameRate = 60;
        
        RuntimeServices.GameModeService.GameModeSettings = _gameModeSettings;

        GameModeEvents.OnShootCompleted += OnShootCompleted;
        GameModeEvents.OnShootScore += OnShootScore;
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShootCompleted -= OnShootCompleted;
        GameModeEvents.OnShootScore -= OnShootScore;
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
        UpdateShootPosition(true);
        StartCountdown();
    }

    /// <summary>
    /// Initialize the start game countdown
    /// </summary>
    private void StartCountdown()
    {
        StopCountdown();

        _currentStartCountdownTimer = _gameModeSettings.StartGameCountdown;
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
        
        // after a fixed wait time, update next shoot position if the shot was successful (perfect or accurate)
        StartCoroutine(CallNextShootPosition(result.Accuracy == ShootAccuracy.Perfect || result.Accuracy == ShootAccuracy.Accurate));
    }

    private void OnShootScore(ShootResult result, int score)
    {
        // check if a backboard bonus score can be generated
        if (!_backboardBonusIsOn)
        {
            // handle generation
            CheckBackboardBonus();
            return;
        }
        
        // if shot was perfect and on backboard, hide the current bonus
        if (result.Accuracy == ShootAccuracy.Perfect && result.Type == ShootType.Backboard)
        {
            DisableBackboardBonus();
        }
    }

    private IEnumerator CallNextShootPosition(bool changePosition)
    {
        yield return new WaitForSeconds(_gameModeSettings.NextShootWaitTime);
        
        // Update new shoot position
        UpdateShootPosition(changePosition);
    }

    private void UpdateShootPosition(bool changePosition)
    {
        GameModeEvents.TriggerResetShootPosition(changePosition);
    }

    private void CheckBackboardBonus()
    {
        int percentageValue = UnityEngine.Random.Range(1, 101);
        
        Debug.Log($"HandleBackboardBonus: {percentageValue}%");
        
        if (percentageValue <= _gameModeSettings.BackboardBonusProbability)
        {
            int backboardBonusScore = _gameModeSettings.GetRandomBackboardBonusScore();
            
            Debug.Log($"Show Backboard Bonus: {backboardBonusScore}");
            
            EnableBackboardBonus(backboardBonusScore);
        }
    }

    private void EnableBackboardBonus(int bonusScore)
    {
        _backboardBonusIsOn = true;
        RuntimeServices.GameModeService.BackboardBonus = bonusScore;
        GameModeEvents.TriggerBackboardBonus(true, bonusScore);
    }

    private void DisableBackboardBonus()
    {
        _backboardBonusIsOn = false;
        RuntimeServices.GameModeService.BackboardBonus = 0;
        GameModeEvents.TriggerBackboardBonus(false, -1);
    }
}



