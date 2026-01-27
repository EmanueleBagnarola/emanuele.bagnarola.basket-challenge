using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

public static class InputEvents
{
    public delegate void PointerDragHandler(Vector2 currentPosition, Vector2 startPosition);
    public static event PointerDragHandler OnPointerDrag;
    
    public delegate void PointerUpHandler(Vector2 currentPosition);
    public static event PointerUpHandler OnPointerUp;
    
    // ---- Event Triggers ----

    public static void TriggerPointerDrag(Vector2 currentPosition, Vector2 startPosition)
    {
        OnPointerDrag?.Invoke(currentPosition, startPosition);
    }

    public static void TriggerPointerUp(Vector2 currentPosition)
    {
        OnPointerUp?.Invoke(currentPosition);
    }
}