using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalScoreGUI : MonoBehaviour
{
    [SerializeField] private PlayerLabelGUI humanLabelGUI;
    [SerializeField] private PlayerLabelGUI aiLabelGUI;
    
    private void Awake()
    {
        GameModeEvents.OnGlobalScoreUpdated += OnGlobalScoreUpdated;
    }

    private void OnDestroy()
    {
        GameModeEvents.OnGlobalScoreUpdated -= OnGlobalScoreUpdated;
    }

    private void OnGlobalScoreUpdated(int score, bool isHumanPlayer)
    {
        if (isHumanPlayer)
        {
            humanLabelGUI.UpdateScore(score);
        }
        else
        {
            
        }
    }
    
}
