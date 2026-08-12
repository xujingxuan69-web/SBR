using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorseGroundState : EntityState<Horse>
{
    public HorseGroundState(Horse _player, EntityStateMachine<Horse> _stateMachine, string _animBoolName)
        : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        HandleMovement();

        if (player.IsGrounded)
        {
            stateTimer = 0.2f;
            player.ResetVerticalSpeed();
        }
        else if (stateTimer < 0 || player.IsOnSlope())
        {
            Debug.Log("Change To FallState");
            stateMachine.ChangeState(player.fallState);
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