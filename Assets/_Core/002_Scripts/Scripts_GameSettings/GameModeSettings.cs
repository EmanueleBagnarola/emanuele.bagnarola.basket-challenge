using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "GameModeSettings", menuName = "ScriptableObjects/GameModeSettings")]
public class GameModeSettings : ScriptableObject
{
    [SerializeField] private ScoreData earlyScoreData;
    [SerializeField] private ScoreData midScoreData;
    [SerializeField] private ScoreData lateScoreData;
    
    public ScoreData GetScoreData(GameModePhase gameModePhase)
    {
        switch (gameModePhase)
        {
            case GameModePhase.Early:
                return earlyScoreData;
            
            case GameModePhase.Mid:
                return midScoreData;
            
            case GameModePhase.Late:
                return lateScoreData;
        }

        return earlyScoreData;
    }
}

[System.Serializable]
public struct ScoreData
{
    public ScoreInfo DirectScoreInfo;
    public ScoreInfo BackboardScoreInfo;
}

[System.Serializable]
public struct ScoreInfo
{
    [Range(0, GameModeEnv.MAX_THROW_SCORE)]
    public int Min;
    [Range(0, GameModeEnv.MAX_THROW_SCORE)]
    public int Max;
}

