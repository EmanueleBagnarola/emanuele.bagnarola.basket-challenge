using System;
using UnityEngine;
using UnityEngine.UI;

public class ShootSliderGUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider _slider;
    [SerializeField] private RectTransform _directScoreLabel;
    [SerializeField] private RectTransform _blackboardScoreLabel;

    [Header("Input")]
    [SerializeField] private float _sensitivity = 2f;

    private float _sliderLength;

    // Input state
    private bool _gestureActive;
    private bool _waitingForRelease;
    private float _currentInputTime;

    private Vector2 _inputStartPos;
    private Vector2 _inputCurrentPos;

    #region Unity

    private void Awake()
    {
        InitSlider();

        InputEvents.OnPointerDown += OnPointerDown;
        InputEvents.OnPointerDrag += OnPointerDrag;
        InputEvents.OnPointerUp += OnPointerUp;

        GameModeEvents.OnVelocityTargetsGenerated += OnVelocityTargetGenerated;
        GameModeEvents.OnShootPositionUpdated += OnShootPositionUpdated;
    }

    private void Update()
    {
        UpdateSlider();
    }

    private void OnDestroy()
    {
        InputEvents.OnPointerDown -= OnPointerDown;
        InputEvents.OnPointerDrag -= OnPointerDrag;
        InputEvents.OnPointerUp -= OnPointerUp;

        GameModeEvents.OnVelocityTargetsGenerated -= OnVelocityTargetGenerated;
        GameModeEvents.OnShootPositionUpdated -= OnShootPositionUpdated;
    }

    #endregion

    #region Init

    private void InitSlider()
    {
        _slider.minValue = 0f;
        _slider.maxValue = GameModeEnv.MAX_SHOOT_VELOCITY;
        _slider.value = 0f;

        _sliderLength = _slider.GetComponent<RectTransform>().sizeDelta.y;
    }

    #endregion

    #region Input

    private void OnPointerDown(Vector2 position)
    {
        if (_waitingForRelease)
            return;

        _gestureActive = true;
        _currentInputTime = 0f;

        _inputStartPos = position;
        _inputCurrentPos = position;

        _slider.value = 0f;
    }

    private void OnPointerDrag(Vector2 currentPosition, Vector2 _)
    {
        if (!_gestureActive || _waitingForRelease)
            return;

        _currentInputTime += Time.deltaTime;

        if (_currentInputTime > RuntimeServices.GameModeService.GameModeSettings.ShootInputMaxTime)
        {
            ShootAttempt();
            return;
        }

        _inputCurrentPos = currentPosition;
    }

    private void OnPointerUp()
    {
        if (!_gestureActive)
            return;

        ShootAttempt();
    }

    #endregion

    #region Slider Logic

    private void UpdateSlider()
    {
        if (!_gestureActive)
            return;

        float deltaY = Mathf.Max(0f, _inputCurrentPos.y - _inputStartPos.y);
        float normalized = Mathf.Clamp01(deltaY / Screen.height);

        float value = normalized * GameModeEnv.MAX_SHOOT_VELOCITY * _sensitivity;

        // Detect input only going up
        _slider.value = Mathf.Max(_slider.value, value);
    }

    #endregion

    #region Shoot

    private void ShootAttempt()
    {
        _gestureActive = false;
        _waitingForRelease = true;
        _currentInputTime = 0f;

        GameModeEvents.TriggerShootAttempt(_slider.value, true);
    }

    private void OnShootPositionUpdated()
    {
        _waitingForRelease = false;
        _slider.value = 0f;
    }

    #endregion

    #region Labels

    private void OnVelocityTargetGenerated(ShootVelocityConfigByType directScore, ShootVelocityConfigByType blackboardScore)
    {
        PlaceLabel(directScore.Min, directScore.Max, _directScoreLabel);
        PlaceLabel(blackboardScore.Min, blackboardScore.Max, _blackboardScoreLabel);
    }

    private void PlaceLabel(int scoreMin, int scoreMax, RectTransform label)
    {
        float middle = (scoreMin + scoreMax) * 0.5f;
        float rectMult = _sliderLength / GameModeEnv.MAX_SHOOT_VELOCITY;

        float position = GameUtils.Map(
            middle * rectMult,
            0f,
            GameModeEnv.MAX_SHOOT_VELOCITY * rectMult,
            -_sliderLength * 0.5f,
            _sliderLength * 0.5f
        );

        label.sizeDelta = new Vector2(
            label.sizeDelta.x,
            (scoreMax - scoreMin) * rectMult
        );

        label.localPosition = new Vector3(0f, position, 0f);
    }

    #endregion
}
