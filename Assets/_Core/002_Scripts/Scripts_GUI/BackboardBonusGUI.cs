using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BackboardBonusGUI : MonoBehaviour
{
    [SerializeField] private GameObject _visualPanel;
    [SerializeField] private TMP_Text _bonusText;

    private void Awake()
    {
        ShowBonusText(false, "");

        GameModeEvents.OnBackboardBonus += OnBackboardBonus;
    }

    private void OnDestroy()
    {
        GameModeEvents.OnBackboardBonus -= OnBackboardBonus;
    }

    private void OnBackboardBonus(bool show, int bonusScore)
    {
        ShowBonusText(show, $"+{bonusScore}");
    }
    
    private void ShowBonusText(bool show, string bonusText)
    {
        _visualPanel.SetActive(show);
        
        _bonusText.text = bonusText;
    }
}
