using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPlayerRotation : MonoBehaviour
{
    [SerializeField] private Transform _lookAtTransform;
    
    private void Awake()
    {
        GameModeEvents.OnShootPositionUpdated += OnShootPositionUpdated;
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShootPositionUpdated -= OnShootPositionUpdated;
    }

    private void OnShootPositionUpdated()
    {
        transform.LookAt(_lookAtTransform);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }
}
