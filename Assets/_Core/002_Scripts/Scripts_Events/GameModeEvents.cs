using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameModeEvents
{
    public delegate void OnScoresGeneratedHandler(ShootVelocityConfigByType directScoreLimits, ShootVelocityConfigByType backboardScoreLimits);
    public static event OnScoresGeneratedHandler OnVelocityTargetsGenerated;
    
    public delegate void OnShootAttemptHandler(float shootVelocity);
    public static event OnShootAttemptHandler OnShootAttempt;

    public delegate void OnShootTargetSetHandler(Vector3 shootTarget);
    public static event OnShootTargetSetHandler OnShootTargetSet;

    public delegate void OnShootCompletedHandler(ShootResult shootResult);

    public static event OnShootCompletedHandler OnShootCompleted;
    
    public delegate void OnScoreHandler(int score);
    public static event OnScoreHandler OnScore;

    public delegate void OnCallNewShootPositionHandler();

    public static event OnCallNewShootPositionHandler OnCallNewPosition;
    
    public delegate void OnShootPositionUpdatedHandler();
    public static event OnShootPositionUpdatedHandler OnShootPositionUpdated;

    // --- Event Triggers ---
    
    public static void TriggerUpdateShootVelocityTargets(ShootVelocityConfigByType directScoreLimits, ShootVelocityConfigByType backboardScoreLimits)
    {
        OnVelocityTargetsGenerated?.Invoke(directScoreLimits, backboardScoreLimits);
    }

    public static void TriggerShootAttempt(float shootVelocity)
    {
        OnShootAttempt?.Invoke(shootVelocity);
    }

    public static void TriggerShootTargetSet(Vector3 shootTarget)
    {
        OnShootTargetSet?.Invoke(shootTarget);
    }

    public static void TriggerShootCompleted(ShootResult result)
    {
        OnShootCompleted?.Invoke(result);
    }

    public static void TriggerScore(int score)
    {
        OnScore?.Invoke(score);
    }

    public static void TriggerCallNewPosition()
    {
        OnCallNewPosition?.Invoke();
    }

    public static void TriggerShootPositionUpdated()
    {
        OnShootPositionUpdated?.Invoke();
    }
}
