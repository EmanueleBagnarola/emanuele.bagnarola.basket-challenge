using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioHandler : MonoBehaviour
{ 
    [SerializeField, NonReorderable] private List<AudioFX> _audioFXList = new List<AudioFX>();
    
    [Header("Debug")]
    [SerializeField] private AudioFXId _debugAudioFX;

    private void Awake()
    {
        InitAudioSources();

        AudioEvents.OnPlayAudioFX += PlayAudioFX;
        AudioEvents.OnStopAudioFX += StopAudioFX;
    }

    private void OnDestroy()
    {
        AudioEvents.OnPlayAudioFX -= PlayAudioFX;
        AudioEvents.OnStopAudioFX -= StopAudioFX;
    }

    private void InitAudioSources()
    {
        foreach (var audioFX in _audioFXList)
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioFX.AudioSource = audioSource;
        }
    }

    private void PlayAudioFX(AudioFXId id)
    {
        AudioFX audioFX = GetAudioFX(id);
        audioFX.Play();
    }

    private void StopAudioFX(AudioFXId id)
    {
        AudioFX audioFX = GetAudioFX(id);
        audioFX.Stop();
    }
    
    private AudioFX GetAudioFX(AudioFXId id)
    {
        return _audioFXList.Find(x => x.Id == id);
    }

    [Button]
    public void Debug_PlayAudioFX()
    {
        PlayAudioFX(_debugAudioFX);
    }
}

[System.Serializable]
public class AudioFX
{
    public AudioFXId Id;
    public List<AudioClip> AudioClipsPool = new List<AudioClip>();
    public float Volume;
    public float Pitch;
    public bool Loop;
    [HideInInspector] public AudioSource AudioSource;

    public void Play()
    {
        AudioSource.volume = Volume;
        AudioSource.pitch = Pitch;
        AudioSource.loop = Loop;

        if (Loop)
        {
            AudioSource.clip = GetRandomAudioClip();
            AudioSource.Play();
        }
        else
        {
            AudioSource.PlayOneShot(GetRandomAudioClip());
        }
    }

    public void Stop()
    {
        AudioSource.Stop();
    }
    
    private AudioClip GetRandomAudioClip()
    {
        return AudioClipsPool[Random.Range(0, AudioClipsPool.Count)];
    }
}

public enum AudioFXId
{
    BounceBackboard,
    BounceRim,
    Shoot,
    Perfect,
    Accurate,
    Countdown_Progress,
    Countdown_End,
    Win,
    Lose,
    Draw,
    Fireball,
}
