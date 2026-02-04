using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHandler : MonoBehaviour
{
    [Header("ADAPTABLE TARGETS")]
    [Header("Player config")]
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Transform _aiTransform;
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

    private Vector3 _initialFrameTargetPos;
    private Vector3 _initialFrameFailTargetPos;

    private void Awake()
    {
        _initialFrameTargetPos = _frameTarget.position;
        _initialFrameFailTargetPos = _frameFailTarget.position;
        
        InitializeRuntimeTargetService();

        GameModeEvents.OnShootPositionUpdated += OnShootPositionUpdated;
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShootPositionUpdated -= OnShootPositionUpdated;
    }

    private void OnShootPositionUpdated(bool _isHumanPlayer)
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
        RuntimeServices.TargetService.ScoreTargetPos = _scoreTarget.position;
    }

    /// <summary>
    /// Move the backboard target position relative to player position
    /// </summary>
    private void UpdateBackboardTargetPosition()
    {
        float playerBackboardTargetX = GetBackboardTargetX(_playerTransform);
        float aiBackboardTargetX = GetBackboardTargetX(_aiTransform);
        
        RuntimeServices.TargetService.PlayerTargetState.BackboardTargetPos = new Vector3(Mathf.Clamp(playerBackboardTargetX, _minPlayerX, _maxTargetX), _backboardTarget.position.y, _backboardTarget.position.z);
        RuntimeServices.TargetService.AITargetState.BackboardTargetPos = new Vector3(Mathf.Clamp(aiBackboardTargetX, _minPlayerX, _maxTargetX), _backboardTarget.position.y, _backboardTarget.position.z);
    }

    private float GetBackboardTargetX(Transform _characterTransform)
    {
        return GameUtils.Map(_characterTransform.position.x, _minPlayerX, _maxPlayerX, _minTargetX, _maxTargetX);
    }

    /// <summary>
    /// Rotate the pivot of the frame target relative to backboard target (opposite)
    /// </summary>
    private void UpdateFrameTargetRotation()
    {
        SetFrameTargetPosition(RuntimeServices.TargetService.PlayerTargetState.BackboardTargetPos, out RuntimeServices.TargetService.PlayerTargetState.FrameTargetPos, out RuntimeServices.TargetService.PlayerTargetState.FrameFailTargetPos);
        SetFrameTargetPosition(RuntimeServices.TargetService.AITargetState.BackboardTargetPos, out RuntimeServices.TargetService.AITargetState.FrameTargetPos, out RuntimeServices.TargetService.AITargetState.FrameFailTargetPos);
    }

    private void SetFrameTargetPosition(Vector3 backboardTargetPos, out Vector3 destinationFrameTargetPos, out Vector3 destinationFrameFailTargetPos)
    {
        float frameTargetAngle = GameUtils.Map( backboardTargetPos.x, _minTargetX, _maxTargetX, _minRot, _maxRot);
        frameTargetAngle = Mathf.Clamp(frameTargetAngle, _minRot, _maxRot);
        
        Vector3 frameTargetOffset = _initialFrameTargetPos - _frameTargetPivot.position;
        Vector3 frameFailTargetOffset = _initialFrameFailTargetPos - _frameTargetPivot.position;
        
        Quaternion framePivotRot = Quaternion.AngleAxis(frameTargetAngle, Vector3.up);
        
        destinationFrameTargetPos = _frameTargetPivot.position + framePivotRot * frameTargetOffset;
        destinationFrameFailTargetPos = _frameTargetPivot.position + framePivotRot * frameFailTargetOffset;
    }

    /// <summary>
    /// Move the ground targets position (backboard and direct)
    /// </summary>
    private void UpdateGroundTargetPosition()
    {
        RuntimeServices.TargetService.PlayerTargetState.BackboardFailGroundTargetPos = GetBackboardGroundTarget(RuntimeServices.TargetService.PlayerTargetState.BackboardTargetPos);
        RuntimeServices.TargetService.AITargetState.BackboardFailGroundTargetPos = GetBackboardGroundTarget(RuntimeServices.TargetService.AITargetState.BackboardTargetPos);

        RuntimeServices.TargetService.PlayerTargetState.DirectFailGroundTargetPos = GetDirectGroundTarget(_playerTransform);
        RuntimeServices.TargetService.AITargetState.DirectFailGroundTargetPos = GetDirectGroundTarget(_aiTransform);
    }

    private Vector3 GetBackboardGroundTarget(Vector3 backboardTargetPos)
    {
        float backboardGroundTargetX = GameUtils.Map(backboardTargetPos.x, _maxTargetX, _minTargetX, _minBackboardGroundX, _maxBackboardGroundX);
        return new Vector3(Mathf.Clamp(backboardGroundTargetX, _minBackboardGroundX, _maxBackboardGroundX), _backboardFailGroundTarget.position.y, _backboardFailGroundTarget.position.z);
    }

    private Vector3 GetDirectGroundTarget(Transform characterTransform)
    {
        float directGroundTargetX = GameUtils.Map(characterTransform.position.x, _minPlayerX, _maxPlayerX, _minDirectGroundX, _maxDirectGroundX);
        return new Vector3(Mathf.Clamp(directGroundTargetX, _minDirectGroundX, _maxDirectGroundX), _directFailGroundTarget.position.y, _directFailGroundTarget.position.z);
    }
}
