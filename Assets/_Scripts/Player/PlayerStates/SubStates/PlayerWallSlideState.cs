using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        if (isTouchingWall & !isTouchingLedge) {
            player.LedgeClimbState.SetDetectedPosition(player.transform.position);
            stateMachine.ChangeState(player.LedgeClimbState);
        }
        else if (xInput == -player.FacingDirection) {
            WallHop(playerData.wallHopSpeed, playerData.wallHopDirectionOffAngle, player.FacingDirection);
            return;
        }
        else if (!playerData.autoWallGrab) {
            if (grabInput)
                stateMachine.ChangeState(player.WallGrabState);
            else if (isOnSolidGround || (isOnPlatform && yInput != -1))
                stateMachine.ChangeState(player.LandState);
        }
        else if (playerData.autoWallGrab) {
            if (xInput == player.FacingDirection || yInput > 0)
                stateMachine.ChangeState(player.WallClimbState);
            else if (xInput == player.FacingDirection && yInput == 0)
                stateMachine.ChangeState(player.WallGrabState);
            else if (isOnSolidGround || (isOnPlatform && yInput != -1))
                stateMachine.ChangeState(player.LandState);
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
