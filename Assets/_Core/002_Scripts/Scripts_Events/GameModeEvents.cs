using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameModeEvents
{
    public delegate void OnScoresGeneratedHandler(ShootVelocityConfigByType directScoreLimits, ShootVelocityConfigByType backboardScoreLimits);
    public static event OnScoresGeneratedHandler OnVelocityTargetsGenerated;
    
    public delegate void OnShootAttemptHandler(float shootVelocity, bool isHumanPlayer);
    public static event OnShootAttemptHandler OnShootAttempt;

    /// <summary>
    /// Called when the first shoot target is selected during the shoot curve animation system
    /// </summary>
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
    
    public static void TriggerUpdateShootVelocityTargets(ShootVelocityConfigByType directScoreLimits, ShootVelocityConfigByType backboardScoreLimits)
    {
        OnVelocityTargetsGenerated?.Invoke(directScoreLimits, backboardScoreLimits);
    }

    public static void TriggerShootAttempt(float shootVelocity, bool isHumanPlayer)
    {
        OnShootAttempt?.Invoke(shootVelocity, isHumanPlayer);
    }

    public static void TriggerFirstShootTargetSet(Vector3 shootTarget)
    {
        OnFirstShootTargetSet?.Invoke(shootTarget);
    }

    public static void TriggerShootCompleted(ShootResult result)
    {
        OnShootCompleted?.Invoke(result);
    }

    public static void TriggerShootScore(int score)
    {
        OnShootScore?.Invoke(score);
    }

    public static void TriggerCallNewPosition()
    {
        OnCallNewPosition?.Invoke();
    }

    public static void TriggerShootPositionUpdated()
    {
        OnShootPositionUpdated?.Invoke();
    }

    public static void TriggerGlobalScoreUpdated(int score, bool isHumanPlayer)
    {
        OnGlobalScoreUpdated?.Invoke(score, isHumanPlayer);
    }
}
