using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTouchingWallState : PlayerState {
    protected float wallHitX;

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

    public override void DoChecks()
    {
        base.DoChecks();
    }

    public override void Enter() {
        base.Enter();

        player.Rb.gravityScale = 0f;
    }

    public override void Exit() {
        base.Exit();

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

        if (!isGrounded && isTouchingWall && ((!playerData.autoWallGrab && grabInput) || playerData.autoWallGrab) && !jumpInput) {
            return;
        }

        player.CheckFacingDirection(xInput);

        if (isExitingState) return;
        
        if (jumpInput && yInput != -1 && !isOnPlatform) {
            player.WallJumpState.GetWallJumpDirection(isTouchingWall);
            stateMachine.ChangeState(player.WallJumpState);
        }
        else if (!isTouchingWall) {
            stateMachine.ChangeState(player.AirborneState);
        }
        // else if (!isWallGrabing && !isWallClimbing && isOnSolidGround) {
        //     if (((!playerData.autoWallGrab && grabInput) || (playerData.autoWallGrab && xInput == player.FacingDirection) && yInput == 0))
        //         stateMachine.ChangeState(player.WallGrabState);
        //     else if (((!playerData.autoWallGrab && grabInput) || (playerData.autoWallGrab && xInput == player.FacingDirection) && yInput != 0))
        //         stateMachine.ChangeState(player.WallClimbState);
        //     else if ((!playerData.autoWallGrab && (!grabInput || playerData.autoWallSlide)) || (playerData.autoWallGrab && xInput == -player.FacingDirection))
        //         stateMachine.ChangeState(player.LandState);
        // }
        // else if (!playerData.autoWallGrab && !playerData.autoWallSlide && xInput == -player.FacingDirection) {
        //     player.SetForce(playerData.wallJumpSpeed, playerData.wallHopDirectionOffAngle, player.FacingDirection);
        //     stateMachine.ChangeState(player.AirborneState);
        // }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();
    }

    public void WallHop(float velocity, Vector2 angle, int direction) {
        player.SetVelocity(velocity, angle, direction);
        player.CheckFacingDirection(xInput);
        stateMachine.ChangeState(player.AirborneState);
    }
}
