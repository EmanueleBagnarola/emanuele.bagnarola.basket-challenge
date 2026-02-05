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
            // If fireball is already enabled, don't add up new score (wait for bar to empty after the set duration)
            if(RuntimeServices.GameModeService.HumanPlayerState.FireballEnabled)
                return;
            
            if (score <= 0)
            {
                // Disable fireball
                RuntimeServices.GameModeService.HumanPlayerState.FireballScore = 0;
                RuntimeServices.GameModeService.HumanPlayerState.FireballEnabled = false;
                GameModeEvents.TriggerSetFireballScoreActive(false, true);
                GameModeEvents.TriggerUpdateFireballScore(0, true);
                AudioEvents.TriggerStopAudioFX(AudioFXId.Fireball);
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
                    
                    // call audio event
                    AudioEvents.TriggerPlayAudioFX(AudioFXId.Fireball);
                    
                    // Call fireball enabled
                    GameModeEvents.TriggerSetFireballScoreActive(true, true);
                    
                    // Show notification
                    UIEvents.TriggerShowNotification(NotificationType.FireballActive);
                }
            }
        }
        else
        {
            // If fireball is already enabled, don't add up new score (wait for bar to empty after the set duration)
            if(RuntimeServices.GameModeService.AIPlayerState.FireballEnabled)
                return;
            
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
        float slowEmptyMultiplier = RuntimeServices.GameModeService.GameModeSettings.FireballPointEmptySpeed;
        float fullDurationMultiplier = RuntimeServices.GameModeService.GameModeSettings.FireballMaxScore / RuntimeServices.GameModeService.GameModeSettings.FireballDuration;
        
        if (RuntimeServices.GameModeService.HumanPlayerState.FireballScore > 0)
        {
            float emptyMultiplier = RuntimeServices.GameModeService.HumanPlayerState.FireballEnabled ? fullDurationMultiplier : slowEmptyMultiplier;
            
            RuntimeServices.GameModeService.HumanPlayerState.FireballScore -= Time.deltaTime * emptyMultiplier;
            GameModeEvents.TriggerUpdateFireballScore(RuntimeServices.GameModeService.HumanPlayerState.FireballScore, true);

            if (RuntimeServices.GameModeService.HumanPlayerState.FireballScore <= 0)
            {
                RuntimeServices.GameModeService.HumanPlayerState.FireballEnabled = false;
                
                AudioEvents.TriggerStopAudioFX(AudioFXId.Fireball);
                
                GameModeEvents.TriggerSetFireballScoreActive(false, true);
            }
        }
        
        if (RuntimeServices.GameModeService.AIPlayerState.FireballScore > 0)
        {
            float emptyMultiplier = RuntimeServices.GameModeService.AIPlayerState.FireballEnabled ? fullDurationMultiplier : slowEmptyMultiplier;

            RuntimeServices.GameModeService.AIPlayerState.FireballScore -= Time.deltaTime * emptyMultiplier;
            GameModeEvents.TriggerUpdateFireballScore( RuntimeServices.GameModeService.AIPlayerState.FireballScore, false);
            
            if (RuntimeServices.GameModeService.AIPlayerState.FireballScore <= 0)
            {
                RuntimeServices.GameModeService.AIPlayerState.FireballEnabled = false;
                GameModeEvents.TriggerSetFireballScoreActive(false, false);
            }
        }
    }
}
