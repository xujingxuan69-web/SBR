using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorseInputReader : PlayerInputReader
{
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

}
