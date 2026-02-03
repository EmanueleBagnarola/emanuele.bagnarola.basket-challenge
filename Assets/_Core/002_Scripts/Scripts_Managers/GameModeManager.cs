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
        UpdateProgressionPhase(GameModeProgression.Early);
        UpdateGameModePhase(GameModePhase.Startup);
        UpdateShootPosition();
        StartCountdown();
    }

    private void GenerateShootVelocityTargets()
    {
        ShootVelocityConfigByType directVelocityConfig = _gameModeSettings.GetShootVelocityConfig(ShootType.Direct);
        ShootVelocityConfigByType backboardVelocityConfig = _gameModeSettings.GetShootVelocityConfig(ShootType.Backboard);
        GameModeEvents.TriggerUpdateShootVelocityTargets(directVelocityConfig, backboardVelocityConfig);
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
        UpdateGameModePhase(GameModePhase.Playing);
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
                    UpdateGameModePhase(GameModePhase.End);
                    break;
                
                case ShootPhase.Started:
                    UpdateGameModePhase(GameModePhase.WaitForEnd);
                    break;
            }
        }
    }

    private void UpdateGameModePhase(GameModePhase gameModePhase)
    {
        if (gameModePhase == GameModePhase.Playing)
        {
            _currentGameModeTimer = RuntimeServices.GameModeService.GameModeSettings.GameModeDuration;
            _gameModeTimerStarted = true;
        }
        
        GameModeEvents.TriggerGameModePhaseUpdated(gameModePhase);
    }
    
    private void UpdateProgressionPhase(GameModeProgression gameModeProgression)
    {
        RuntimeServices.GameModeService.GameModeProgression = gameModeProgression;
        GenerateShootVelocityTargets();
    }

    private void OnShootCompleted(ShootResult result)
    {
        if(RuntimeServices.GameModeService.GameModePhase == GameModePhase.WaitForEnd)
        {
            UpdateGameModePhase(GameModePhase.End);
            return;
        }        
        
        // Calculate score taking in consideration type, accuracy and if "special backboard phase" is active:
        // 3 points for "Perfect", 2 points for "Accurate"
        // if type is "Backboard" and special backboard phase is active:
        // based on game phase: early (4 points), mid (6 points), late (8 points)
        switch (result.Accuracy)
        {
            
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



