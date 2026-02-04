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

        int finalShootScore = basicScore;
        
        // if shot result was perfect and on backboard, use the backboard bonus (if was generated)
        if (result.Accuracy == ShootAccuracy.Perfect && result.Type == ShootType.Backboard && RuntimeServices.GameModeService.BackboardBonus > 0)
            finalShootScore = RuntimeServices.GameModeService.BackboardBonus;

        Debug.Log($"OnShootCompleted | shot score: {finalShootScore}");

        if (result.IsHumanPlayer)
            RuntimeServices.GameModeService.PlayerScore += finalShootScore;
        else
            RuntimeServices.GameModeService.AIScore += finalShootScore;
        
        if(finalShootScore > 0)
            GameModeEvents.TriggerShootScore(result, finalShootScore);
        
        GameModeEvents.TriggerGlobalScoreUpdated(result.IsHumanPlayer ? RuntimeServices.GameModeService.PlayerScore : RuntimeServices.GameModeService.AIScore, result.IsHumanPlayer);
    }
}
