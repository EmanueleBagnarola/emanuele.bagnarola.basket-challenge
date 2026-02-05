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
    [SerializeField] private GameObject _humanWinnerLabel;
    [SerializeField] private GameObject _aiWinnerLabel;
    [SerializeField] private GameObject _drawLabel;
    [SerializeField] private TMP_Text _moneyRewardText;
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private Button _playAgainButton;
 
    private void OnEnable()
    {
        _mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        _playAgainButton.onClick.AddListener(OnPlayAgainButtonClicked);
        
        ShowContent();
    }
    private void ShowContent()
    {
        ShowScore();
        ShowMoney();
        ShowWinnerLabel();
    }

    private void ShowScore()
    {
        _humanLabelGUI.UpdateScore(RuntimeServices.GameModeService.HumanPlayerState.Score);
        _aiLabelGUI.UpdateScore(RuntimeServices.GameModeService.AIPlayerState.Score);
    }

    private void ShowMoney()
    {
        _moneyRewardText.text = $"${RuntimeServices.GameModeService.GameModeSettings.GetReward()}";
    }

    private void ShowWinnerLabel()
    {
        _humanWinnerLabel.SetActive(false);
        _aiWinnerLabel.SetActive(false);
        _drawLabel.SetActive(false);
        
        switch (RuntimeServices.GameModeService.GameModeOutcome)
        {
            case GameModeOutcome.Win:
                _humanWinnerLabel.SetActive(true);
                break;
            
            case GameModeOutcome.Lose:
                _aiWinnerLabel.SetActive(true);
                break;
            
            case GameModeOutcome.Draw:
                _drawLabel.SetActive(true);
                break;
        }
    }

    private void OnMainMenuButtonClicked()
    {
        GameManager_SceneManager.Instance.LoadMainMenuScene();
    }

    private void OnPlayAgainButtonClicked()
    {
        GameManager_SceneManager.Instance.LoadGameModeScene();
    }
}
