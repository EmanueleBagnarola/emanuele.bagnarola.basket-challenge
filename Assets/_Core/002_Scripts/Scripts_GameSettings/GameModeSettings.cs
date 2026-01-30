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

    // How much time a gameplay session lasts in seconds
    [field: SerializeField] public int GameModeDuration { get; set; } = 60;
   
    // Max time to check to keep the touch input for shooting attempt valid
    [field: SerializeField] public float ShootInputMaxTime { get; set; } = 2;
     
    // Time to wait from shooting result (score / fail) to next shooting position
    [field: Header("Gameplay config")]
    [field: SerializeField] public float NextShootWaitTime { get; private set; } = 0.7f;

    [Header("Shoot velocity config")]
    [SerializeField, NonReorderable] private List<ShootConfigByPhase> shootConfigs = new List<ShootConfigByPhase>();
    
    [Header("Score config")]
    [SerializeField, NonReorderable] private List<BasicScoreConfig> basicScoreConfigs = new List<BasicScoreConfig>();
    
    [field: Header("Debug")]
    [field: SerializeField] public bool Debug_UseMaxInputTime { get; private set; } = true;
    
    public ShootVelocityConfigByType GetShootVelocityConfig(ShootType shootType)
    {
        ShootConfigByPhase shootConfigByPhase = shootConfigs.Find(t => t.Phase == RuntimeServices.GameModeService.CurrentPhase);

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

