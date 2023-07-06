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

        if (playerData.canLedgeClimb && isTouchingWall & !isTouchingLedge) {
            player.LedgeClimbState.SetDetectedPosition(player.transform.position);
            stateMachine.ChangeState(player.LedgeClimbState);
        }
        else if (xInput == -player.FacingDirection && !isWallJumping) {
            WallHop(playerData.wallHopSpeed, playerData.wallHopDirectionOffAngle, player.FacingDirection);
        }
        else if (isOnSolidGround || (isOnPlatform && yInput != -1)) {
            stateMachine.ChangeState(player.LandState);
        }
        else if (!playerData.autoWallGrab) {
            if (playerData.canWallClimb && grabInput)
                stateMachine.ChangeState(player.WallGrabState);
        }
        else if (playerData.autoWallGrab) {
            if (playerData.canWallClimb && yInput == 1)
                stateMachine.ChangeState(player.WallClimbState);
            else if (playerData.canWallClimb && xInput == player.FacingDirection && yInput == 0)
                stateMachine.ChangeState(player.WallGrabState);
        }
        else if (!isTouchingWall) {
            stateMachine.ChangeState(player.AirborneState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (yInput == -1)
            player.SetVelocityY(-playerData.fastWallSlideSpeed, playerData.wallSlideAcceleration, playerData.lerpWallVelocity);
        else
            player.SetVelocityY(-playerData.wallSlideSpeed, playerData.wallSlideAcceleration, playerData.lerpWallVelocity);
            
        player.SetVelocityX(0f);
    }
}
