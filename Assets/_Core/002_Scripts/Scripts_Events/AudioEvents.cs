using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioEvents
{
    public delegate void OnPlayAudioFXHandler(AudioFXId id);
    public static event OnPlayAudioFXHandler OnPlayAudioFX;
    
    public delegate void OnStopAudioFXHandler(AudioFXId id);
    public static event OnStopAudioFXHandler OnStopAudioFX;
    
    public static void TriggerPlayAudioFX(AudioFXId id)
    {
        OnPlayAudioFX?.Invoke(id);
    }

    public static void TriggerStopAudioFX(AudioFXId id)
    {
        OnStopAudioFX?.Invoke(id);
    }
}
