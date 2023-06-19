using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpState : PlayerAbilityState {
    private int wallJumpDirection;

    public PlayerWallJumpState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        player.InputHandler.UseJumpInput();
        player.JumpState.ResetAmountOfJumpsLeft();
        player.SetVelocity(playerData.wallJumpSpeed, playerData.wallJumpDirectionOffAngle, wallJumpDirection);
        player.CheckFacingDirection(wallJumpDirection);
        player.JumpState.DecreaseAmountOfJumpsLeft();
    }

    public override void Exit() {
        base.Exit();
        
        player.CheckFacingDirection(xInput);

        if (xInput == player.FacingDirection) {
            player.SetVelocityX(xInput * playerData.runSpeed);
        }
        else if (xInput == -player.FacingDirection) {
            player.SetVelocityX(xInput * playerData.runSpeed, playerData.airAcceleration, playerData.lerpVelocityInAir);
        }
        else if (xInput == 0) {
            player.SetVelocityX(0f, playerData.airDecceleration, playerData.lerpVelocityInAir);
        }
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (Time.time >= startTime + playerData.wallJumpTime)
            isAbilityDone = true;
    }

    public void GetWallJumpDirection(bool isTouchingWall) {
        if (isTouchingWall)
            wallJumpDirection = -player.FacingDirection;
        else
            wallJumpDirection = player.FacingDirection;
    }
}
