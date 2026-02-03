using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreHandler : MonoBehaviour
{
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
        Debug.Log($"OnShootCompleted | shot score: {basicScore}");

        int finalShootScore = basicScore;
        
        // Calculate score taking in consideration type, accuracy and if "special backboard phase" is active:
        // 3 points for "Perfect", 2 points for "Accurate"
        // if type is "Backboard" and special backboard phase is active:
        // based on game phase: early (4 points), mid (6 points), late (8 points)
        switch (result.Accuracy)
        {
            
        }

        if (result.IsHumanPlayer)
            RuntimeServices.GameModeService.PlayerScore += finalShootScore;
        else
            RuntimeServices.GameModeService.AIScore += finalShootScore;
        
        if(finalShootScore > 0)
            GameModeEvents.TriggerShootScore(finalShootScore);
        
        GameModeEvents.TriggerGlobalScoreUpdated(result.IsHumanPlayer ? RuntimeServices.GameModeService.PlayerScore : RuntimeServices.GameModeService.AIScore, result.IsHumanPlayer);
    }
}
