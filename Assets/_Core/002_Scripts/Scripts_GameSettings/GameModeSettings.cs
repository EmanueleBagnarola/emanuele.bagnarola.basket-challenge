using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[CreateAssetMenu(fileName = "GameModeSettings", menuName = "ScriptableObjects/GameModeSettings")]
public class GameModeSettings : ScriptableObject
{
    [field: Header("Timers config")]
    [field: SerializeField] public int StartGameCountdown { get; private set; } = 3;

    // How much time a gameplay session lasts in seconds
    [field: SerializeField] public int GameModeDuration { get; private set; } = 60;
    
    // Time to wait from shooting result (score / fail) to next shooting position
    [field: Header("Gameplay config")]
    [field: SerializeField] public ShootSettings ShootSettings { get; private set; }

    // Probability that the backboard bonus can appear after each shot
    [field: Header("Bonus Score config")]
    [field: SerializeField] public int BackboardBonusProbability { get; private set; }
    [SerializeField, NonReorderable] private List<BackboardBonusScoreConfig> BackboardBonusScoreConfigs = new List<BackboardBonusScoreConfig>();
    
    [Header("Shoot velocity config")]
    [SerializeField, NonReorderable] private List<ShootConfigByPhase> shootConfigs = new List<ShootConfigByPhase>();
    
    [Header("Basic Score config")]
    [SerializeField, NonReorderable] private List<BasicScoreConfig> basicScoreConfigs = new List<BasicScoreConfig>();

    [field: Header("Fireball config")]
    [field: SerializeField] public int FireballPointValue { get; private set; } = 1;
    [field: SerializeField] public int FireballMaxScore { get; private set; } = 10;
    [field: SerializeField] public float FireballPointEmptySpeed { get; private set; } = 0.1f;
    [field: SerializeField] public int FireballScoreMultiplier { get; private set; } = 2;
    [field: SerializeField] public float FireballDuration { get; private set; } = 5f;
    
    [field: Header("Rewards config")]
    [field: SerializeField] public float ShowRewardsPageWaitTime { get; private set; } = 2;
    [field: SerializeField, NonReorderable] private List<MoneyRewardConfig> moneyRewardConfigs = new List<MoneyRewardConfig>();
    
    [field: Header("Debug")]
    [field: SerializeField] public bool Debug_UseMaxInputTime { get; private set; } = true;
    
    public ShootVelocityConfigByType GetShootVelocityConfig(ShootType shootType)
    {
        ShootConfigByPhase shootConfigByPhase = shootConfigs.Find(t => t.Phase == RuntimeServices.GameModeService.GameModePhase);

        return shootConfigByPhase.VelocityConfigs.Find(s => s.ShootType == shootType);
    }

    public int GetBasicScoreByAccuracy(ShootAccuracy accuracy, ShootType type)
    {
        int score = 0;

        foreach (BasicScoreConfig basicScoreConfig in basicScoreConfigs)
        {
            if (basicScoreConfig.Accuracy == accuracy && basicScoreConfig.Type == type)
            {
                score = basicScoreConfig.Score;
                break;
            }
        }
        
        return score;
    }

    public int GetRandomBackboardBonusScore()
    {
        int totalWeight = 0;

        foreach (var backboardBonusScore in BackboardBonusScoreConfigs)
        {
            totalWeight += backboardBonusScore.RandomWeight;
        }
        
        int r = UnityEngine.Random.Range(0, totalWeight);
        int v = 0;

        foreach (var backboardBonusScore in BackboardBonusScoreConfigs)
        {
            v += backboardBonusScore.RandomWeight;
            if (r < v)
            {
                return backboardBonusScore.Score;
            }
        }

        return BackboardBonusScoreConfigs[BackboardBonusScoreConfigs.Count - 1].Score;
    }

    public int GetReward()
    {
        return moneyRewardConfigs.Find(c => c.Outcome == RuntimeServices.GameModeService.GameModeOutcome).MoneyReward;
    }
}

/// <summary>
/// Based on the current match phase (updated checking the lasting time), get the corresponding velocity config
/// </summary>
[System.Serializable]
public class ShootConfigByPhase
{
    public List<ShootVelocityConfigByType> VelocityConfigs => velocityConfigs;
    public GameModePhase Phase => phase;
    
    [SerializeField] private GameModePhase phase;
    [SerializeField, NonReorderable] private List<ShootVelocityConfigByType> velocityConfigs;
}

/// <summary>
/// Based on shoot type (direct or backboard) chooses the limits to show on the slider bar
/// </summary>
[System.Serializable]
public class ShootVelocityConfigByType
{
    public ShootType ShootType;
    
    [Range(0, GameModeEnv.MAX_SHOOT_VELOCITY)]
    public int Min;
    [Range(0, GameModeEnv.MAX_SHOOT_VELOCITY)]
    public int Max;
}

/// <summary>
/// Base on final shoot type (accuracy and if direct/backboard) gives the correct basic score (i.e. 3 if perfect, otherwise 2)
/// </summary>
[System.Serializable]
public class BasicScoreConfig
{
    public int Score;
    public ShootAccuracy Accuracy;
    public ShootType Type;
}

[System.Serializable]
public class BackboardBonusScoreConfig
{
    public int Score;
    public int RandomWeight;
}

[System.Serializable]
public class MoneyRewardConfig
{
    public GameModeOutcome Outcome;
    public int MoneyReward;
}

