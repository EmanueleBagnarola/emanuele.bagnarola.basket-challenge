using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager_SceneManager : MonoBehaviour
{
    public static GameManager_SceneManager Instance;
    
    [SerializeField] private string _mainMenuSceneName = "Scene_MainMenu";
    [SerializeField] private string _gameModeSceneName = "Scene_GameMode";
    [SerializeField] private string _rewardsSceneName = "Scene_Rewards";

    private void Awake()
    {
        InitInstance();
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void InitInstance()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    public void LoadMainMenuScene()
    {
        SceneManagementEvents.TriggerSceneLoad(_mainMenuSceneName);
        
        SceneManager.LoadSceneAsync(_mainMenuSceneName, LoadSceneMode.Single);
    }

    public void LoadGameModeScene()
    {
        SceneManagementEvents.TriggerSceneLoad(_gameModeSceneName);
        
        SceneManager.LoadSceneAsync(_gameModeSceneName, LoadSceneMode.Single);
    }

    public void LoadRewardsScene()
    {
        SceneManagementEvents.TriggerSceneLoad(_rewardsSceneName);
        
        SceneManager.LoadSceneAsync(_rewardsSceneName, LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManagementEvents.TriggerSceneLoaded(scene.name);
    }
}