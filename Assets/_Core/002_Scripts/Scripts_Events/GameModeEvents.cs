using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameModeEvents
{
    public delegate void OnScoresGeneratedHandler(ShootVelocityConfigByType directScoreLimits, ShootVelocityConfigByType backboardScoreLimits);
    public static event OnScoresGeneratedHandler OnVelocityTargetsGenerated;

    public delegate void OnShootAttemptHandler(float shootVelocity, bool isHumanPlayer);
    public static event OnShootAttemptHandler OnShootAttempt;

    public delegate void OnFirstShootTargetSetHandler(Vector3 shootTarget);
    public static event OnFirstShootTargetSetHandler OnFirstShootTargetSet;

    public delegate void OnShootCompletedHandler(ShootResult shootResult);

    public static event OnShootCompletedHandler OnShootCompleted;
    
    public delegate void OnScoreHandler(int score);
    public static event OnScoreHandler OnShootScore;

    public delegate void OnCallNewShootPositionHandler();

    public static event OnCallNewShootPositionHandler OnCallNewPosition;
    
    public delegate void OnShootPositionUpdatedHandler();
    public static event OnShootPositionUpdatedHandler OnShootPositionUpdated;
    
    public delegate void OnScoreUpdatedHandler(int score, bool isHumanPlayer);
    public static event OnScoreUpdatedHandler OnGlobalScoreUpdated;

    // --- Event Triggers ---
    
    /// <summary>
    /// Called when the correct velocity limits visuals are set on the slider
    /// </summary>
    public static void TriggerUpdateShootVelocityTargets(ShootVelocityConfigByType directScoreLimits, ShootVelocityConfigByType backboardScoreLimits)
    {
        OnVelocityTargetsGenerated?.Invoke(directScoreLimits, backboardScoreLimits);
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
    public static void TriggerFirstShootTargetSet(Vector3 shootTarget)
    {
        OnFirstShootTargetSet?.Invoke(shootTarget);
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
    /// <param name="score"></param>
    public static void TriggerShootScore(int score)
    {
        OnShootScore?.Invoke(score);
    }

    /// <summary>
    /// Called when the system is ready to call the next shot position
    /// </summary>
    public static void TriggerCallNewPosition()
    {
        OnCallNewPosition?.Invoke();
    }

    /// <summary>
    /// Called when the next shot position is set
    /// </summary>
    public static void TriggerShootPositionUpdated()
    {
        OnShootPositionUpdated?.Invoke();
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
}
