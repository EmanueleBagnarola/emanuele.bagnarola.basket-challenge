using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreHandler : MonoBehaviour
{
    private int currentPlayerScore;
    private int currentAIScore;
    
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
        int basicScore = RuntimeServices.GameModeService.GameModeSettings.GetBasicScoreByAccuracy(result.Accuracy, result.Type);
        Debug.Log($"SCORE: {basicScore}");

        int shootScore = basicScore;
        
        currentPlayerScore += shootScore;
        
        if(shootScore > 0)
            GameModeEvents.TriggerShootScore(shootScore);
        
        GameModeEvents.TriggerGlobalScoreUpdated(result.IsHumanPlayer ? currentPlayerScore : currentAIScore, result.IsHumanPlayer);

        if (result.IsHumanPlayer)
        {
            RuntimeServices.GameModeService.PlayerScore = currentPlayerScore;
        }
        else
        {
            RuntimeServices.GameModeService.AIScore = currentAIScore;
        }
    }
}
