using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "GameModeSettings", menuName = "ScriptableObjects/GameModeSettings")]
public class GameModeSettings : ScriptableObject
{
    public float NextShootWaitTime => _nextShootWaitTime;
    
    [Header("Gameplay config")]
    [SerializeField] private float _nextShootWaitTime;

    [Header("Shoot velocity config")]
    [SerializeField, NonReorderable] private List<ShootConfigByPhase> shootConfigs = new List<ShootConfigByPhase>();
    
    public ShootVelocityConfigByType GetShootVelocityConfig(ShootType shootType)
    {
        ShootConfigByPhase shootConfigByPhase = shootConfigs.Find(t => t.Phase == RuntimeServices.GameModeService.CurrentPhase);

        return shootConfigByPhase.VelocityConfigs.Find(s => s.ShootType == shootType);
    }
}

[System.Serializable]
public struct ShootConfigByPhase
{
    public List<ShootVelocityConfigByType> VelocityConfigs => velocityConfigs;
    public GameModePhase Phase => phase;
    
    [SerializeField] private GameModePhase phase;
    [SerializeField, NonReorderable] private List<ShootVelocityConfigByType> velocityConfigs;
}

[System.Serializable]
public struct ShootVelocityConfigByType
{
    public ShootType ShootType;
    
    [Range(0, GameModeEnv.MAX_SHOOT_VELOCITY)]
    public int Min;
    [Range(0, GameModeEnv.MAX_SHOOT_VELOCITY)]
    public int Max;
}

