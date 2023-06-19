using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallGrabState : PlayerTouchingWallState {
    public PlayerWallGrabState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void AnimationFinishTrigger()
    {
        base.AnimationFinishTrigger();
    }

    public override void AnimationTrigger()
    {
        base.AnimationTrigger();
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }

    public override void Enter() {
        base.Enter();

        isWallGrabing = true;
        player.SetVelocityX(0f);
    }

    public override void Exit() {
        base.Exit();

        isWallGrabing = false;
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        if (isTouchingWall & !isTouchingLedge) {
            player.LedgeClimbState.SetDetectedPosition(player.transform.position);
            stateMachine.ChangeState(player.LedgeClimbState);
        }
        else if (xInput == -player.FacingDirection) {
            WallHop(playerData.wallJumpSpeed, playerData.wallHopDirectionOffAngle, player.FacingDirection);
        }
        else if (playerData.autoWallGrab) {
            if (xInput == player.FacingDirection && yInput != 0) {
                stateMachine.ChangeState(player.WallClimbState);
            }
            else if (xInput == 0) {
                if (player.CheckGround(playerData.platformLayer))
                    stateMachine.ChangeState(player.LandState);
                else
                    stateMachine.ChangeState(player.WallSlideState);
            }
        }
        else if (!playerData.autoWallGrab) {
            if (grabInput) {
                if (yInput != 0) {
                    stateMachine.ChangeState(player.WallClimbState);
                }
            }
            else if (!grabInput) {
                if (xInput == 0 && playerData.autoWallSlide) {
                    if (player.CheckGround(playerData.platformLayer))
                        stateMachine.ChangeState(player.LandState);
                    else
                        stateMachine.ChangeState(player.WallSlideState);
                }
            }
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        player.SetVelocityY(0f, playerData.wallGrabAcceleration, playerData.lerpWallVelocity);
        player.SetVelocityX(0f);
    }
}
