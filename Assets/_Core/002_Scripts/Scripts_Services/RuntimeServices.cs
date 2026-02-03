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
        public static ShootPhase ShootPhase;
        public static int BackboardBonus;
        public static float Timer;
        public static int PlayerScore;
        public static int AIScore;
    }

    public static class TargetService
    {
        public static Transform ScoreTarget;
        public static Transform BackboardTarget;
        public static Transform FrameTarget;
        public static Transform FrameFailTarget;
        public static Transform BackboardFailGroundTarget;
        public static Transform DirectFailGroundTarget;
    }
}
