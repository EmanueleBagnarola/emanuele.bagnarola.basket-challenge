using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPlayerRotation : MonoBehaviour
{
    [SerializeField] private Transform _lookAtTransform;
    [SerializeField] private bool _isHumanPlayer;
    
    private void Awake()
    {
        GameModeEvents.OnShootPositionUpdated += OnShootPositionUpdated;
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShootPositionUpdated -= OnShootPositionUpdated;
    }

    /// <summary>
    /// Rotate the player to face the hoop when next shot position is set
    /// </summary>
    private void OnShootPositionUpdated(bool isHumanPlayer)
    {
        if(_isHumanPlayer != isHumanPlayer)
            return;
        
        transform.LookAt(_lookAtTransform);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }
}
