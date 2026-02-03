using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardsGUI : MonoBehaviour
{
    [SerializeField] private PlayerLabelGUI _humanLabelGUI;
    [SerializeField] private PlayerLabelGUI _aiLabelGUI;
    [SerializeField] private TMP_Text _moneyRewardText;
    [SerializeField] private Button _mainMenuButton;
 
    private void Awake()
    {
        GameModeEvents.OnShowRewards += OnShowRewards;
        
        _mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShowRewards -= OnShowRewards;
    }

    private void OnShowRewards()
    {
        ShowScore();
        ShowMoney();
    }

    private void ShowScore()
    {
        _humanLabelGUI.UpdateScore(RuntimeServices.GameModeService.PlayerScore);
    }

    private void ShowMoney()
    {
        _moneyRewardText.text = $"${RuntimeServices.GameModeService.GameModeSettings.CurrencyReward}";
    }

    private void OnMainMenuButtonClicked()
    {
        GameManager_SceneManager.Instance.LoadMainMenuScene();
    }
}
