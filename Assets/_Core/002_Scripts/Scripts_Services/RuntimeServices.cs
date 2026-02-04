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
        public static PlayerShootPhase PlayerShootPhase;
        public static GameModeOutcome GameModeOutcome;
        public static int BackboardBonus;
        public static float Timer;
        public static int PlayerScore;
        public static int AIScore;

        public static void Reset()
        {
            PlayerShootPhase = PlayerShootPhase.WaitForShot; 
            BackboardBonus = 0;
            PlayerScore = 0;
            AIScore = 0;
        }
    }

    public static class TargetService
    {
        public static Vector3 ScoreTargetPos;
        // public static Transform BackboardTarget;
        // public static Transform FrameTarget;
        // public static Transform FrameFailTarget;
        // public static Transform BackboardFailGroundTarget;
        // public static Transform DirectFailGroundTarget;

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


