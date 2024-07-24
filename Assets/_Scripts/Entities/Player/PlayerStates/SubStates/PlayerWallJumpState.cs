using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerWallJumpState : PlayerAbilityState {
    protected bool isWallHoping;
    protected int wallJumpDirection;

    protected float nextVelocity;
    protected Vector2 nextAngle;
    protected int nextDirection; 
    protected float usedJumpTime;
    protected float elapsedJumpTime;

    public PlayerWallJumpState(Player player, StateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();
        
        player.Rb.gravityScale = 0f;
        elapsedJumpTime = 0f;

        player.InputHandler.UseJumpInput();

        player.AirborneState.StopWallJumpCoyoteTime();
        player.JumpState.ResetAmountOfJumpsLeft();
        player.JumpState.DecreaseAmountOfJumpsLeft();

        player.SetVelocity(nextVelocity, nextAngle, nextDirection, true);

        RaycastHit2D wallHit = player.GetWallHit(-player.FacingDirection);

        AudioManager.instance.PlaySFX("PlayerJump");
    }

    public override void Exit() {
        base.Exit();

        player.Rb.gravityScale = playerData.defaultGravityScale;
        elapsedJumpTime = 0f;

        isWallJumping = false;
        isWallHoping = false;
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        CheckWallJumpMultiplier();

        elapsedJumpTime += Time.deltaTime;

        if (elapsedJumpTime >= usedJumpTime || (elapsedJumpTime >= playerData.minWallJumpTime && (player.isTouchingCeiling || player.isTouchingWall || player.isOnSolidGround)))
            player.isAbilityDone = true;
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();
    }

    private void CheckWallJumpMultiplier() {
        if (!isWallJumping || isWallHoping) return;

        if (jumpInputStop && elapsedJumpTime >= playerData.minWallJumpTime) {
            player.InputHandler.UseJumpStopInput();
            player.SetVelocityY(player.CurrentVelocity.y * playerData.variableJumpHeightMultiplier);
            isWallJumping = false;
            player.isAbilityDone = true;
        }
        else if (player.isFalling) {
            player.InputHandler.UseJumpStopInput();
            player.isAbilityDone = true;
            isWallJumping = false;
        }
    }

    public void WallJumpConfiguration(float velocity, Vector2 angle, int direction, float time, bool wallHop = false) {
        player.CheckFacingDirection(-direction);

        nextVelocity = velocity;
        nextAngle = angle;
        nextDirection = -direction;
        usedJumpTime = time;

        isWallJumping = !wallHop;
        isWallHoping = wallHop;
    }
}
