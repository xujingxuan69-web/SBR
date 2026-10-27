using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HorseUnstableState : HorseState
{
    float smoothedAnimRate;
    float targetAnimRate;

    public HorseUnstableState(global::Horse _player, EntityStateMachine<global::Horse> _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
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

        if (player.IsStableGrounded)
        {
            stateMachine.ChangeState(player.groundedState);
        }
        else if (!player.IsUnstableGrounded)
        {
            stateMachine.ChangeState(player.airState);
        }

        if (player.InputReader.CheckJumpPressed()) //必须呈包含关系，否则跳跃会被提前消耗掉
        {
            stateMachine.ChangeState(player.jumpState);
            return;
        }

        AnimExecute();
    }

    private void AnimExecute()
    {
        targetAnimRate = 0.1f * Mathf.Abs(horizontalInput);
        smoothedAnimRate = Mathf.Lerp(smoothedAnimRate, targetAnimRate, Time.fixedDeltaTime);
        float animRate = Mathf.Clamp01(Mathf.Abs(player.horizontalSpeed) / player.maxForwardSpeed + smoothedAnimRate);
        player.anim.SetFloat("GroundSpeed", animRate);
    }
}
