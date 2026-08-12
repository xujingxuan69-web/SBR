using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class EntityState<T> where T : Entity
{
    protected T player;
    protected EntityStateMachine<T> stateMachine;
    private string animBoolName;
    private AnimatorControllerParameterType animType;

    protected float stateTimer;
    protected bool triggerCalled;

    protected float verticalInput;
    protected float horizontalInput;

    public EntityState(T _player, EntityStateMachine<T> _stateMachine, string _animBoolName)
    {
        this.player = _player;
        this.stateMachine = _stateMachine;
        this.animBoolName = _animBoolName;
    }

    public virtual bool IsAttackState() => false;

    public virtual void Enter()
    {
        foreach (var _animParam in player.anim.parameters)
        {
            if (_animParam.name == animBoolName)
            {
                animType = _animParam.type;
                break;
            }
        }

        switch (animType)
        {
            case AnimatorControllerParameterType.Bool:
                player.anim.SetBool(animBoolName, true);
                break;
            case AnimatorControllerParameterType.Trigger:
                player.anim.SetTrigger(animBoolName);
                break;
        }

        triggerCalled = false;

        GetMoveInput();
    }

    public virtual void FixedUpdate()
    {
        stateTimer -= Time.deltaTime;
        GetMoveInput();
    }

    private void GetMoveInput()
    {
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");
    }

    public virtual void Exit()
    {
        switch (animType)
        {
            case AnimatorControllerParameterType.Bool:
                player.anim.SetBool(animBoolName, false);
                break;
            case AnimatorControllerParameterType.Trigger:
                player.anim.ResetTrigger(animBoolName);
                break;
        }
    }

    public virtual void AnimationFinishTrigger()
    {
        triggerCalled = true;
    }
}
