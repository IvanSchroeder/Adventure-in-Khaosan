using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerWallJumpState : PlayerAbilityState {
    protected int wallJumpDirection;
    protected Vector2 nextWallJumpDirectionVec;

    public PlayerWallJumpState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        isWallJumping = true;
        player.Rb.gravityScale = 0f;

        player.InputHandler.UseJumpInput();
        wallJumpDirection = -player.FacingDirection;
        // player.SetVelocity(playerData.wallJumpSpeed, playerData.wallJumpDirectionOffAngle, wallJumpDirection);
        player.SetVelocity(playerData.wallJumpSpeed, nextWallJumpDirectionVec, wallJumpDirection);
        player.CheckFacingDirection(wallJumpDirection);
        player.JumpState.ResetAmountOfJumpsLeft();
        player.JumpState.DecreaseAmountOfJumpsLeft();
        player.AirborneState.StopWallJumpCoyoteTime();

        // default angle
        nextWallJumpDirectionVec = playerData.wallJumpDirectionOffAngle;
    }

    public override void Exit() {
        base.Exit();

        isWallJumping = false;
        player.Rb.gravityScale = playerData.defaultGravityScale;
        
        // player.CheckFacingDirection(xInput);

        // if (xInput == player.FacingDirection) {
        //     player.SetVelocityX(xInput * playerData.runSpeed);
        // }
        // else if (xInput == -player.FacingDirection) {
        //     player.SetVelocityX(xInput * playerData.runSpeed, playerData.airAcceleration, playerData.lerpVelocityInAir);
        // }
        // else if (xInput == 0) {
        //     player.SetVelocityX(0f, playerData.airDecceleration, playerData.lerpVelocityInAir);
        // }
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        CheckWallJumpMultiplier();

        if (Time.time >= startTime + playerData.wallJumpTime)
            isAbilityDone = true;
    }

    // public override void PhysicsUpdate() {
    //     base.PhysicsUpdate();
    // }

    private void CheckWallJumpMultiplier() {
        if (!isWallJumping) return;

        if (jumpInputStop) {
            player.InputHandler.UseJumpStopInput();
            player.SetVelocityY(player.CurrentVelocity.y * playerData.variableJumpHeightMultiplier);
            isWallJumping = false;
            isAbilityDone = true;
        }
        else if (isFalling) {
            player.InputHandler.UseJumpStopInput();
            isAbilityDone = true;
            isWallJumping = false;
        }
    }

    public void SetNextWallJumpDirection(Vector2 vector) {
        nextWallJumpDirectionVec = vector;
    }

    public void GetWallJumpDirection(bool isTouchingWall) {
        if (isTouchingWall)
            wallJumpDirection = -player.FacingDirection;
        else
            wallJumpDirection = player.FacingDirection;
    }
}
