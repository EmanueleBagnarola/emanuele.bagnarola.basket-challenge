using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PunchScaleLoop : MonoBehaviour
{
    [SerializeField] private float _scale;
    [SerializeField] private float _duration;
    [SerializeField] private int _vibration = 10;
    [SerializeField] private float _elasticity = 1;
    
    private void Start()
    {
        transform.DOPunchScale(Vector3.one * _scale, _duration, _vibration,  _elasticity).SetLoops(-1, LoopType.Restart);
    }
}
