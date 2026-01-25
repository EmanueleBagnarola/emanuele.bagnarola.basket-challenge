using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameModeEvents
{
    public delegate void OnScoresGeneratedHandler(ScoreInfo directScoreLimits, ScoreInfo backboardScoreLimits);
    public static OnScoresGeneratedHandler OnScoresGenerated;
    
    public delegate void OnThrowAttemptHandler(float score);
    public static OnThrowAttemptHandler OnThrowAttempt;
}
