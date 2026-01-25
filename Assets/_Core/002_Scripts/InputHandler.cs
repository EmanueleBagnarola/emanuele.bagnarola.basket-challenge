using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handle mouse/touch inputs and fire relative events
/// </summary>
public class InputHandler : MonoBehaviour
{ 
    private InputSystem_Actions _inputSystemActions;
    private bool _pressStarted;
    private InputAction _pointerPosition;
    private InputAction _pointerPress;
    private Vector2 _startPosition;

    private void Awake()
    {
        InitializeInputSystemActions();
    }
    
    private void OnEnable()
    {
        _inputSystemActions.Enable();
        _pointerPosition.Enable();
        _pointerPress.Enable();

        _pointerPress.started += OnPressStarted;
        _pointerPress.canceled += OnPressEnded;
    }

    private void OnDisable()
    {
        _pointerPress.started -= OnPressStarted;
        _pointerPress.canceled -= OnPressEnded;

        _inputSystemActions.Disable();
        _pointerPosition.Disable();
        _pointerPress.Disable();
    }

    private void Update()
    {
        ReadPointerDrag();
    }

    private void InitializeInputSystemActions()
    {
        _inputSystemActions = new InputSystem_Actions();
        _pointerPosition = _inputSystemActions.Pointer.Position;
        _pointerPress = _inputSystemActions.Pointer.Press;
    }

    private void ReadPointerDrag()
    {
        if(!_pressStarted)
            return;
        
        Vector2 currentPos = _pointerPosition.ReadValue<Vector2>();
        InputEvents.OnPointerDrag.Invoke(currentPos, _startPosition);
    }

    private void OnPressStarted(InputAction.CallbackContext ctx)
    {
        _startPosition = _pointerPosition.ReadValue<Vector2>();
        _pressStarted = true;
    }

    private void OnPressEnded(InputAction.CallbackContext ctx)
    {
        _pressStarted = false;
        Vector2 pos = _pointerPosition.ReadValue<Vector2>();
        InputEvents.OnPointerUp?.Invoke(pos);
    }
}
