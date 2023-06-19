using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        if (isTouchingWall & !isTouchingLedge && yInput == 1) {
            player.LedgeClimbState.SetDetectedPosition(player.transform.position);
            stateMachine.ChangeState(player.LedgeClimbState);
        }
        else if (xInput == -player.FacingDirection) {
            WallHop(playerData.wallJumpSpeed, playerData.wallHopDirectionOffAngle, player.FacingDirection);
        }
        else if (!playerData.autoWallGrab) {
            if (grabInput)
                if (yInput != 0)
                    return;
                else 
                    stateMachine.ChangeState(player.WallGrabState);
            else {
                if (playerData.autoWallSlide && isTouchingWall && xInput != -player.FacingDirection)
                    stateMachine.ChangeState(player.WallSlideState);
                else if (!playerData.autoWallSlide && isTouchingWall && xInput == player.FacingDirection)
                    stateMachine.ChangeState(player.WallSlideState);
                else if (!playerData.autoWallSlide && xInput != player.FacingDirection)
                    stateMachine.ChangeState(player.AirborneState);
            }
        }
        else if (playerData.autoWallGrab) {
            if (xInput == player.FacingDirection && yInput == 0)
                stateMachine.ChangeState(player.WallGrabState);
            else if (xInput != player.FacingDirection && yInput == 0)
                stateMachine.ChangeState(player.WallSlideState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        player.SetVelocityY(yInput * playerData.wallClimbSpeed, playerData.wallClimbAcceleration, playerData.lerpWallVelocity);
        player.SetVelocityX(0f);
    }
}
