using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKnockbackState : PlayerState {
    protected Vector2 lastContactPoint;
    protected int lastFacingDirection;
    protected bool bounceOffWall;

    public PlayerKnockbackState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void DoChecks() {
        base.DoChecks();
    }

    public override void Enter() {
        base.Enter();

        isDamaged = true;
        isInvulnerable = true;
        lastFacingDirection = player.FacingDirection;

        cumulatedKnockbackTime = 0f;

        player.SetVelocity(playerData.jumpHeight * 0.5f, (Vector2.up + Vector2.right).normalized, -player.FacingDirection);
        // player.CanMove = false;
    }

    public override void Exit() {
        base.Exit();

        isDamaged = false;
        player.KnockbackEnd();
        // player.CanMove = true;
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        if (isTouchingBackWall) {
            BounceOffWall();
        }

        if (cumulatedKnockbackTime < playerData.maxKnockbackTime) cumulatedKnockbackTime += Time.deltaTime;
        if (cumulatedKnockbackTime < playerData.minKnockbackTime) return;
        
        if (isGrounded) {
            stateMachine.ChangeState(player.LandState);
        }
        else if (cumulatedKnockbackTime >= playerData.maxKnockbackTime) {
            stateMachine.ChangeState(player.AirborneState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (bounceOffWall) {
            player.SetVelocityX(-player.CurrentVelocity.x * playerData.wallBounceFalloff);
            bounceOffWall = false;
        }
        else {
            player.SetVelocityX(player.CurrentVelocity.x);
        }

        if (isFalling) {
            playerData.currentFallSpeed = playerData.defaultFallSpeed;
            player.SetVelocityY(player.CurrentVelocity.y, playerData.fallAcceleration, playerData.lerpVerticalVelocity);
        }
        else if (isAscending) player.SetVelocityY(player.CurrentVelocity.y);
    }

    public void SetLastContactPoint(Vector2 point) {

    }

    public void BounceOffWall() {
        bounceOffWall = true;
        lastFacingDirection = player.FacingDirection;
        player.CheckFacingDirection(-lastFacingDirection);
        isTouchingBackWall = false;
    }
}
