using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorseUnGroundedState : HorseState
{
    public HorseUnGroundedState(Horse _player, EntityStateMachine<Horse> _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
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

        if (player.IsGrounded && !player.IsOnSlope())
        {
            stateMachine.ChangeState(player.groundState);
            return;
        }
        if (player.InputReader.CheckJumpPressed() && !player.IsOnSlope())
        {
            stateMachine.ChangeState(player.jumpState);
            return;
        }

        player.AddVerticalSpeed();
        player.anim.SetFloat("AirSpeed", player.verticalSpeed);

        player.IsObstacleInFront(); //后续要更改逻辑，因为撞墙的头部逻辑不同
    }
}
