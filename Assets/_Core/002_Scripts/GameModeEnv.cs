using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameModeEnv 
{
    public const int MAX_THROW_SCORE = 100;
}

public enum ThrowType
{
    Direct,
    Backboard,
}

public enum ThrowStrength
{
    Weak,
    Medium,
    Strong
}

public enum ThrowPosition
{
    Left,
    Right
}

public enum ThrowAccuracy
{
    Perfect,
    Accurate,
    Fail,
}

public enum GameModePhase
{
    Early,
    Mid,
    Late,
}

