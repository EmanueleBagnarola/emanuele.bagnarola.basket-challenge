using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimationEvents
{
    public delegate void OnRimTouchedHandler();
    public static event OnRimTouchedHandler OnRimTouched;
    
    // --- Event Triggers ---

    public static void TriggerRimTouched()
    {
        OnRimTouched?.Invoke();
    }
}
