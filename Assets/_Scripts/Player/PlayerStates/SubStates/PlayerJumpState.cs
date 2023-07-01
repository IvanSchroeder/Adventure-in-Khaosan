using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerJumpState : PlayerAbilityState {
    public PlayerJumpState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
        amountOfJumpsLeft = playerData.amountOfJumps;
    }

    public override void Enter() {
        base.Enter();

        player.InputHandler.UseJumpInput();
        player.SetVelocityY(playerData.jumpHeight);
        // player.SetForce(playerData.jumpHeight, Vector2.up, xInput);
        // isJumping = true;
        player.AirborneState.SetIsJumping();
        if (amountOfJumpsLeft > 0) DecreaseAmountOfJumpsLeft();
        else if (amountOfJumpsLeft < 0) amountOfJumpsLeft = 0;
        isAbilityDone = true;
    }

    public override void Exit() {
        base.Exit();

        isJumping = false;
    }

    public bool CanJump() {
        if (amountOfJumpsLeft > 0) return true;
        else return false;
    }

    public void ResetAmountOfJumpsLeft() {
        amountOfJumpsLeft = playerData.amountOfJumps;
    }

    public void DecreaseAmountOfJumpsLeft() {
        amountOfJumpsLeft--;
    }
}
