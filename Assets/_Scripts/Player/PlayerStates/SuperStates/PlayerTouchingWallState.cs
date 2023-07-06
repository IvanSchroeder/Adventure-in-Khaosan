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
        
        if (playerData.canWallJump && jumpInput && (isTouchingWall || isTouchingBackWall || wallJumpCoyoteTime || hasTouchedWall) && yInput != -1) {
            stateMachine.ChangeState(player.WallJumpState);
        }
        else if (!isTouchingWall) {
            stateMachine.ChangeState(player.AirborneState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();
        
        wallHit = Physics2D.Raycast((Vector2)player.GroundPoint.position + (Vector2.up * playerData.wallCheckOffset.y), Vector2.right * player.FacingDirection, playerData.wallCheckDistance * 1.5f, playerData.wallLayer);
        player.wallHitPos = wallHit.point;
    }

    public void WallHop(float velocity, Vector2 angle, int direction) {
        player.SetVelocity(velocity, angle, direction);
        player.CheckFacingDirection(xInput);
        stateMachine.ChangeState(player.AirborneState);
    }
}
