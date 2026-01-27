using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CinemachineShake : MonoBehaviour
{
    private CinemachineVirtualCamera vcam;
    private CinemachineBasicMultiChannelPerlin perlin;
    private float shakeTimer;
    private float shakeTimerTotal;
    private float startingIntensity;
    
    private void Awake()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        perlin = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            perlin.m_AmplitudeGain = Mathf.Lerp(startingIntensity, 0.0f, (1-(shakeTimer / shakeTimerTotal)));
        }
    }

    public void Shake(float intensity, float duration)
    {
        perlin.m_AmplitudeGain = intensity;
        startingIntensity = intensity;
        shakeTimerTotal = duration;
        shakeTimer = duration;
    }
}
