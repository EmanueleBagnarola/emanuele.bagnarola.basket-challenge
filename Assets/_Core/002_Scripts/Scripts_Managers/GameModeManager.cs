using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameModeManager : MonoBehaviour
{
    // [Header("General config")]
    // [SerializeField] private int _minScoresDistance;
    
    [Header("Score config")]
    [SerializeField] private GameModeSettings _gameModeSettings;
    [SerializeField] private GameModePhase _currentPhase;

    private void Awake()
    {
        GameModeServices.GameModeSettings = _gameModeSettings;
    }

    private void Start()
    {
        UpdateGamePhase(_currentPhase);
    }

    private void GenerateScores(GameModePhase gameModePhase)
    {
        ScoreData scoreData = _gameModeSettings.GetScoreData(gameModePhase);
        GameModeEvents.OnScoresGenerated?.Invoke(scoreData.DirectScoreInfo, scoreData.BackboardScoreInfo);
    }

    private void UpdateGamePhase(GameModePhase gameModePhase)
    {
        _currentPhase = gameModePhase;
        GameModeServices.CurrentPhase = _currentPhase;
        GenerateScores(_currentPhase);
    }

    [Button]
    public void Debug_GenerateScore()
    {
        UpdateGamePhase(_currentPhase);
    }
}

public static class GameModeServices
{
    public static GameModeSettings GameModeSettings;
    public static GameModePhase CurrentPhase;
}

public enum GameModePhase
{
    Early,
    Mid,
    Late,
}

