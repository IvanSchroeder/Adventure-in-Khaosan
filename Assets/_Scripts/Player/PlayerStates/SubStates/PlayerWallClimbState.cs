using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerWallClimbState : PlayerTouchingWallState {
    public PlayerWallClimbState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        isWallClimbing = true;
        player.SetVelocityX(0f);
    }

    public override void Exit() {
        base.Exit();

        isWallClimbing = false;
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        if (playerData.CanLedgeGrab.Value && isTouchingWall & !isTouchingLedge) {
            player.LedgeClimbState.SetDetectedPosition(player.transform.position);
            stateMachine.ChangeState(player.LedgeClimbState);
        }
        // else if (xInput == -player.FacingDirection && !isWallJumping) {
        //     player.WallJumpState.WallJumpConfiguration(playerData.wallHopSpeed, playerData.wallHopDirectionOffAngle, player.FacingDirection, playerData.wallHopTime);
        //     stateMachine.ChangeState(player.WallJumpState);
        // }
        else if (playerData.CanWallSlide.Value && !playerData.autoWallGrab) {
            if (grabInput)
                if (yInput != 0)
                    return;
                else if (player.CurrentVelocity.y <= 0f)
                    stateMachine.ChangeState(player.WallGrabState);
            else {
                if (playerData.autoWallSlide && isTouchingWall && xInput != -player.FacingDirection && player.CurrentVelocity.y <= 0f)
                    stateMachine.ChangeState(player.WallSlideState);
                else if (!playerData.autoWallSlide && isTouchingWall && xInput == player.FacingDirection && player.CurrentVelocity.y <= 0f)
                    stateMachine.ChangeState(player.WallSlideState);
                else if (!playerData.autoWallSlide && xInput != player.FacingDirection && player.CurrentVelocity.y <= 0f)
                    stateMachine.ChangeState(player.AirborneState);
            }
        }
        else if (playerData.CanWallSlide.Value && playerData.autoWallGrab) {
            if (xInput == player.FacingDirection && yInput == 0 && player.CurrentVelocity.y <= 0f)
                stateMachine.ChangeState(player.WallGrabState);
            else if (xInput != player.FacingDirection && yInput == 0 && player.CurrentVelocity.y <= 0f)
                stateMachine.ChangeState(player.WallSlideState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (yInput != 0) {
            player.SetVelocityY(yInput * playerData.wallClimbSpeed, playerData.wallClimbAcceleration, playerData.lerpWallVelocity);
        }
        else {
            player.SetVelocityY(0f, playerData.wallClimbAcceleration, playerData.lerpWallVelocity);
        }
    }
}
