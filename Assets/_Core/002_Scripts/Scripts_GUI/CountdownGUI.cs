using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class CountdownGUI : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private TMP_Text _countdownText;
    [SerializeField] private string _countdownEndMessage = "Start!";
    [SerializeField] private float _hideCountdownEndMessageTime = 1;

    [Header("Animation")]
    [SerializeField] private float _scaleDuration = 0.15f;
    
    private void Awake()
    {
        HideCountdownMessage();
        GameModeEvents.OnCountdownTick += OnCountdownTick;
    }
    
    private void OnDestroy()
    {
        GameModeEvents.OnCountdownTick -= OnCountdownTick;
    }

    private void OnCountdownTick(int currentCountdownTimer)
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, _scaleDuration);
        
        if (currentCountdownTimer > 0)
        {
            SetCountdownText(currentCountdownTimer.ToString());
        }
        else
        {
            SetCountdownText(_countdownEndMessage);
            Invoke(nameof(HideCountdownMessage), _hideCountdownEndMessageTime);
        }
    }

    private void SetCountdownText(string text)
    {
        _countdownText.text = text;
    }

    private void HideCountdownMessage()
    {
        transform.localScale = Vector3.zero;
        _countdownText.text = "";
    }
}