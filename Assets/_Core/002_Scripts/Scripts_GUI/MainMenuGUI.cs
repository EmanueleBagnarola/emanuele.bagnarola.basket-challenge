using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuGUI : MonoBehaviour
{
    [SerializeField] private Button _startButton;

    private void Awake()
    {
        _startButton.onClick.AddListener(OnStartButtonClicked);
    }

    private void OnStartButtonClicked()
    {
        GameManager_SceneManager.Instance.LoadGameModeScene();
    }
}
