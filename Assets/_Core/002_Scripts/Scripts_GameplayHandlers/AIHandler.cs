using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIHandler : MonoBehaviour
{
    [SerializeField] private float _minShotVelocity;
    [SerializeField] private float _maxShotVelocity;
    [SerializeField] private float _nextShotWaitTime;
    [SerializeField, NonReorderable] private List<AIDifficultyConfig> _difficultyConfigs;

    private Coroutine _shootAttemptCoroutine;
    
    private void Awake()
    {
        GameModeEvents.OnGameModeStateUpdated += OnGameModeStateUpdated;
        GameModeEvents.OnShootPositionUpdated += OnShootPositionUpdated;
    }

    private void Start()
    {
        Debug.Log($"AI Difficulty: {RuntimeServices.GameModeService.AIDifficulty}");
    }

    private void OnDestroy()
    {
        GameModeEvents.OnGameModeStateUpdated -= OnGameModeStateUpdated;
        GameModeEvents.OnShootPositionUpdated -= OnShootPositionUpdated;
    }

    private void OnGameModeStateUpdated(GameModeState gameModeState)
    {
        switch (gameModeState)
        {
            case GameModeState.Playing:
                _shootAttemptCoroutine = StartCoroutine(ShootAttempt(_nextShotWaitTime));
                break;
        }
    }

    private void OnShootPositionUpdated(bool isHumanPlayer)
    {
        if(isHumanPlayer)
            return;

        if(RuntimeServices.GameModeService.GameModeState != GameModeState.Playing)
            return;
        
        if (RuntimeServices.GameModeService.GameModeState == GameModeState.End)
        {
            StopCoroutine(_shootAttemptCoroutine);
            return;
        }
        
        _shootAttemptCoroutine = StartCoroutine(ShootAttempt(_nextShotWaitTime));
    }
    
    private IEnumerator ShootAttempt(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        
        ShootType randomShootType = (ShootType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(ShootType)).Length);


        ShootResult aiShotResult = new ShootResult(
            randomShootType,
            GetShootAccuracy(),
            ShootVelocityType.Medium,
            false);
        
        GameModeEvents.TriggerAIShot(aiShotResult);

        // GameModeEvents.TriggerShootAttempt(UnityEngine.Random.Range(_minShotVelocity, _maxShotVelocity), false);
    }
    
    private ShootAccuracy GetShootAccuracy()
    {
        int totalWeight = 0;

        AIDifficultyConfig difficultyConfig = GetDifficultyConfig();

        foreach (var config in difficultyConfig.ShootConfigs)
        {
            totalWeight += config.Weight;
        }
        
        int r = UnityEngine.Random.Range(0, totalWeight);
        int v = 0;

        foreach (var config in difficultyConfig.ShootConfigs)
        {
            v += config.Weight;
            if (r < v)
            {
                return config.ShootAccuracy;
            }
        }

        return difficultyConfig.ShootConfigs[difficultyConfig.ShootConfigs.Count - 1].ShootAccuracy;
    }

    private AIDifficultyConfig GetDifficultyConfig()
    {
        return _difficultyConfigs.Find(c => c.Difficulty == RuntimeServices.GameModeService.AIDifficulty);
    }
}

[System.Serializable]
public class AIDifficultyConfig
{
    public AIDifficulty Difficulty => _difficulty;
    public List<AIShootConfig> ShootConfigs => _shootsConfig;
    
    [SerializeField] private AIDifficulty _difficulty;
    [SerializeField, NonReorderable] private List<AIShootConfig> _shootsConfig = new List<AIShootConfig>();
}

[System.Serializable]
public class AIShootConfig
{
    public ShootAccuracy ShootAccuracy;
    public int Weight;
}
