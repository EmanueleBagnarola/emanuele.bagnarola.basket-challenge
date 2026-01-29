using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreHandler : MonoBehaviour
{
    private int currentScore;
    
    private void Awake()
    {
        GameModeEvents.OnShootCompleted += OnShootCompleted;
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShootCompleted -= OnShootCompleted;
    }

    /// <summary>
    /// Based on shoot result evaluate the correct score, taking in consideration if the bonus score on backboard was active
    /// </summary>
    /// <param name="result"></param>
    private void OnShootCompleted(ShootResult result)
    {
        int basicScore = RuntimeServices.GameModeService.GameModeSettings.GetBasicScoreByAccuracy(result.Accuracy);
        Debug.Log($"SCORE: {basicScore}");

        int totalScore = basicScore;
        
        currentScore += totalScore;
        
        if(totalScore > 0)
            GameModeEvents.TriggerShootScore(totalScore);
        
        GameModeEvents.TriggerGlobalScoreUpdated(currentScore, result.IsHumanPlayer);
    }
}
