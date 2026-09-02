using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    private PlayerInputActions inputActions = new PlayerInputActions();

    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }

    private void OnEnable()
    {
        inputActions.Player_Horse.Enable();
        inputActions.Player_Horse.Jump.performed += OnJump;
    }

    private void OnDisable()
    {
        inputActions.Player_Horse.Jump.performed -= OnJump;
        inputActions.Player_Horse.Disable();
    }

    private void FixedUpdate()
    {
        MoveInput = inputActions.Player_Horse.Move.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        JumpPressed = true;
    }

    public bool ConsumeJumpPressed()
    {
        if (!JumpPressed) return false;
        JumpPressed = false;
        return true;
    }

    private void OnDestroy()
    {
        inputActions?.Dispose();
    }
}
