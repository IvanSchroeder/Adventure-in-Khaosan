using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

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

        if (playerData.CanLedgeGrab.Value && isTouchingWall & !isTouchingLedge) {
            player.LedgeClimbState.SetDetectedPosition(player.transform.position);
            stateMachine.ChangeState(player.LedgeClimbState);
        }
        // else if (xInput == -player.FacingDirection && !isWallJumping) {
        //     player.WallJumpState.WallJumpConfiguration(playerData.wallHopSpeed, playerData.wallHopDirectionOffAngle, player.FacingDirection, playerData.wallHopTime);
        //     stateMachine.ChangeState(player.WallJumpState);
        // }
        else if (player.CheckGround(playerData.platformLayer) && xInput == 0) {
            stateMachine.ChangeState(player.LandState);
        }
        else if (playerData.autoWallGrab) {
            if (playerData.CanWallClimb.Value && xInput == player.FacingDirection && yInput != 0) {
                stateMachine.ChangeState(player.WallClimbState);
            }
            else if (xInput == 0) {
                if (playerData.CanWallSlide.Value && playerData.autoWallSlide)
                    stateMachine.ChangeState(player.WallSlideState);
                else
                    stateMachine.ChangeState(player.AirborneState);
            }
        }
        else if (!playerData.autoWallGrab) {
            if (playerData.CanWallClimb.Value && grabInput) {
                if (yInput != 0) {
                    stateMachine.ChangeState(player.WallClimbState);
                }
            }
            else if (!grabInput) {
                if (xInput == 0) {
                    if (playerData.CanWallSlide.Value && playerData.autoWallSlide)
                        stateMachine.ChangeState(player.WallSlideState);
                    else
                        stateMachine.ChangeState(player.AirborneState);
                }
            }
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        player.SetVelocityY(0f, playerData.wallGrabAcceleration, playerData.lerpWallVelocity);
    }
}
