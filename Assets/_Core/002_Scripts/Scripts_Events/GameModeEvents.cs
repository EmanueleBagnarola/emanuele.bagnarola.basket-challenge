using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameModeEvents
{
    public delegate void OnCountdownTickHandler(int currentCountdownTimer);
    public static event OnCountdownTickHandler OnCountdownTick;
    
    
    public delegate void OnGameModeStateUpdatedHandler(GameModeState gameModeState);
    public static event OnGameModeStateUpdatedHandler OnGameModeStateUpdated;
    
    
    public delegate void OnGameModePhaseUpdatedHandler(GameModePhase gameModePhase);
    public static event OnGameModePhaseUpdatedHandler OnGameModePhaseUpdated;
    
    
    public delegate void OnShootAttemptHandler(float shootVelocity, bool isHumanPlayer);
    public static event OnShootAttemptHandler OnShootAttempt;
    

    public delegate void OnFirstShootTargetSetHandler(Vector3 shootTarget, bool isHumanPlayer);
    public static event OnFirstShootTargetSetHandler OnFirstShootTargetSet;
    

    public delegate void OnShootCompletedHandler(ShootResult shootResult);
    public static event OnShootCompletedHandler OnShootCompleted;
    
    
    public delegate void OnScoreHandler(ShootResult result, int score);
    public static event OnScoreHandler OnShootScore;

    public delegate void OnResetShootPositionHandler(bool changePosition, bool isHumanPlaye);
    public static event OnResetShootPositionHandler OnResetShootPosition;
    
    
    public delegate void OnShootPositionUpdatedHandler(bool isHumanPlayer);
    public static event OnShootPositionUpdatedHandler OnShootPositionUpdated;


    public delegate void OnBackboardBonusHandler(bool show, int bonusScore);
    public static event OnBackboardBonusHandler OnBackboardBonus;
    
    
    public delegate void OnScoreUpdatedHandler(int score, bool isHumanPlayer);
    public static event OnScoreUpdatedHandler OnGlobalScoreUpdated;


    public delegate void OnShowRewardsHandler();
    public static event OnShowRewardsHandler OnShowRewards;

    // --- Event Triggers ---

    /// <summary>
    /// Called when start countdown timer is updated
    /// </summary>
    /// <param name="currentCountdownTimer"></param>
    public static void TriggerCountdownTick(int currentCountdownTimer)
    {
        OnCountdownTick?.Invoke(currentCountdownTimer);
    }

    public static void TriggerGameModeStateUpdated(GameModeState gameModeState)
    {
        OnGameModeStateUpdated?.Invoke(gameModeState);
    }

    public static void TriggerGamePhaseUpdated(GameModePhase gameModePhase)
    {
        OnGameModePhaseUpdated?.Invoke(gameModePhase);
    }

    /// <summary>
    /// Called when the shoot input is sent
    /// </summary>
    /// <param name="shootVelocity"></param>
    /// <param name="isHumanPlayer"></param>
    public static void TriggerShootAttempt(float shootVelocity, bool isHumanPlayer)
    {
        OnShootAttempt?.Invoke(shootVelocity, isHumanPlayer);
    }
    
    /// <summary>
    /// Called when the first shoot target is selected during the shoot curve animation system
    /// </summary>
    public static void TriggerFirstShootTargetSet(Vector3 shootTarget, bool isHumanPlayer)
    {
        OnFirstShootTargetSet?.Invoke(shootTarget, isHumanPlayer);
    }

    /// <summary>
    /// Called when the ball completed the curve path
    /// </summary>
    /// <param name="result"></param>
    public static void TriggerShootCompleted(ShootResult result)
    {
        OnShootCompleted?.Invoke(result);
    }

    /// <summary>
    /// Called when the current shot score is decided
    /// </summary>
    /// <param name="result"></param>
    /// <param name="score"></param>
    public static void TriggerShootScore(ShootResult result, int score)
    {
        OnShootScore?.Invoke(result, score);
    }

    /// <summary>
    /// Called when the system is ready to call the next shot position
    /// </summary>
    public static void TriggerResetShootPosition(bool changePosition, bool isHumanPlayer)
    {
        OnResetShootPosition?.Invoke(changePosition, isHumanPlayer);
    }

    /// <summary>
    /// Called when the next shot position is set
    /// </summary>
    public static void TriggerShootPositionUpdated(bool isHumanPlayer)
    {
        OnShootPositionUpdated?.Invoke(isHumanPlayer);
    }

    /// <summary>
    /// Called when the backboard bonus is generated
    /// </summary>
    /// <param name="show"></param>
    /// <param name="bonusScore"></param>
    public static void TriggerBackboardBonus(bool show, int bonusScore)
    {
        OnBackboardBonus?.Invoke(show, bonusScore);
    }
    
    /// <summary>
    /// Called to update the total score of the player
    /// </summary>
    /// <param name="score"></param>
    /// <param name="isHumanPlayer"></param>
    public static void TriggerGlobalScoreUpdated(int score, bool isHumanPlayer)
    {
        OnGlobalScoreUpdated?.Invoke(score, isHumanPlayer);
    }

    /// <summary>
    /// Called after game is over
    /// </summary>
    public static void TriggerShowRewards()
    {
        OnShowRewards?.Invoke();
    }
}
