using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShootSettings", menuName = "ScriptableObjects/ShootSettings")]
public class ShootSettings : ScriptableObject
{
    [field:Header("Base - Shoot")]
    [field:SerializeField] public float ShootForce { get; private set; } =  1.1f;
    [field:SerializeField] public float ShootDuration { get; private set; } = 1.1f;
    
    [field:Header("Accurate - Backboard to frame")]
    [field:SerializeField] public float BounceForce { get; private set; } = 0.1f;
    [field:SerializeField] public float BounceDuration { get; private set; } = 0.15f;
    
    [field:Header("Accurate - Frame to loop")]
    [field:SerializeField] public float RimToScoreForce { get; private set; } = 0.05f;
    [field:SerializeField] public float RimToScoreDuration { get; private set; } = 0.1f;
    
    [field:Header("Fail - Backboard to ground")]
    [field:SerializeField] public float BounceToGroundForce { get; private set; } = 1.5f;
    [field:SerializeField] public float BounceToGroundDuration { get; private set; }= 0.8f;
    
    [field:Header("Fail - Shoot to ground")]
    [field:SerializeField] public float ShootToGroundForce { get; private set; } = 3;
    [field:SerializeField] public float ShootToGroundDuration { get; private set; } = 1.1f;
    
    [field:Header("Accuracy thresholds")]
    [field:SerializeField] public float DirectAccuracyThreshold { get; private set; } = 5f;
    [field:SerializeField] public float BackboardAccuracyThreshold { get; private set; } = 2f;
    
    [field:Header("Randomizer values")]
    [field:SerializeField] public float RandomOffsetInsideCircleRadius { get; private set; } = 0.15f;
}
