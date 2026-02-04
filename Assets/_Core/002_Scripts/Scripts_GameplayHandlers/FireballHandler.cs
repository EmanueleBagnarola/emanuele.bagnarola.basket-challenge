using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballHandler : MonoBehaviour
{
    private void Awake()
    {
        GameModeEvents.OnShootScore += OnShootScore;
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShootScore -= OnShootScore;
    }

    private void Update()
    {
        EmptyFireballScores();
    }

    private void OnShootScore(ShootResult result, int score)
    {
        Debug.Log($"FireballHandler | OnShootScore | result {result.Accuracy}");
        
        if (result.IsHumanPlayer)
        {
            if (score <= 0)
            {
                // Disable fireball
                RuntimeServices.GameModeService.HumanPlayerState.FireballScore = 0;
                RuntimeServices.GameModeService.HumanPlayerState.FireballEnabled = false;
                GameModeEvents.TriggerSetFireballScoreActive(false, true);
                GameModeEvents.TriggerUpdateFireballScore(0, true);
                return;
            }
            
            // Register new fireball score
            float newPlayerFireballScore = RuntimeServices.GameModeService.HumanPlayerState.FireballScore + RuntimeServices.GameModeService.GameModeSettings.FireballPointValue;
            
            // Set score to player state
            RuntimeServices.GameModeService.HumanPlayerState.FireballScore = Mathf.Min(newPlayerFireballScore, RuntimeServices.GameModeService.GameModeSettings.FireballMaxScore);
            
            // Call fireball score updated event
            GameModeEvents.TriggerUpdateFireballScore(RuntimeServices.GameModeService.HumanPlayerState.FireballScore, true);

            if (!RuntimeServices.GameModeService.HumanPlayerState.FireballEnabled)
            {
                // Check if fireball score is max level
                if (RuntimeServices.GameModeService.HumanPlayerState.FireballScore >= RuntimeServices.GameModeService.GameModeSettings.FireballMaxScore)
                {
                    RuntimeServices.GameModeService.HumanPlayerState.FireballEnabled = true;
                    
                    // Call fireball enabled
                    GameModeEvents.TriggerSetFireballScoreActive(true, true);
                    
                    // Show notification
                    UIEvents.TriggerShowNotification(NotificationType.FireballActive);
                }
            }
        }
        else
        {
            if (score <= 0)
            {
                // Disable fireball
                RuntimeServices.GameModeService.AIPlayerState.FireballScore = 0;
                RuntimeServices.GameModeService.AIPlayerState.FireballEnabled = false;
                GameModeEvents.TriggerSetFireballScoreActive(false, false);
                GameModeEvents.TriggerUpdateFireballScore(0, false);
                return;
            }
            
            // Register new fireball score
            float newAIFireballScore = RuntimeServices.GameModeService.AIPlayerState.FireballScore + RuntimeServices.GameModeService.GameModeSettings.FireballPointValue;
            
            // Set score to player state
            RuntimeServices.GameModeService.AIPlayerState.FireballScore = Mathf.Min(newAIFireballScore, RuntimeServices.GameModeService.GameModeSettings.FireballMaxScore);
            
            // Call fireball score updated event
            GameModeEvents.TriggerUpdateFireballScore(RuntimeServices.GameModeService.AIPlayerState.FireballScore, false);

            if (!RuntimeServices.GameModeService.AIPlayerState.FireballEnabled)
            {
                // Check if fireball score is max level
                if (RuntimeServices.GameModeService.AIPlayerState.FireballScore >= RuntimeServices.GameModeService.GameModeSettings.FireballMaxScore)
                {
                    RuntimeServices.GameModeService.AIPlayerState.FireballEnabled = true;
                    
                    // Call fireball enabled
                    GameModeEvents.TriggerSetFireballScoreActive(true, false);
                }
            }
        }
    }

    private void EmptyFireballScores()
    {
        if (RuntimeServices.GameModeService.HumanPlayerState.FireballScore > 0)
        {
            RuntimeServices.GameModeService.HumanPlayerState.FireballScore -= Time.deltaTime * RuntimeServices.GameModeService.GameModeSettings.FireballPointEmptySpeed;
            GameModeEvents.TriggerUpdateFireballScore(RuntimeServices.GameModeService.HumanPlayerState.FireballScore, true);

            if (RuntimeServices.GameModeService.HumanPlayerState.FireballScore <= 0)
            {
                RuntimeServices.GameModeService.HumanPlayerState.FireballEnabled = false;
                GameModeEvents.TriggerSetFireballScoreActive(false, true);
            }
        }
        
        if (RuntimeServices.GameModeService.AIPlayerState.FireballScore > 0)
        {
            RuntimeServices.GameModeService.AIPlayerState.FireballScore -= Time.deltaTime * RuntimeServices.GameModeService.GameModeSettings.FireballPointEmptySpeed;
            GameModeEvents.TriggerUpdateFireballScore( RuntimeServices.GameModeService.AIPlayerState.FireballScore, false);
            
            if (RuntimeServices.GameModeService.AIPlayerState.FireballScore <= 0)
            {
                RuntimeServices.GameModeService.AIPlayerState.FireballEnabled = false;
                GameModeEvents.TriggerSetFireballScoreActive(false, false);
            }
        }
    }
}
