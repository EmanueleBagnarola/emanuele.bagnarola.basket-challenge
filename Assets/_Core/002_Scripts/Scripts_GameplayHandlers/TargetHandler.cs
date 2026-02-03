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

    [Header("Backboard fail ground target config")]
    [SerializeField] private Transform _backboardFailGroundTarget;
    [SerializeField] private float _minBackboardGroundX = -2.7f;
    [SerializeField] private float _maxBackboardGroundX = 2.7f;
    
    [Header("Direct fail ground target config")]
    [SerializeField] private Transform _directFailGroundTarget;
    [SerializeField] private float _minDirectGroundX;
    [SerializeField] private float _maxDirectGroundX;
    
    [Header("Frame target config")]
    [SerializeField] private Transform _frameTargetPivot;
    [SerializeField] private Transform _frameTarget;
    [SerializeField] private Transform _frameFailTarget;
    [SerializeField] private float _minRot;
    [SerializeField] private float _maxRot;

    [Header("Score target config")]
    [SerializeField] private Transform _scoreTarget;

    private void Awake()
    {
        InitializeRuntimeTargetService();
    }

    private void Update()
    {
        UpdateBackboardTargetPosition();

        UpdateFrameTargetRotation();
        
        UpdateGroundTargetPosition();
    }

    /// <summary>
    /// Initialize the targets references in the TargetService 
    /// </summary>
    private void InitializeRuntimeTargetService()
    {
        
        RuntimeServices.TargetService.ScoreTarget = _scoreTarget;
        RuntimeServices.TargetService.BackboardTarget = _backboardTarget;
        RuntimeServices.TargetService.FrameTarget = _frameTarget;
        RuntimeServices.TargetService.FrameFailTarget = _frameFailTarget;
        RuntimeServices.TargetService.BackboardFailGroundTarget = _backboardFailGroundTarget;
        RuntimeServices.TargetService.DirectFailGroundTarget = _directFailGroundTarget;
    }

    /// <summary>
    /// Move the backboard target position relative to player position
    /// </summary>
    private void UpdateBackboardTargetPosition()
    {
        float backboardTargetX = GameUtils.Map(_playerTransform.position.x, _minPlayerX, _maxPlayerX, _minTargetX, _maxTargetX);
        _backboardTarget.localPosition = new Vector3(Mathf.Clamp(backboardTargetX, _minPlayerX, _maxTargetX), _backboardTarget.localPosition.y, _backboardTarget.localPosition.z);
    }

    /// <summary>
    /// Rotate the pivot of the frame target relative to backboard target (opposite)
    /// </summary>
    private void UpdateFrameTargetRotation()
    {
        float frameTargetAngle = GameUtils.Map(_backboardTarget.localPosition.x, _minTargetX, _maxTargetX, _minRot, _maxRot);
        _frameTargetPivot.localEulerAngles = new Vector3(0, Mathf.Clamp(frameTargetAngle, _minRot, _maxRot), 0);
    }

    /// <summary>
    /// Move the ground targets position (backboard and direct)
    /// </summary>
    private void UpdateGroundTargetPosition()
    {
        float backboardGroundTargetX = GameUtils.Map(_backboardTarget.localPosition.x, _maxTargetX, _minTargetX, _minBackboardGroundX, _maxBackboardGroundX);
        _backboardFailGroundTarget.localPosition = new Vector3(Mathf.Clamp(backboardGroundTargetX, _minBackboardGroundX, _maxBackboardGroundX), _backboardFailGroundTarget.localPosition.y, _backboardFailGroundTarget.localPosition.z);
        
        float directGroundTargetX = GameUtils.Map(_playerTransform.localPosition.x, _minPlayerX, _maxPlayerX, _minDirectGroundX, _maxDirectGroundX);
        _directFailGroundTarget.localPosition = new Vector3(Mathf.Clamp(directGroundTargetX, _minDirectGroundX, _maxDirectGroundX), _directFailGroundTarget.localPosition.y, _directFailGroundTarget.localPosition.z);
    }
}
