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
                _playerScoreText.text = RuntimeServices.GameModeService.PlayerScore.ToString();
                _aiScoreText.text = RuntimeServices.GameModeService.AIScore.ToString();
                Show(true);
                break;
        }
    }

    private void Show(bool show)
    {
        _visualPanel.gameObject.SetActive(show);
        
        if (show)
        {
            _visualPanel.transform.localScale = Vector3.zero;
            _visualPanel.transform.DOScale(Vector3.one, _animationDuration);
        }
    }
}
