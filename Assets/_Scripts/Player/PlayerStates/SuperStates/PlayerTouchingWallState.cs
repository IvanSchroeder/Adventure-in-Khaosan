using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerTouchingWallState : PlayerState {
    protected float wallHitX;
    protected RaycastHit2D wallHit;

    public PlayerTouchingWallState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
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

        player.Rb.gravityScale = 0f;
    }

    public override void Exit() {
        base.Exit();

        if (isExitingState) return;

        player.Rb.gravityScale = playerData.defaultGravityScale;

        isTouchingWall = player.CheckWall();
        isTouchingBackWall = player.CheckBackWall();
        if (!isTouchingWall) hasTouchedWall = true;
        if (!isTouchingBackWall) hasTouchedWallBack = true;

        if (!wallJumpCoyoteTime && (!isTouchingWall || hasTouchedWallBack)) {
            player.AirborneState.StartWallJumpCoyoteTime();
        }
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        // if (!isGrounded && isTouchingWall && ((!playerData.autoWallGrab && grabInput) || playerData.autoWallGrab) && !jumpInput) {
        //     return;
        // }

        if (isExitingState) return;
        
        if (playerData.CanWallJump.Value && jumpInput && (isTouchingWall || isTouchingBackWall || wallJumpCoyoteTime || hasTouchedWall)) {
            // if (yInput == -1) {
            //     player.WallJumpState.SetNextWallJumpDirection(playerData.wallJumpDirectionOffAngle * new Vector2(1f, -1f));
            //     stateMachine.ChangeState(player.WallJumpState);
            // }
            // else if (xInput == 0 && yInput == 0) {
            //     player.WallJumpState.SetNextWallJumpDirection(playerData.wallJumpDirectionOffAngle * Vector2.right);
            //     stateMachine.ChangeState(player.WallJumpState);
            // }
            // else {
            //     player.WallJumpState.SetNextWallJumpDirection(playerData.wallJumpDirectionOffAngle);
            //     stateMachine.ChangeState(player.WallJumpState);
            // }

            if ((isWallSliding && yInput == 0) || (isWallGrabing) || (isWallClimbing && yInput != 1)) {
                player.WallJumpState.WallJumpConfiguration(playerData.wallJumpSpeed, playerData.wallJumpDirectionOffAngle * Vector2.right, player.FacingDirection, playerData.wallJumpTime);
            }
            else {
                player.WallJumpState.WallJumpConfiguration(playerData.wallJumpSpeed, playerData.wallJumpDirectionOffAngle, player.FacingDirection, playerData.wallJumpTime);
            }

            stateMachine.ChangeState(player.WallJumpState);
        }
        else if (xInput == -player.FacingDirection && !isWallJumping) {
            player.WallJumpState.WallJumpConfiguration(playerData.wallHopSpeed, playerData.wallHopDirectionOffAngle, player.FacingDirection, playerData.wallHopTime);
            stateMachine.ChangeState(player.WallJumpState);
        }
        else if (!isTouchingWall) {
            stateMachine.ChangeState(player.AirborneState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        player.SetVelocityX(0f);
        
        wallHit = player.GetWallHit(player.FacingDirection);
        if (wallHit) player.wallHitPos = wallHit.point;
    }
}
