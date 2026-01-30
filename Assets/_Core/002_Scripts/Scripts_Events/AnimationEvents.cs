using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimationEvents
{
    public delegate void OnRimTouchedHandler();
    public static event OnRimTouchedHandler OnRimTouched;
    
    // --- Event Triggers ---

    /// <summary>
    /// Called when the ball movement curve reach the rim target position
    /// </summary>
    public static void TriggerRimTouched()
    {
        OnRimTouched?.Invoke();
    }
}
