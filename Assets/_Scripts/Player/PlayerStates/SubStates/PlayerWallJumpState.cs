using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerWallJumpState : PlayerAbilityState {
    protected int wallJumpDirection;

    protected float nextVelocity;
    protected Vector2 nextAngle;
    protected int nextDirection; 
    protected float usedTime;
    protected float elapsedJumpTime;

    public PlayerWallJumpState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        player.Rb.gravityScale = 0f;
        elapsedJumpTime = 0f;

        player.InputHandler.UseJumpInput();

        player.AirborneState.StopWallJumpCoyoteTime();
        player.JumpState.ResetAmountOfJumpsLeft();
        player.JumpState.DecreaseAmountOfJumpsLeft();

        isWallJumping = true;

        RaycastHit2D wallHit = player.GetWallHit(nextDirection);

        if (wallHit) wallJumpDirection = -nextDirection;
        else wallJumpDirection = nextDirection;

        player.SetVelocity(nextVelocity, nextAngle, wallJumpDirection, true);
        player.CheckFacingDirection(wallJumpDirection);
    }

    public override void Exit() {
        base.Exit();

        player.Rb.gravityScale = playerData.defaultGravityScale;

        isWallJumping = false;
        elapsedJumpTime = 0f;
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        CheckWallJumpMultiplier();

        elapsedJumpTime += Time.deltaTime;

        if (elapsedJumpTime >= usedTime || isTouchingCeiling || isOnSolidGround)
            isAbilityDone = true;
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();
    }

    private void CheckWallJumpMultiplier() {
        if (!isWallJumping) return;

        if (jumpInputStop) {
            player.InputHandler.UseJumpStopInput();
            player.SetVelocityY(player.CurrentVelocity.y * playerData.variableJumpHeightMultiplier);
            isWallJumping = false;
            isAbilityDone = true;
        }
        else if (isFalling) {
            player.InputHandler.UseJumpStopInput();
            isAbilityDone = true;
            isWallJumping = false;
        }
    }

    public void WallJumpConfiguration(float velocity, Vector2 angle, int direction, float time) {
        nextVelocity = velocity;
        nextAngle = angle;
        nextDirection = direction;

        usedTime = time;

        Debug.Log($"Wall Jumped");
    }
}
