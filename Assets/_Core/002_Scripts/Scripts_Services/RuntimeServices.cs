using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RuntimeServices 
{
    public static class GameModeService
    {
        public static GameModeSettings GameModeSettings;
        public static GameModePhase CurrentPhase;
    }

    public static class TargetService
    {
        public static Transform ScoreTarget;
        public static Transform BackboardTarget;
        public static Transform FrameTarget;
        public static Transform AdaptableGroundTarget;
        public static Transform LeftSideGroundTarget;
        public static Transform RightSideGroundTarget;
    }
}
