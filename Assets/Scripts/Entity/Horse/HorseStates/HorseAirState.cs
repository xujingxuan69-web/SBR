using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorseAirState : EntityState<Horse>
{
    public HorseAirState(Horse _player, EntityStateMachine<Horse> _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
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
        }
        else
        {
            player.AddVerticalSpeed();
            player.anim.SetFloat("AirSpeed", player.verticalSpeed);
        }
    }
}
