using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the game mode timer UI and the end game scores panel
/// </summary>
public class GameModeProgressionGUI : MonoBehaviour
{
    [Header("Slider config")]
    [SerializeField] private Slider _timerSlider;
    [SerializeField] private Image _fillImage;
    [SerializeField] private List<Color> _timerVisualizationColors = new List<Color>();

    private bool _updateSliderVisual = false;

    private void Awake()
    {
        GameModeEvents.OnGameModeStateUpdated += OnGameModeStateUpdated;
    }

    private void OnDestroy()
    {
        GameModeEvents.OnGameModeStateUpdated -= OnGameModeStateUpdated;
    }

    private void Update()
    {
        UpdateSlider();
    }

    private void InitSlider()
    {
        _timerSlider.maxValue = RuntimeServices.GameModeService.GameModeSettings.GameModeDuration;
        _timerSlider.value = _timerSlider.maxValue;
        
        _fillImage.color = _timerVisualizationColors[0];
    }

    private void UpdateSlider()
    {
        if(!_updateSliderVisual)
            return;
        
        // Update value
        _timerSlider.value = RuntimeServices.GameModeService.Timer;
        
        // Update colors
        float n = Mathf.Clamp01(1 - (_timerSlider.value / RuntimeServices.GameModeService.GameModeSettings.GameModeDuration));
        int index = Mathf.FloorToInt(n * _timerVisualizationColors.Count);
        index = Mathf.Clamp(index, 0, _timerVisualizationColors.Count - 1);
        _fillImage.color = _timerVisualizationColors[index];
    }

    private void OnGameModeStateUpdated(GameModeState gameModeState)
    {
        switch (gameModeState)
        {
            case GameModeState.Startup:
                InitSlider();
                ShowFill(false);
                break;
            
            case GameModeState.Playing:
                ShowFill(true);
                _updateSliderVisual = true;
                break;
            
            case GameModeState.End:
                break;
        }
    }

    private void ShowFill(bool show)
    {
        _fillImage.enabled = show;
    }
}
