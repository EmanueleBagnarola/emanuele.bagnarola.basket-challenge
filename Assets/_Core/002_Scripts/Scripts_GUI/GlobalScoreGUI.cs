using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GlobalScoreGUI : MonoBehaviour
{
    [SerializeField] private PlayerLabelGUI _humanLabelGUI;
    [SerializeField] private PlayerLabelGUI _aiLabelGUI;
    
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
            _humanLabelGUI.UpdateScore(score);
        }
        else
        {
            _aiLabelGUI.UpdateScore(score);
        }
    }
}
