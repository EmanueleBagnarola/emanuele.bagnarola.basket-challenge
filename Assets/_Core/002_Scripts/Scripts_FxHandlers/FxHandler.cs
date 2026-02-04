using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FxHandler : MonoBehaviour
{
    [SerializeField, NonReorderable] private List<ShotFxConfig> _shotFxConfigs = new List<ShotFxConfig>(); 
    
    private void Awake()
    {
        GameModeEvents.OnShootCompleted += OnShootCompleted;
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShootCompleted -= OnShootCompleted;
    }

    private void OnShootCompleted(ShootResult shootResult)
    {
        ShotFxConfig shotFxConfig = _shotFxConfigs.Find(c => c.Accuracy == shootResult.Accuracy);
        
        if(shotFxConfig == null)
            return;
        
        foreach (var fx in shotFxConfig.FxList)
        {
            fx.Play();
        }
    }
}

[System.Serializable]
public class ShotFxConfig
{
    public ShootAccuracy Accuracy;
    public List<ParticleSystem> FxList = new List<ParticleSystem>();
}
