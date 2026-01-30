using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHandler : MonoBehaviour
{
    [Header("ADAPTABLE TARGETS")]
    [Header("Player config")]
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _minPlayerX;
    [SerializeField] private float _maxPlayerX;

    [Header("Backboard target config")]
    [SerializeField] private Transform _backboardTarget;
    [SerializeField] private float _minTargetX;
    [SerializeField] private float _maxTargetX;

    [Header("Ground target config")]
    [SerializeField] private Transform _backboardFailGroundTarget;
    [SerializeField] private float _minGroundX;
    [SerializeField] private float _maxGroundX;
    
    [Header("Frame target config")]
    [SerializeField] private Transform _frameTargetPivot;
    [SerializeField] private Transform _frameTarget;
    [SerializeField] private Transform _frameFailTarget;
    [SerializeField] private float _minRot;
    [SerializeField] private float _maxRot;

    [Header("STATIC TARGETS")]
    [SerializeField] private Transform _scoreTarget;
    [SerializeField] private Transform _leftDirectFailGroundTarget;
    [SerializeField] private Transform _rightDirectFailGroundTarget;

    private void Awake()
    {
        RuntimeServices.TargetService.ScoreTarget = _scoreTarget;
        RuntimeServices.TargetService.BackboardTarget = _backboardTarget;
        RuntimeServices.TargetService.FrameTarget = _frameTarget;
        RuntimeServices.TargetService.FrameFailTarget = _frameFailTarget;
        RuntimeServices.TargetService.BackboardFailGroundTarget = _backboardFailGroundTarget;
        RuntimeServices.TargetService.LeftDirectFailGroundTarget = _leftDirectFailGroundTarget;
        RuntimeServices.TargetService.RightDirectFailGroundTarget = _rightDirectFailGroundTarget;
    }

    private void Update()
    {
        UpdateBackboardTargetPosition();

        UpdateFrameTargetRotation();
        
        UpdateGroundTargetPosition();
    }

    private void UpdateBackboardTargetPosition()
    {
        float backboardTargetX = GameUtils.Map(_playerTransform.position.x, _minPlayerX, _maxPlayerX, _minTargetX, _maxTargetX);
        _backboardTarget.localPosition = new Vector3(Mathf.Clamp(backboardTargetX, _minPlayerX, _maxTargetX), _backboardTarget.localPosition.y, _backboardTarget.localPosition.z);
    }

    private void UpdateFrameTargetRotation()
    {
        float frameTargetAngle = GameUtils.Map(_backboardTarget.localPosition.x, _minTargetX, _maxTargetX, _minRot, _maxRot);
        _frameTargetPivot.localEulerAngles = new Vector3(0, Mathf.Clamp(frameTargetAngle, _minRot, _maxRot), 0);
    }

    private void UpdateGroundTargetPosition()
    {
        float groundTargetX = GameUtils.Map(_backboardTarget.localPosition.x, _maxTargetX, _minTargetX, _minGroundX, _maxGroundX);
        _backboardFailGroundTarget.localPosition = new Vector3(Mathf.Clamp(groundTargetX, _minGroundX, _maxGroundX), _backboardFailGroundTarget.localPosition.y, _backboardFailGroundTarget.localPosition.z);
    }
}
