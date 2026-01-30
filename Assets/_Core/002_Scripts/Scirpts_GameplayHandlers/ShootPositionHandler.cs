using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootPositionHandler : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Transform _shootRangeCenter;
    [SerializeField, NonReorderable] private List<ShootRange> _shootRangesByPhase; //NonReorderable attribute added to fix the editor serialized class visualization but
    
    private ShootRange currentShootRange;

    private void Awake()
    {
        GameModeEvents.OnCallNewPosition += UpdateCurrentShootPosition;
    }

    private void OnDestroy()
    {
        GameModeEvents.OnCallNewPosition -= UpdateCurrentShootPosition;
    }

    private void UpdateCurrentShootPosition()
    {
        currentShootRange = GeShootPositionsPoolByPhase();
        Vector3 shootPosition = GetRandomPointOnShootRange(currentShootRange);
        _playerTransform.position = shootPosition;

        GameModeEvents.TriggerShootPositionUpdated();
    }
    
    private Vector3 GetRandomPointOnShootRange(ShootRange range)
    {
        float angleDeg = UnityEngine.Random.Range(range.AngleMin, range.AngleMax);
        float rad = angleDeg * Mathf.Deg2Rad;

        return new Vector3(
            _shootRangeCenter.position.x + Mathf.Cos(rad) * range.RangeRadius,
            0,
            _shootRangeCenter.position.z + Mathf.Sin(rad) * range.RangeRadius
        );
    }

    private ShootRange GeShootPositionsPoolByPhase()
    {
        return _shootRangesByPhase.Find(p => p.Phase == RuntimeServices.GameModeService.CurrentPhase);
    }
    
        
    // ----- SHOOT RANGE GIZMO DRAW ------
    private void OnDrawGizmos()
    {
        if (_shootRangesByPhase == null)
            return;

        foreach (var range in _shootRangesByPhase)
        {
            Color gizmoColor = Color.white;
            switch (range.Phase)
            {
                case GameModePhase.Early:
                    gizmoColor = Color.yellow;
                    break;
                
                case GameModePhase.Mid:
                    gizmoColor = Color.green;
                    break;
                
                case GameModePhase.Late:
                    gizmoColor = Color.blue;
                    break;
            }
            
            DrawArcXZ(
                _shootRangeCenter.position,
                range.RangeRadius,
                range.AngleMin,
                range.AngleMax,
                gizmoColor
            );
        }
    }
    
    private void DrawArcXZ(Vector3 center, float radius, float angleMinDeg, float angleMaxDeg, Color color)
    {
        Gizmos.color = color;
        int segments = 10;

        float step = (angleMaxDeg - angleMinDeg) / segments;
        Vector3 prevPoint = GetPoint(center, radius, angleMinDeg);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleMinDeg + step * i;
            Vector3 nextPoint = GetPoint(center, radius, angle);

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
        
        Gizmos.DrawLine(center, GetPoint(center, radius, angleMinDeg));
        Gizmos.DrawLine(center, GetPoint(center, radius, angleMaxDeg));
    }

    private Vector3 GetPoint(Vector3 center, float radius, float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;

        return new Vector3(
            center.x + Mathf.Cos(rad) * radius,
            center.y,
            center.z + Mathf.Sin(rad) * radius
        );
    }
}

/// <summary>
/// For each game mode phase, choose the radius and the angle of the area in which the next shot position could be chosen from
/// </summary>
[Serializable]
public struct ShootRange
{
    public GameModePhase Phase;
    public float RangeRadius;
    
    [Range(-180f, 180f)]
    public float AngleMin;
    
    [Range(-180f, 180f)]
    public float AngleMax;
}

