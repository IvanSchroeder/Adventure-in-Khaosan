using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerGroundedState : PlayerState {
    protected RaycastHit2D groundHit;
    protected float elapsedTimeSinceStandup;
    protected bool standUp;

    public PlayerGroundedState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        player.JumpState.ResetAmountOfJumpsLeft();
        player.InputHandler.UseJumpStopInput();
        player.Anim.SetBool("airborne", false);

        elapsedTimeSinceStandup = 0f;
    }

    public override void Exit() {
        base.Exit();
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        if (playerData.CanJump.Value && jumpInput && player.JumpState.CanJump() && !isTouchingCeiling && !isIgnoringPlatforms) {
            // bullet jump maybe? for a longer horizontal jump
            coyoteTime = false;
            stateMachine.ChangeState(player.JumpState);
        }
        else if (!isGrounded) {
            player.AirborneState.StartCoyoteTime();
            stateMachine.ChangeState(player.AirborneState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        groundHit = Physics2D.Raycast((Vector2)player.GroundPoint.position + (Vector2.up * playerData.groundCheckOffset.y), Vector2.down, playerData.groundCheckDistance * 1.5f, playerData.groundLayer);
        player.groundHitPos = groundHit.point;

        // if (playerData.stickToGround) {
        //     // groundHit = player.GetGroundHit();

        //     if (groundHit) {
        //         Vector2 temp = Vector2.zero;
        //         temp.Set(player.transform.position.x, groundHit.point.y + 0.05f);
        //         player.transform.position = temp;
        //     }
        // }
    }
}
