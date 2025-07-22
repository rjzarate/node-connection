using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInputManager : Singleton<PlayerInputManager>
{
    internal PlayerInput PlayerInput { get; private set; }
    public bool IsSprinting { get; private set; }

    private void Awake()
    {
        PlayerInput = new PlayerInput();
        PlayerInput.Player.Enable();

        PlayerInput.Player.Jump.performed += Jump_performed;
        PlayerInput.Player.Sprint.performed += Sprint_performed;
        PlayerInput.Player.Sprint.canceled += Sprint_canceled;
    }

    private void OnDestroy()
    {
        PlayerInput.Player.Jump.performed -= Jump_performed;
        PlayerInput.Player.Sprint.performed -= Sprint_performed;
        PlayerInput.Player.Sprint.canceled -= Sprint_canceled;

        PlayerInput.Dispose();
    }

    private void Sprint_canceled(InputAction.CallbackContext context)
    {
        IsSprinting = false;
    }

    private void Sprint_performed(InputAction.CallbackContext context)
    {
        IsSprinting = true;
    }

    private void Jump_performed(InputAction.CallbackContext context)
    {
        JumpEvent?.Invoke();
    }

    public delegate void JumpEventHandler();
    public event JumpEventHandler JumpEvent;

    public float GetMouseDeltaX()
    {
        return Mouse.current.delta.x.ReadValue();
    }

    public float GetMouseDeltaY()
    {
        return Mouse.current.delta.y.ReadValue();
    }

    public Vector3 GetNormalizedMovement()
    {
        Vector2 inputVector = PlayerInput.Player.Move.ReadValue<Vector2>();

        inputVector = inputVector.normalized;

        return new Vector3(inputVector.x, 0, inputVector.y);
    }
}
