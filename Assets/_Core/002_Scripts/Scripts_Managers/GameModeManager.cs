using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    // [Header("General config")]
    // [SerializeField] private int _minScoresDistance;
    
    [Header("Score config")]
    [SerializeField] private GameModeSettings _gameModeSettings;
    [SerializeField] private GameModePhase _currentPhase;

    [Header("Gameplay config")]
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Transform _shootRangeCenter;
    [SerializeField, NonReorderable] private List<ShootRange> _shootRangesByPhase; //NonReorderable attribute added to fix the editor serialized class visualization but

    private ShootRange currentShootRange;
    
    private void Awake()
    {
        Application.targetFrameRate = 60;
        
        RuntimeServices.GameModeService.GameModeSettings = _gameModeSettings;

        GameModeEvents.OnShootCompleted += OnShootCompleted;
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShootCompleted -= OnShootCompleted;
    }

    private void Start()
    {
        UpdateGamePhase(_currentPhase);
        StartGameMode();
    }

    private void StartGameMode()
    {
        UpdateShootPosition();
    }

    private void GenerateShootVelocityTargets()
    {
        ShootVelocityConfigByType directVelocityConfig = _gameModeSettings.GetShootVelocityConfig(ShootType.Direct);
        ShootVelocityConfigByType backboardVelocityConfig = _gameModeSettings.GetShootVelocityConfig(ShootType.Backboard);
        GameModeEvents.TriggerUpdateShootVelocityTargets(directVelocityConfig, backboardVelocityConfig);
    }

    private void UpdateGamePhase(GameModePhase gameModePhase)
    {
        _currentPhase = gameModePhase;
        RuntimeServices.GameModeService.CurrentPhase = _currentPhase;
        GenerateShootVelocityTargets();
    }

    private void OnShootCompleted(ShootResult result)
    {
        // Calculate score taking in consideration type, accuracy and if "special backboard phase" is active:
        // 3 points for "Perfect", 2 points for "Accurate"
        // if type is "Backboard" and special backboard phase is active:
        // based on game phase: early (4 points), mid (6 points), late (8 points)
        switch (result.Accuracy)
        {
            
        }
        
        // Update game phase based on current timer

        // after a fixed wait time, update next shoot position
        StartCoroutine(CallNextShootPosition());
    }

    private IEnumerator CallNextShootPosition()
    {
        yield return new WaitForSeconds(_gameModeSettings.NextShootWaitTime);
        
        // Update new shoot position
        UpdateShootPosition();
    }

    private void UpdateShootPosition()
    {
        GameModeEvents.TriggerCallNewPosition();
        
    }

    [Button]
    public void Debug_GenerateScore()
    {
        UpdateGamePhase(_currentPhase);
    }

}



