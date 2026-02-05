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
        RuntimeServices.GameModeService.Reset();
        RuntimeServices.GameModeService.GameModeSettings = _gameModeSettings;

        GameModeEvents.OnShootCompleted += OnShootCompleted;
        GameModeEvents.OnShootScore += OnShootScore;
        GameModeEvents.OnGlobalScoreUpdated += OnGlobalScoreUpdated;
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShootCompleted -= OnShootCompleted;
        GameModeEvents.OnShootScore -= OnShootScore;
        GameModeEvents.OnGlobalScoreUpdated -= OnGlobalScoreUpdated;
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
        UpdateShootPosition(true, true);
        UpdateShootPosition(true, false);
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
            AudioEvents.TriggerPlayAudioFX(AudioFXId.Countdown_Progress);
            
            yield return new WaitForSeconds(1f);

            _currentStartCountdownTimer--;
        }
        
        GameModeEvents.TriggerCountdownTick(_currentStartCountdownTimer);
        AudioEvents.TriggerPlayAudioFX(AudioFXId.Countdown_End);
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
        if(!_gameModeTimerStarted ||RuntimeServices.GameModeService.GameModeState == GameModeState.End)
            return;
        
        _currentGameModeTimer -= Time.deltaTime;

        RuntimeServices.GameModeService.Timer = _currentGameModeTimer;

        if (_currentGameModeTimer <= 0)
        {
            // Set the current phase temporarily in waiting state, waiting for players to end their shooting phases
            UpdateGameModeState(GameModeState.WaitForEnd);
            
            // If both players are not in shooting phase, then call the end game
            if (RuntimeServices.GameModeService.AIPlayerState.ShootPhase != PlayerShootPhase.Started
                && RuntimeServices.GameModeService.HumanPlayerState.ShootPhase != PlayerShootPhase.Started)
            {
                UpdateGameModeState(GameModeState.End);
            }
        }
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
        if(RuntimeServices.GameModeService.GameModeState == gameModeState)
            return;

        switch (gameModeState)
        {
            case GameModeState.Playing:
                // reset the total timer and enable the bool that lets timer count down
                _currentGameModeTimer = RuntimeServices.GameModeService.GameModeSettings.GameModeDuration;
                _gameModeTimerStarted = true;
                break;
            
            case GameModeState.End:
                CheckGameModeOutcome();
                StartCoroutine(CallRewardsPage());
                break;
        }
        
        RuntimeServices.GameModeService.GameModeState = gameModeState;
        
        GameModeEvents.TriggerGameModeStateUpdated(gameModeState);
    }
    
    private void CheckGamePhaseUpdate()
    {
        GameModePhase previousPhase = RuntimeServices.GameModeService.GameModePhase;
        GameModePhase newPhase = GetUpdatedGameModePhase();
        
        if(previousPhase == newPhase)
            return;
        
        // Check if game mode phase can change based on current timer
        UpdateGameModePhase(newPhase);
    }
    
    private void UpdateGameModePhase(GameModePhase gameModePhase)
    {
        RuntimeServices.GameModeService.GameModePhase = gameModePhase;
        Debug.Log($"UpdateGameModePhase: {RuntimeServices.GameModeService.GameModePhase}");

        GameModeEvents.TriggerGamePhaseUpdated(gameModePhase);
    }

    private void OnShootCompleted(ShootResult result)
    {
        // check if game phase needs to be updated based on current timer
        CheckGamePhaseUpdate();

        // reset the player shoot phase
        if (result.IsHumanPlayer)
        {
            if (RuntimeServices.GameModeService.GameModeState == GameModeState.WaitForEnd)
                RuntimeServices.GameModeService.HumanPlayerState.FinalShotExecuted = true;
            else
                RuntimeServices.GameModeService.HumanPlayerState.ShootPhase = PlayerShootPhase.WaitForShot;

        }
        else
        {
            if (RuntimeServices.GameModeService.GameModeState == GameModeState.WaitForEnd)
                RuntimeServices.GameModeService.HumanPlayerState.FinalShotExecuted = true;
            else
                RuntimeServices.GameModeService.AIPlayerState.ShootPhase = PlayerShootPhase.WaitForShot;
        }
        
        // after a fixed wait time, update next shoot position if the shot was successful (perfect or accurate)
        StartCoroutine(CallNextShootPosition(result.Accuracy == ShootAccuracy.Perfect || result.Accuracy == ShootAccuracy.Accurate, result.IsHumanPlayer));
    }

    private void OnShootScore(ShootResult result, int score)
    {
        if (score <= 0)
            return;
        
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

    private void OnGlobalScoreUpdated(int score, bool isHumanPlayer)
    {
        // if(!isHumanPlayer)
        //     return;
        //
        // if(RuntimeServices.GameModeService.GameModeState == GameModeState.WaitForEnd
        //    && ((RuntimeServices.GameModeService.HumanPlayerState.FinalShotExecuted
        //         && RuntimeServices.GameModeService.AIPlayerState.FinalShotExecuted) 
        //        || 
        //        RuntimeServices.GameModeService.HumanPlayerState.ShootPhase == PlayerShootPhase.WaitForShot 
        //        && RuntimeServices.GameModeService.AIPlayerState.ShootPhase == PlayerShootPhase.WaitForShot))
        // {
        //     UpdateGameModeState(GameModeState.End);
        // }
    }
    
    private IEnumerator CallNextShootPosition(bool changePosition, bool isHumanPlayer)
    {
        yield return new WaitForSeconds(_gameModeSettings.ShootSettings.NextShootWaitTime);
        
        // Update new shoot position
        UpdateShootPosition(changePosition, isHumanPlayer);
    }

    private void UpdateShootPosition(bool changePosition, bool isHumanPlayer)
    {
        GameModeEvents.TriggerResetShootPosition(changePosition, isHumanPlayer);
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
        UIEvents.TriggerShowNotification(NotificationType.BackboardBonus);
    }

    private void DisableBackboardBonus()
    {
        _backboardBonusIsOn = false;
        RuntimeServices.GameModeService.BackboardBonus = 0;
        GameModeEvents.TriggerBackboardBonus(false, -1);
    }

    private void CheckGameModeOutcome()
    {
        bool humanPlayerWin = RuntimeServices.GameModeService.HumanPlayerState.Score > RuntimeServices.GameModeService.AIPlayerState.Score;
        bool draw = RuntimeServices.GameModeService.HumanPlayerState.Score == RuntimeServices.GameModeService.AIPlayerState.Score;

        SetGameModeOutcome(draw ? GameModeOutcome.Draw : (humanPlayerWin ? GameModeOutcome.Win : GameModeOutcome.Lose));
    }

    private void SetGameModeOutcome(GameModeOutcome outcome)
    {
        RuntimeServices.GameModeService.GameModeOutcome = outcome;

        switch (outcome)
        {
            case GameModeOutcome.Win:
                AudioEvents.TriggerPlayAudioFX(AudioFXId.Win);
                break;
            
            case GameModeOutcome.Lose:
                AudioEvents.TriggerPlayAudioFX(AudioFXId.Lose);
                break;
            
            case GameModeOutcome.Draw:
                AudioEvents.TriggerPlayAudioFX(AudioFXId.Draw);
                break;
        }
    }
    
    private IEnumerator CallRewardsPage()
    {
        yield return new WaitForSeconds(_gameModeSettings.ShowRewardsPageWaitTime);
        
        GameModeEvents.TriggerShowRewards();
    }
}



