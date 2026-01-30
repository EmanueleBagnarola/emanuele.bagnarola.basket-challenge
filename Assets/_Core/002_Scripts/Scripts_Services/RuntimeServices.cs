using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
        public static Transform FrameFailTarget;
        public static Transform BackboardFailGroundTarget;
        public static Transform LeftDirectFailGroundTarget;
        public static Transform RightDirectFailGroundTarget;
    }

    public static class InputService
    {
        public static InputAction PointerPosition;
    }
}
