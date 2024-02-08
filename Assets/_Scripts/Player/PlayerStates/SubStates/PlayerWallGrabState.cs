using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerWallGrabState : PlayerTouchingWallState {
    protected float elapsedGrabTime;
    protected bool stopGrabbing;

    public PlayerWallGrabState(Player player, StateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
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

        elapsedGrabTime = 0f;
        stopGrabbing = false;

        player.SetVelocityX(0f);
    }

    public override void Exit() {
        base.Exit();

        isWallGrabing = false;

        elapsedGrabTime = 0f;
        stopGrabbing = false;
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        if (xInput == player.FacingDirection) {
            elapsedGrabTime = 0f;
            stopGrabbing = false;
        }
        else if (xInput == 0) {
            elapsedGrabTime += Time.deltaTime;

            if (elapsedGrabTime >= playerData.grabDelay && !stopGrabbing) {
                stopGrabbing = true;
            }
        }

        if (playerData.CanLedgeGrab.Value && isTouchingWall & !isTouchingLedge) {
            player.LedgeClimbState.SetDetectedPosition(player.transform.position);
            stateMachine.ChangeState(player.LedgeClimbState);
        }
        else if (playerData.CanWallJump.Value && jumpInput && (isTouchingWall || isTouchingBackWall || wallJumpCoyoteTime || hasTouchedWall) && !isOnSolidGround) {
            if (yInput >= 0) {
                player.WallJumpState.WallJumpConfiguration(playerData.wallJumpSpeed, playerData.wallJumpDirectionOffAngle, player.FacingDirection, playerData.wallJumpTime);
            }
            else {
                player.WallJumpState.WallJumpConfiguration(playerData.wallJumpSpeed, -player.FacingDirection * Vector2.right, player.FacingDirection, playerData.wallJumpTime);
            }

            stateMachine.ChangeState(player.WallJumpState);
        }
        // else if (player.CheckGround(playerData.platformLayer) && xInput == 0) {
        //     stateMachine.ChangeState(player.LandState);
        // }
        else if (playerData.autoWallGrab) {
            if (playerData.CanWallClimb.Value && yInput != 0) {
                stateMachine.ChangeState(player.WallClimbState);
            }
            else if ((!playerData.hasGrabDelay && xInput == 0) || (playerData.hasGrabDelay && stopGrabbing)) {
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
                if ((!playerData.hasGrabDelay && xInput == 0) || (playerData.hasGrabDelay && stopGrabbing)) {
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
