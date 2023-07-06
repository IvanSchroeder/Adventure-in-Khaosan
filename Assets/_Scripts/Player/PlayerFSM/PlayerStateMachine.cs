using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine {
    public PlayerState CurrentState { get; private set; }
    public PlayerState PreviousState { get; private set; }

    public Action<PlayerState, PlayerState> OnStateChange;

    public void Initialize(PlayerState startingState) {
        CurrentState = startingState;
        PreviousState = null;
        CurrentState.Enter();
    }

    public void ChangeState(PlayerState newState) {
        if (CurrentState == newState) return;
        PreviousState = CurrentState;
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
        OnStateChange?.Invoke(CurrentState, PreviousState);
    }

}
