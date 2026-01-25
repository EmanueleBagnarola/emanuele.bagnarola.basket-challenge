using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ThrowSliderGUI : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private RectTransform _directScoreLabel;
    [SerializeField] private RectTransform _blackboardScoreLabel;

    private float _sliderLenght;
    
    private void OnEnable()
    {
        InputEvents.OnPointerDrag += OnPointerDrag;
        InputEvents.OnPointerUp += OnPointerUp;

        GameModeEvents.OnScoresGenerated += OnScoreGenerated;
    }

    private void OnDisable()
    {
        InputEvents.OnPointerDrag -= OnPointerDrag;
        InputEvents.OnPointerUp -= OnPointerUp;
        
        GameModeEvents.OnScoresGenerated -= OnScoreGenerated;
    }

    private void Awake()
    {
        InitSlider();
    }

    private void InitSlider()
    {
        _slider.minValue = 0;
        _slider.maxValue = GameModeEnv.MAX_THROW_SCORE;
        _slider.value = 0;
        _sliderLenght = _slider.GetComponent<RectTransform>().sizeDelta.y;
    }
    
    private void OnPointerDrag(Vector2 currentPosition, Vector2 startPosition)
    {
        UpdateSlider(currentPosition, startPosition);
    }
    
    private void OnPointerUp(Vector2 currentPosition)
    {
        ThrowAttempt();
        _slider.value = 0;
    }

    private void OnScoreGenerated(ScoreInfo _directScoreData, ScoreInfo _blackboardScoreData)
    {
        PlaceLabel(_directScoreData.Min, _directScoreData.Max, _directScoreLabel);
        PlaceLabel(_blackboardScoreData.Min, _blackboardScoreData.Max, _blackboardScoreLabel);
    }

    /// <summary>
    /// Updates the slider value based on current drag input
    /// </summary>
    /// <param name="currentPosition"></param>
    /// <param name="startPosition"></param>
    private void UpdateSlider(Vector2 currentPosition, Vector2 startPosition)
    {
        float deltaY = Mathf.Max(0f, currentPosition.y - startPosition.y);
        
        float normalized = Mathf.Clamp01(deltaY / Screen.height);
        
        float dragValue = normalized * GameModeEnv.MAX_THROW_SCORE;
        
        float previousSliderValue = _slider.value;
        
        // update only if drag input is increasing
        if(dragValue < previousSliderValue)
            return;

        _slider.value = dragValue;
    }
    
    /// <summary>
    /// When mouse or touch input ends, call the throw attempt
    /// </summary>
    private void ThrowAttempt()
    {
        GameModeEvents.OnThrowAttempt?.Invoke(_slider.value);
    }

    private void PlaceLabel(int _scoreMin, int _scoreMax, RectTransform _labelRect)
    {
        // get the average score point that will be used to set the slider position
        float scoreMiddlePoint = (_scoreMin + _scoreMax) / 2.0f;
        
        // calculate the label size multiplier that will be used to set the rect correct size in proportion to current slider height
        float rectSizeMult = _sliderLenght / GameModeEnv.MAX_THROW_SCORE;
        
        // map where the label should be placed on slider
        float sliderPosition = GameUtils.Map(
            scoreMiddlePoint * rectSizeMult, 
            0, 
            GameModeEnv.MAX_THROW_SCORE * rectSizeMult,
            -_sliderLenght / 2, 
            _sliderLenght / 2);
        
        // resize label
        _labelRect.sizeDelta = new Vector2(_labelRect.sizeDelta.x, (_scoreMax - _scoreMin)*rectSizeMult);
        
        // set label position
        _labelRect.localPosition = new Vector3(0, sliderPosition, 0);
    }
}
