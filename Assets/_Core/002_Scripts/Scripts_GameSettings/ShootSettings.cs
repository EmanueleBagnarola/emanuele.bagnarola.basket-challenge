using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShootSettings", menuName = "ScriptableObjects/ShootSettings")]
public class ShootSettings : ScriptableObject
{
    [field:Header("Base - Shoot")]
    // The height/duration of the tween curve when shot is started (from hand)
    [field:SerializeField] public float ShootForce { get; private set; } =  1.1f;
    [field:SerializeField] public float ShootDuration { get; private set; } = 1.1f;
    
    [field:Header("Accurate - Backboard to frame")]
    // The height/duration of the tween curve when ball bounces (from backboard or rim)
    [field:SerializeField] public float BounceForce { get; private set; } = 0.1f;
    [field:SerializeField] public float BounceDuration { get; private set; } = 0.15f;
    
    [field:Header("Accurate - Frame to loop")]
    // The height/duration of the tween curve when ball need to reach the score target from the rim 
    [field:SerializeField] public float RimToScoreForce { get; private set; } = 0.05f;
    [field:SerializeField] public float RimToScoreDuration { get; private set; } = 0.1f;
    
    [field:Header("Fail - Backboard to ground")]
    // The height/duration of the tween curve when ball need to reach the ground target after a bounce
    [field:SerializeField] public float BounceToGroundForce { get; private set; } = 1.5f;
    [field:SerializeField] public float BounceToGroundDuration { get; private set; }= 0.8f;
    
    [field:Header("Fail - Shoot to ground")]
    // The height/duration of the tween curve when ball need to reach the ground target after a direct shot fail
    [field:SerializeField] public float ShootToGroundForce { get; private set; } = 3;
    [field:SerializeField] public float ShootToGroundDuration { get; private set; } = 1.1f;
    
    [field:Header("Accuracy thresholds")]
    // Threshold limits to consider when deciding if a not perfect shot could be still considered as accurate
    [field:SerializeField] public float DirectAccuracyThreshold { get; private set; } = 5f;
    [field:SerializeField] public float BackboardAccuracyThreshold { get; private set; } = 2f;
    
    [field:Header("Randomizer values")]
    // Radius to add to the random offset inside a circe to randomize the curve path step position
    [field:SerializeField] public float RandomOffsetInsideCircleRadius { get; private set; } = 0.15f;
}
