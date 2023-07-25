using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerWallSlideState : PlayerTouchingWallState {
    public PlayerWallSlideState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        isWallSliding = true;
        player.SetVelocityX(0f);
    }

    public override void Exit() {
        base.Exit();

        isWallSliding = false;
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        if (playerData.CanLedgeGrab.Value && isTouchingWall & !isTouchingLedge) {
            player.LedgeClimbState.SetDetectedPosition(player.transform.position);
            stateMachine.ChangeState(player.LedgeClimbState);
        }
        else if (playerData.CanWallJump.Value && jumpInput && (isTouchingWall || isTouchingBackWall || wallJumpCoyoteTime || hasTouchedWall) && !isOnSolidGround) {
            if (yInput == -1) {
                player.WallJumpState.WallJumpConfiguration(playerData.wallJumpSpeed, Vector2.right, player.FacingDirection, playerData.wallJumpTime);
            }
            else {
                player.WallJumpState.WallJumpConfiguration(playerData.wallJumpSpeed, playerData.wallJumpDirectionOffAngle, player.FacingDirection, playerData.wallJumpTime);
            }

            stateMachine.ChangeState(player.WallJumpState);
        }
        else if (isOnSolidGround || (isOnPlatform && yInput != -1)) {
            stateMachine.ChangeState(player.IdleState);
        }
        else if (!playerData.autoWallGrab) {
            if (playerData.CanWallClimb.Value && grabInput)
                stateMachine.ChangeState(player.WallGrabState);
        }
        else if (playerData.autoWallGrab) {
            if (playerData.CanWallClimb.Value && yInput == 1)
                stateMachine.ChangeState(player.WallClimbState);
            else if (playerData.CanWallClimb.Value && xInput == player.FacingDirection && yInput == 0)
                stateMachine.ChangeState(player.WallGrabState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (yInput == -1)
            player.SetVelocityY(-playerData.fastWallSlideSpeed, playerData.wallSlideAcceleration, playerData.lerpWallVelocity);
        else
            player.SetVelocityY(-playerData.wallSlideSpeed, playerData.wallSlideAcceleration, playerData.lerpWallVelocity);
    }
}
