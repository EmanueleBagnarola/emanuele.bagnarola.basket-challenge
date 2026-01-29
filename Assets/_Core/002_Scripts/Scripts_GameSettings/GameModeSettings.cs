using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "GameModeSettings", menuName = "ScriptableObjects/GameModeSettings")]
public class GameModeSettings : ScriptableObject
{
    [field: Header("Timers config")]
    [field: SerializeField] public int StartGameCountdown { get; set; } = 3;

    [field: SerializeField] public int GameModeDuration { get; set; } = 60;
    [field: SerializeField] public float ShootInputMaxTime { get; set; } = 2;
     
    [field: Header("Gameplay config")]
    [field: SerializeField] public float NextShootWaitTime { get; private set; } = 0.7f;

    [Header("Shoot velocity config")]
    [SerializeField, NonReorderable] private List<ShootConfigByPhase> shootConfigs = new List<ShootConfigByPhase>();
    
    [Header("Score config")]
    [SerializeField, NonReorderable] private List<BasicScoreConfig> basicScoreConfigs = new List<BasicScoreConfig>();
    
    public ShootVelocityConfigByType GetShootVelocityConfig(ShootType shootType)
    {
        ShootConfigByPhase shootConfigByPhase = shootConfigs.Find(t => t.Phase == RuntimeServices.GameModeService.CurrentPhase);

        return shootConfigByPhase.VelocityConfigs.Find(s => s.ShootType == shootType);
    }

    public int GetBasicScoreByAccuracy(ShootAccuracy accuracy)
    {
        int score = 0;

        foreach (BasicScoreConfig basicScoreConfig in basicScoreConfigs)
        {
            if (basicScoreConfig.Accuracy == accuracy)
            {
                score = basicScoreConfig.Score;
                break;
            }
        }
        
        return score;
    }
}

[System.Serializable]
public class ShootConfigByPhase
{
    public List<ShootVelocityConfigByType> VelocityConfigs => velocityConfigs;
    public GameModePhase Phase => phase;
    
    [SerializeField] private GameModePhase phase;
    [SerializeField, NonReorderable] private List<ShootVelocityConfigByType> velocityConfigs;
}

[System.Serializable]
public class ShootVelocityConfigByType
{
    public ShootType ShootType;
    
    [Range(0, GameModeEnv.MAX_SHOOT_VELOCITY)]
    public int Min;
    [Range(0, GameModeEnv.MAX_SHOOT_VELOCITY)]
    public int Max;
}

[System.Serializable]
public class BasicScoreConfig
{
    public int Score;
    public ShootAccuracy Accuracy;
}

