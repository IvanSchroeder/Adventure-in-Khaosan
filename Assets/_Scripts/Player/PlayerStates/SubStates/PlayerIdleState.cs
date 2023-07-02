using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerIdleState : PlayerGroundedState {
    public PlayerIdleState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void DoChecks() {
        base.DoChecks();
    }

    public override void Enter() {
        base.Enter();

        player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig);

        // isIdle = true;
    }

    public override void Exit() {
        base.Exit();

        // isIdle = false;
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        player.CheckFacingDirection(xInput);

        if (isExitingState) return;

        if (playerData.canCrouch && yInput == -1)
            stateMachine.ChangeState(player.CrouchIdleState);
        else if (xInput != 0) {
            if (playerData.canMove && (!isTouchingWall || !isTouchingLedge))
                stateMachine.ChangeState(player.MoveState);
            else if (playerData.canWallClimb && isTouchingWall && isTouchingLedge && playerData.autoWallGrab && xInput == player.FacingDirection) {
                if (playerData.autoWallGrab && xInput == player.FacingDirection && yInput == 0) 
                    stateMachine.ChangeState(player.WallGrabState);
                else if (playerData.autoWallGrab && ((isOnPlatform && yInput != 0) || (isOnSolidGround && yInput == 1)))
                    stateMachine.ChangeState(player.WallClimbState);
            }
        }
        else if (xInput == 0) {
            if (playerData.canWallClimb && isTouchingWall && isTouchingLedge && playerData.autoWallGrab && xInput == player.FacingDirection) {
                if (playerData.autoWallGrab && xInput == player.FacingDirection && yInput == 0) 
                    stateMachine.ChangeState(player.WallGrabState);
                else if (playerData.autoWallGrab && ((isOnPlatform && yInput != 0) || (isOnSolidGround && yInput == 1)))
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
