using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerKnockbackState : PlayerState {
    public PlayerKnockbackState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        isDamaged = true;
        isInvulnerable = true;
        lastKnockbackFacingDirection = player.FacingDirection;

        cumulatedKnockbackTime = 0f;

        player.SetVelocity(playerData.jumpHeight * 0.5f, (Vector2.up + Vector2.right).normalized, -player.FacingDirection);
        // player.CanMove = false;
    }

    public override void Exit() {
        base.Exit();

        isDamaged = false;
        player.KnockbackEnd(null, null);
        // player.CanMove = true;
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        if (isTouchingBackWall) {
            BounceOffWall();
        }

        if (cumulatedKnockbackTime < playerData.maxKnockbackTime) cumulatedKnockbackTime += Time.deltaTime;
        
        if (isGrounded) {
            stateMachine.ChangeState(player.LandState);
        }

        if (cumulatedKnockbackTime < playerData.minKnockbackTime) return;

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

        if (isFalling) {
            playerData.currentFallSpeed = playerData.defaultFallSpeed;
            player.SetVelocityY(player.CurrentVelocity.y, playerData.fallAcceleration, playerData.lerpVerticalVelocity);
        }
        else if (isAscending) player.SetVelocityY(player.CurrentVelocity.y);
    }

    public void SetLastContactPoint(Vector2 point) {
        lastContactPoint = point;
    }
}
