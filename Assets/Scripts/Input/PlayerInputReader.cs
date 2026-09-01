using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputReader : MonoBehaviour
{
    private PlayerInputActions inputActions;

    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }
}
