using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    [SerializeField] private Transform _target;

    private void Update()
    {
        transform.LookAt(_target);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }
}
