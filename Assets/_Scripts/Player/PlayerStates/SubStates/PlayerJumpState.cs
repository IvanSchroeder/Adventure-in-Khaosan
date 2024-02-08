using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerJumpState : PlayerAbilityState {
    public PlayerJumpState(Player player, StateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
        amountOfJumpsLeft = playerData.amountOfJumps;
    }

    public override void Enter() {
        base.Enter();

        player.InputHandler.UseJumpInput();
        player.SetVelocityY(playerData.jumpHeight);
        isJumping = true;
        player.AirborneState.StopCoyoteTime();
        // player.AirborneState.SetIsJumping();
        if (amountOfJumpsLeft > 0) DecreaseAmountOfJumpsLeft();
        else if (amountOfJumpsLeft < 0) amountOfJumpsLeft = 0;
        isAbilityDone = true;
    }

    public override void Exit() {
        base.Exit();
    }

    public override void LogicUpdate() {
        base.LogicUpdate();
    }

    public bool CanJump() {
        if (!playerData.CanJump.Value) return false;

        if (amountOfJumpsLeft > 0) return true;
        
        return false;
    }

    public void ResetAmountOfJumpsLeft() {
        amountOfJumpsLeft = playerData.amountOfJumps;
    }

    public void DecreaseAmountOfJumpsLeft() {
        amountOfJumpsLeft--;
    }
}
