using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    protected PlayerInputActions inputActions;

    [field: SerializeField] public Vector2 MoveInput { get; protected set; }

    [SerializeField] private float jumpBufferTime = 0.2f;   //预输入缓存
    [SerializeField] private float lastJumpPressedTime = -1f;
    [SerializeField] private float coyoteTime = 0.2f;    //土狼时间
    [SerializeField] private float lastGroundedTime = -1f;

    protected virtual void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    protected void OnDestroy()
    {
        inputActions?.Dispose();
    }

    protected void OnJump(InputAction.CallbackContext context) => lastJumpPressedTime = Time.time;

    public void SetGroundedTime() => lastGroundedTime = Time.time;
    
    public bool CheckJumpPressed()
    {
        bool isBufferTime = Time.time - lastJumpPressedTime < jumpBufferTime;
        bool isCoyoteTime = Time.time - lastGroundedTime < coyoteTime;

        if (isBufferTime && isCoyoteTime)
        {
            lastJumpPressedTime = -1f;
            lastGroundedTime = -1f; //对输入进行消费，防止重复输入导致问题
            return true;
        }

        return false;
    }
}
