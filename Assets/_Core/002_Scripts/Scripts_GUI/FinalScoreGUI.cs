using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles the panel that shows the final score of each player on end game
/// </summary>
public class FinalScoreGUI : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private Transform _visualPanel;
    [SerializeField] private TMP_Text _playerScoreText;
    [SerializeField] private TMP_Text _aiScoreText;
    [SerializeField] private TMP_Text _outcomeText;
    [SerializeField] private string _winOutcomeText = "You Win!";
    [SerializeField] private string _loseOutcomeText = "You Lose...";
    [SerializeField] private string _drawOutcomeText = "Draw";
    
    [Header("Animations")]
    [SerializeField] private float _animationDuration = 0.5f;
    private void Awake()
    {
        GameModeEvents.OnGameModeStateUpdated += OnGameModeStateUpdated;

        Show(false);
    }

    private void OnDestroy()
    {
        GameModeEvents.OnGameModeStateUpdated -= OnGameModeStateUpdated;
    }

    private void OnGameModeStateUpdated(GameModeState gameModeState)
    {
        switch (gameModeState)
        {
            case GameModeState.Playing:
                Show(false);
                break;
            
            case GameModeState.End:
                _playerScoreText.text = RuntimeServices.GameModeService.HumanPlayerState.Score.ToString();
                _aiScoreText.text = RuntimeServices.GameModeService.AIPlayerState.Score.ToString();
                Show(true);
                break;
        }
    }

    private void Show(bool show)
    {
        _visualPanel.gameObject.SetActive(show);
        
        if (show)
        {
            // panel animation
            _visualPanel.transform.localScale = Vector3.zero;
            _visualPanel.transform.DOScale(Vector3.one, _animationDuration);
            
            // Set outcome text
            _outcomeText.text = RuntimeServices.GameModeService.GameModeOutcome == GameModeOutcome.Draw ? _drawOutcomeText : (RuntimeServices.GameModeService.GameModeOutcome == GameModeOutcome.Win ? _winOutcomeText : _loseOutcomeText);
        }
    }
}
