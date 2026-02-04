using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class RuntimeServices 
{
    public static class GameModeService
    {
        public static GameModeSettings GameModeSettings;
        public static GameModeState GameModeState;
        public static GameModePhase GameModePhase;
        public static GameModeOutcome GameModeOutcome;
        public static int BackboardBonus;
        public static float Timer;
        public static PlayerState HumanPlayerState = new PlayerState();
        public static PlayerState AIPlayerState = new PlayerState();

        public static void Reset()
        {
            HumanPlayerState = new PlayerState();
            AIPlayerState = new PlayerState();
            
            BackboardBonus = 0;
        }
    }

    public class PlayerState
    {
        public int Score;
        public float FireballScore;
        public bool FireballEnabled;
        public PlayerShootPhase ShootPhase;
    }

    public static class TargetService
    {
        public static Vector3 ScoreTargetPos;
        
        public static TargetState PlayerTargetState;
        public static TargetState AITargetState;
    }

    public struct TargetState
    {
        public Vector3 BackboardTargetPos;
        public Vector3 FrameTargetPos;
        public Vector3 FrameFailTargetPos;
        public Vector3 BackboardFailGroundTargetPos;
        public Vector3 DirectFailGroundTargetPos;
    }
}


