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

        if (elapsedTimeSinceStandup < playerData.standupDelay) {
            elapsedTimeSinceStandup += Time.deltaTime;

            if (elapsedTimeSinceStandup >= playerData.standupDelay) {
                player.SetColliderParameters(player.HitboxTrigger, playerData.standingColliderConfig);
            }
        }

        player.CheckFacingDirection(xInput);

        if (isExitingState) return;

        // if (playerData.canJump && jumpInput && player.JumpState.CanJump() && !isTouchingCeiling && !isIgnoringPlatforms) {
        //     // bullet jump maybe? for a longer horizontal jump
        //     coyoteTime = false;
        //     stateMachine.ChangeState(player.JumpState);
        // }
        if (playerData.CanCrouch.Value && yInput == -1)
            if (xInput == 0)
                stateMachine.ChangeState(player.CrouchIdleState);
            else
                stateMachine.ChangeState(player.CrouchMoveState);
        else if (playerData.CanMove.Value && xInput != 0) {
            if (playerData.CanWallClimb.Value && isTouchingWall && isTouchingLedge) {
                if (((playerData.autoWallGrab && xInput == player.FacingDirection) || (!playerData.autoWallGrab && grabInput)) && yInput == 0) 
                    stateMachine.ChangeState(player.WallGrabState);
                else if ((playerData.autoWallGrab || (!playerData.autoWallGrab && grabInput)) && (/*(isOnPlatform && yInput != 0) ||*/ (isGrounded && yInput == 1)))
                    stateMachine.ChangeState(player.WallClimbState);
            }
            else
                stateMachine.ChangeState(player.MoveState);
        }
        else if (xInput == 0) {
            if (playerData.CanWallClimb.Value && isTouchingWall && isTouchingLedge) {
                if (((playerData.autoWallGrab && xInput == player.FacingDirection) || (!playerData.autoWallGrab && grabInput)) && yInput == 0) 
                    stateMachine.ChangeState(player.WallGrabState);
                else if ((playerData.autoWallGrab || (!playerData.autoWallGrab && grabInput)) && (/*(isOnPlatform && yInput != 0) ||*/ (isGrounded && yInput == 1)))
                    stateMachine.ChangeState(player.WallClimbState);
            }
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (isTouchingWall || isTouchingLedge) {
            player.SetVelocityX(0f);
        }

        if (playerData.enableFriction) {
            player.SetVelocityX(0f, playerData.runDecceleration * playerData.frictionAmount, playerData.lerpVelocity);
        }
        else {
            player.SetVelocityX(0f, playerData.runDecceleration, playerData.lerpVelocity);
        }
        player.SetVelocityY(player.CurrentVelocity.y);
    }
}
