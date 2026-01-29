using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

public static class InputEvents
{
    public delegate void PointerDragHandler(Vector2 currentPosition, Vector2 startPosition);
    public static event PointerDragHandler OnPointerDrag;
    
    public delegate void PointerUpHandler();
    public static event PointerUpHandler OnPointerUp;
    
    public delegate void PointerDownHandler(Vector2 inputPosition);
    public static event PointerDownHandler OnPointerDown;
    
    // ---- Event Triggers ----

    public static void TriggerPointerDrag(Vector2 currentPosition, Vector2 startPosition)
    {
        OnPointerDrag?.Invoke(currentPosition, startPosition);
    }

    public static void TriggerPointerDown(Vector2 inputPosition)
    {
        OnPointerDown?.Invoke(inputPosition);
    }

    public static void TriggerPointerUp()
    {
        OnPointerUp?.Invoke();
    }
}