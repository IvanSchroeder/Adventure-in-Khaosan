using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerTouchingWallState : PlayerState {
    protected float wallHitX;
    protected RaycastHit2D wallHit;

    public PlayerTouchingWallState(Player player, StateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
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

        wallHit = player.GetWallHit(player.FacingDirection);
        if (wallHit) {
            player.wallHitPos = wallHit.point;
            player.transform.SetParent(wallHit.transform);
        }

        player.Rb.gravityScale = 0f;
        if (player.CurrentVelocity.y.Sign() == -1) player.SetVelocityY(player.CurrentVelocity.y * playerData.wallTouchVelocityCutoff);
    }

    public override void Exit() {
        base.Exit();

        if (player.transform.parent.IsNotNull()) player.transform.SetParent(null);

        player.Rb.gravityScale = playerData.defaultGravityScale;

        player.isTouchingWall = player.CheckWall();
        player.isTouchingBackWall = player.CheckBackWall();
        if (!player.isTouchingWall) player.hasTouchedWall = true;
        if (!player.isTouchingBackWall) player.hasTouchedWallBack = true;

        if (!wallJumpCoyoteTime && (!player.isTouchingWall || player.hasTouchedWallBack)) {
            player.AirborneState.StartWallJumpCoyoteTime();
        }
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        // if (!isGrounded && isTouchingWall && ((!playerData.autoWallGrab && grabInput) || playerData.autoWallGrab) && !jumpInput) {
        //     return;
        // }

        if (isExitingState) return;
        
        if (xInput == -player.FacingDirection && !isWallJumping) {
            if (player.isOnSolidGround) {
                stateMachine.ChangeState(player.IdleState);
            }
            else {
                player.WallJumpState.WallJumpConfiguration(playerData.wallHopSpeed, playerData.wallHopDirectionOffAngle, player.FacingDirection, playerData.wallHopTime, true);
                stateMachine.ChangeState(player.WallJumpState);
            }
        }
        else if (!player.isTouchingWall) {
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
