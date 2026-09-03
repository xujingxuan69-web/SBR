using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorseState : EntityState<Horse>
{
    public HorseState(Horse _player, EntityStateMachine<Horse> _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    protected override void GetMoveInput()
    {
        base.GetMoveInput();
        verticalInput = player.InputReader.MoveInput.y;
        horizontalInput = player.InputReader.MoveInput.x;
    }

}
