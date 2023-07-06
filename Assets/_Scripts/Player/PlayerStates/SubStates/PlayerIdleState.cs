using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public class PlayerIdleState : PlayerGroundedState {
    public PlayerIdleState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig);

        isIdle = true;
    }

    public override void Exit() {
        base.Exit();

        isIdle = false;
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        player.CheckFacingDirection(xInput);

        if (isExitingState) return;

        // if (playerData.canJump && jumpInput && player.JumpState.CanJump() && !isTouchingCeiling && !isIgnoringPlatforms) {
        //     // bullet jump maybe? for a longer horizontal jump
        //     coyoteTime = false;
        //     stateMachine.ChangeState(player.JumpState);
        // }
        if (playerData.canCrouch && yInput == -1 && (!isTouchingLedge || !isTouchingWall))
            if (xInput == 0)
                stateMachine.ChangeState(player.CrouchIdleState);
            else
                stateMachine.ChangeState(player.CrouchMoveState);
        else if (playerData.canMove && xInput != 0) {
            if (playerData.canWallClimb && isTouchingWall && isTouchingLedge) {
                if (((playerData.autoWallGrab && xInput == player.FacingDirection) || (!playerData.autoWallGrab && grabInput)) && yInput == 0) 
                    stateMachine.ChangeState(player.WallGrabState);
                else if ((playerData.autoWallGrab || (!playerData.autoWallGrab && grabInput)) && ((isOnPlatform && yInput != 0) || (isOnSolidGround && yInput == 1)))
                    stateMachine.ChangeState(player.WallClimbState);
            }
            else if ((isTouchingWall && !isTouchingLedge) || (!isTouchingWall && isTouchingLedge))
                return;
            else
                stateMachine.ChangeState(player.MoveState);
        }
        else if (xInput == 0) {
            if (playerData.canWallClimb && isTouchingWall && isTouchingLedge) {
                if (((playerData.autoWallGrab && xInput == player.FacingDirection) || (!playerData.autoWallGrab && grabInput)) && yInput == 0) 
                    stateMachine.ChangeState(player.WallGrabState);
                else if ((playerData.autoWallGrab || (!playerData.autoWallGrab && grabInput)) && ((isOnPlatform && yInput != 0) || (isOnSolidGround && yInput == 1)))
                    stateMachine.ChangeState(player.WallClimbState);
            }
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        player.SetVelocityX(0f, playerData.runDecceleration, playerData.lerpVelocity);
        player.SetVelocityY(player.CurrentVelocity.y);
    }
}
