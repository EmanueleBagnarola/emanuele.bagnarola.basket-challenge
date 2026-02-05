using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "ShootSettings", menuName = "ScriptableObjects/ShootSettings")]
public class ShootSettings : ScriptableObject
{
    [field:Header("General")]
    // The time to wait after the input attempt is called (used for player jump animation)
    [field: SerializeField] public float ShootWaitTime { get; private set; } = 0.5f;
    // The time to wait to reset the player on next shot position
    [field: SerializeField] public float NextShootWaitTime { get; private set; } = 0.7f;
    
    // Max time to check to keep the touch input for shooting attempt valid
    [field: Header("Input settings")]
    [field: SerializeField] public float ShootInputMaxTime { get; private set; } = 2;
    [field: SerializeField] public float InputSensitivity { get; private set; } = 1;
    
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
    
    [field:Header("Bounce physics settings")]
    [field:SerializeField] public float FinalSimulatedBounceMultiplier { get; private set; } = 1;
    
    [field:Header("Ease settings")]
    [field:SerializeField] public Ease ShootEase { get; private set; } = Ease.Linear;
    [field:SerializeField] public Ease BounceEase { get; private set; } = Ease.Linear;

    [field:Header("Shot strength relative Settings")]
    [field:SerializeField] public float StrongShootBackboardTargetYOffset { get; private set; } = 0.25f;
    
    [field:Header("Failed shot check time")]
    [field:SerializeField] public float DirectFailedCheckTime { get; private set; } = 0.6f;
    [field:SerializeField] public float BackboardFailedCheckTime { get; private set; } = 0.3f;

    /// <summary>
    /// Get the current shot time to use as value to call the next shot position update
    /// </summary>
    /// <returns></returns>
    public float GetShotValidateTime(ShootResult shootResult)
    {
        switch (shootResult.Type)
        {
            case ShootType.Direct:
                switch (shootResult.Accuracy)
                {
                    case ShootAccuracy.Perfect:
                        return ShootDuration;
                    
                    case ShootAccuracy.Accurate:
                        return ShootDuration + RimToScoreDuration;
                    
                    case ShootAccuracy.Fail:
                        return DirectFailedCheckTime;
                }
                break;
            
            case ShootType.Backboard:
                switch (shootResult.Accuracy)
                {
                    case ShootAccuracy.Perfect:
                        return ShootDuration + BounceDuration;
                    
                    case ShootAccuracy.Accurate:
                        return ShootDuration + BounceDuration + RimToScoreDuration;
                    
                    case ShootAccuracy.Fail:
                        return ShootDuration + BounceDuration + BackboardFailedCheckTime;
                }
                break;
        }

        return ShootDuration;
    }
}
