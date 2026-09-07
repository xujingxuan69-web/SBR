using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorseGroundedState : HorseState
{
    public HorseGroundedState(Horse _player, EntityStateMachine<Horse> _stateMachine, string _animBoolName)
        : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.InputReader.SetGroundedTime();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        HandleMovement();
        
        if (player.IsGrounded || player.IsOnSlope())
        {
            stateTimer = 0.2f;
            player.ResetVerticalSpeed();
            player.InputReader.SetGroundedTime();
        }

        
        if (stateTimer < 0)
        {
            stateMachine.ChangeState(player.airState);
            return;
        }

        if (!player.IsOnSlope())
        {
            if (player.InputReader.CheckJumpPressed()) //必须呈包含关系，否则跳跃会被提前消耗掉
            {
                stateMachine.ChangeState(player.jumpState);
                return;
            }

        }
    }

    private void HandleMovement()
    {
        float acc = 0f;

        if (verticalInput > 0.1f) acc = player.forwardAcceleration;
        else if (verticalInput < -0.1f) acc = -player.backwardAcceleration;
        else
        {   //无输入，自动降速
            if (player.IsMoving)
            {
                acc = -Mathf.Sign(player.horizontalSpeed) * player.deceleration;
            }
            else
            {
                player.SetHorizontalSpeedAs(0f);
                return;
            }
        }

        bool isOpposite = Mathf.Sign(verticalInput) != Mathf.Sign(player.horizontalSpeed)  //相反按键急停
                          && Mathf.Abs(verticalInput) > 0.1f
                          && player.IsMoving;

        if (isOpposite) acc *= 2f;

        player.ChangeHorizontalSpeedBy(acc);

        if (player.IsMoving) player.Turn(horizontalInput);  //转向控制

        player.anim.SetFloat("GroundSpeed", Mathf.Abs(player.horizontalSpeed / player.maxForwardSpeed));
    }
}
