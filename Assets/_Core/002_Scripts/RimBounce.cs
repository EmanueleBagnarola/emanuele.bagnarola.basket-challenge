using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RimBounce : MonoBehaviour
{
    [SerializeField] private Vector3 _bounceAxis;
    [SerializeField] private float _bouncePower;
    [SerializeField] private float _bounceDuration;
    [SerializeField] private int _bounceVibrato;
    [SerializeField] private float _bounceElasticity;

    private void Awake()
    {
        AnimationEvents.OnRimTouched += Bounce;
    }

    private void OnDestroy()
    {
        AnimationEvents.OnRimTouched -= Bounce;
    }

    [Button]
    public void Bounce()
    {
        transform.DOPunchRotation(_bounceAxis * _bouncePower, _bounceDuration, _bounceVibrato, _bounceElasticity);
    }
}
