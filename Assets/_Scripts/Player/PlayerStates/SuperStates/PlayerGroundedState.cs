using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerState {
    protected RaycastHit2D groundHit;

    public PlayerGroundedState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void DoChecks() {
        base.DoChecks();
    }

    public override void Enter() {
        base.Enter();

        player.JumpState.ResetAmountOfJumpsLeft();
    }

    public override void Exit() {
        base.Exit();
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (jumpInput && player.JumpState.CanJump() && !isTouchingCeiling && !isIgnoringPlatforms && yInput != -1) {
            coyoteTime = false;
            stateMachine.ChangeState(player.JumpState);
        }
        else if (!isGrounded) {
            player.AirborneState.StartCoyoteTime();
            stateMachine.ChangeState(player.AirborneState);
        }
        else if (isTouchingWall && ((playerData.autoWallGrab && xInput == player.FacingDirection) || (!playerData.autoWallGrab && grabInput)) && isTouchingLedge) {
            stateMachine.ChangeState(player.WallGrabState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (playerData.stickToGround) {
            // groundHit = player.GetGroundHit();
            groundHit = Physics2D.Raycast((Vector2)player.GroundCheck.position + (Vector2.up * playerData.groundCheckOffset.y), Vector2.down, playerData.groundCheckDistance * 1.5f, playerData.groundLayer);
            player.groundHitPos = groundHit.point;

            if (groundHit) {
                Vector2 temp = Vector2.zero;
                temp.Set(player.transform.position.x, groundHit.point.y + 0.05f);
                player.transform.position = temp;
            }
        }
    }
}
