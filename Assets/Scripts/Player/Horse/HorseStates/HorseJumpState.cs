using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorseJumpState : HorseUnGroundedState
{
    public HorseJumpState(Horse _player, EntityStateMachine<Horse> _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.SetJumpToVerticalSpeed();
        player.kcc.ForceUnground(); //跳跃需要暂时去除KCC锁地，否则马匹会卡在地面上
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }
}
