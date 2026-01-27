using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameModeEvents
{
    public delegate void OnScoresGeneratedHandler(ScoreInfo directScoreLimits, ScoreInfo backboardScoreLimits);
    public static event OnScoresGeneratedHandler OnScoresGenerated;
    
    public delegate void OnThrowAttemptHandler(float throwScore);
    public static event OnThrowAttemptHandler OnThrowAttempt;
    
    public delegate void OnScoreHandler(int score, ThrowResult throwResult);
    public static event OnScoreHandler OnScore;

    // --- Event Triggers ---
    
    public static void TriggerScoreGenerated(ScoreInfo directScoreLimits, ScoreInfo backboardScoreLimits)
    {
        OnScoresGenerated?.Invoke(directScoreLimits, backboardScoreLimits);
    }

    public static void TriggerThrowAttempt(float throwScore)
    {
        OnThrowAttempt?.Invoke(throwScore);
    }

    public static void TriggerScore(int score, ThrowResult throwResult)
    {
        OnScore?.Invoke(score, throwResult);
    }
}
