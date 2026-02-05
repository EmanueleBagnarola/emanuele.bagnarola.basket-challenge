using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Radishmouse;
using Unity.VisualScripting;
using UnityEngine;

public class InputFeedbackGUI : MonoBehaviour
{
    [SerializeField] private UILineRenderer _uiLineRenderer;
    
    private bool _dragging;

    private void Awake()
    {
        InputEvents.OnPointerDown += OnPointerDown;
        InputEvents.OnPointerDrag += OnPointerDrag;
        InputEvents.OnPointerUp += OnPointerUp;
    }

    private void OnDestroy()
    {
        InputEvents.OnPointerDown -= OnPointerDown;
        InputEvents.OnPointerDrag -= OnPointerDrag;
        InputEvents.OnPointerUp -= OnPointerUp;
    }

    private void OnPointerDown(Vector2 pos)
    {
        _dragging = true;
        _uiLineRenderer.Clear();
        _uiLineRenderer.AddPoint(pos);
    }

    private void OnPointerDrag(Vector2 currentPos, Vector2 startPos)
    {
        if (!_dragging) return;
        _uiLineRenderer.AddPoint(currentPos);
    }

    private void OnPointerUp()
    {
        _dragging = false;
        _uiLineRenderer.Clear();
    }
}
