using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioFXSource3D : MonoBehaviour
{
    [SerializeField] private AudioFX _audioFX;
    [SerializeField] private AudioSource _audioSource;

    private void Awake()
    {
        InitAudioSource();
        
        AudioEvents.OnPlayAudioFX += PlayAudioFX;

    }

    private void OnDestroy()
    {
        AudioEvents.OnPlayAudioFX -= PlayAudioFX;
    }

    private void InitAudioSource()
    {
        _audioFX.AudioSource = _audioSource;
    }

    private void PlayAudioFX(AudioFXId id)
    {
        if(_audioFX.Id != id)
            return;
        
        _audioFX.Play();
    }
}
