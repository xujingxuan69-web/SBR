using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStateMachine<T> where T : Entity
{
    public EntityState<T> currentState {get; private set; }


    public void Initialize(EntityState<T> _startState)
    {
        currentState = _startState;
        currentState.Enter();
    }

    public void ChangeState(EntityState<T> _newState)
    {
        currentState.Exit();
        currentState = _newState;
        currentState.Enter();
    }
}
