using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine {
    public State CurrentState { get; protected set; }
    public State PreviousState { get; protected set; }

    public Action<State, State> OnStateChange;

    public virtual void Initialize(State startingState) {
        CurrentState = startingState;
        PreviousState = null;
        CurrentState.Enter();
        OnStateChange?.Invoke(CurrentState, PreviousState);
    }

    public virtual void ChangeState(State newState) {
        if (CurrentState == newState) return;
        PreviousState = CurrentState;
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
        OnStateChange?.Invoke(CurrentState, PreviousState);
    }
}
