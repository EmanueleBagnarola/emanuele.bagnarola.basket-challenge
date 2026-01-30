using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game global variables
/// </summary>
public static class GameModeEnv 
{
    public const int MAX_SHOOT_VELOCITY = 100;
}

public enum ShootType
{
    Direct,
    Backboard,
}

public enum ShootVelocityType
{
    Weak,
    Medium,
    Strong
}

public enum ShootDirection
{
    Left,
    Right
}

public enum ShootAccuracy
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

