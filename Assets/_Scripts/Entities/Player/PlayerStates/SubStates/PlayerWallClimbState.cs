using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerWallClimbState : PlayerTouchingWallState {
    public PlayerWallClimbState(Player player, StateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
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

        if (playerData.CanLedgeGrab.Value && player.isTouchingWall & !player.isTouchingLedge) {
            player.LedgeClimbState.SetDetectedPosition(player.transform.position);
            stateMachine.ChangeState(player.LedgeClimbState);
        }
        else if (playerData.CanWallJump.Value && jumpInput && (player.isTouchingWall || player.isTouchingBackWall || wallJumpCoyoteTime || player.hasTouchedWall) && !player.isOnSolidGround) {
            if (yInput == -1) {
                player.WallJumpState.WallJumpConfiguration(playerData.wallJumpSpeed, Vector2.right, player.FacingDirection, playerData.wallJumpTime);
            }
            else {
                player.WallJumpState.WallJumpConfiguration(playerData.wallJumpSpeed, playerData.wallJumpDirectionOffAngle, player.FacingDirection, playerData.wallJumpTime);
            }

            stateMachine.ChangeState(player.WallJumpState);
        }
        else if (player.isOnSolidGround && yInput == -1) {
            stateMachine.ChangeState(player.LandState);
        }
        else if (playerData.CanWallSlide.Value && !playerData.autoWallGrab) {
            if (grabInput)
                if (yInput != 0)
                    return;
                else if (player.CurrentVelocity.y <= 0f)
                    stateMachine.ChangeState(player.WallGrabState);
            else {
                if (playerData.autoWallSlide && player.isTouchingWall && xInput != -player.FacingDirection && player.CurrentVelocity.y <= 0f)
                    stateMachine.ChangeState(player.WallSlideState);
                else if (!playerData.autoWallSlide && player.isTouchingWall && xInput == player.FacingDirection && player.CurrentVelocity.y <= 0f)
                    stateMachine.ChangeState(player.WallSlideState);
                else if (!playerData.autoWallSlide && xInput != player.FacingDirection && player.CurrentVelocity.y <= 0f)
                    stateMachine.ChangeState(player.AirborneState);
            }
        }
        else if (playerData.CanWallSlide.Value && playerData.autoWallGrab && yInput == 0f) {
            if ((xInput == player.FacingDirection || yInput == 0) && player.CurrentVelocity.y.AbsoluteValue() == 0f)
                stateMachine.ChangeState(player.WallGrabState);
            else if (xInput == 0 && player.CurrentVelocity.y.AbsoluteValue() == 0f)
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
