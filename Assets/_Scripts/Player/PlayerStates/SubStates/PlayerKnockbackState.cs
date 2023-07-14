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
        hasBouncedOffWall = false;
        hasBouncedOffCeiling = false;
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

        if (isTouchingBackWall && !hasBouncedOffWall) {
            BounceOffWall();
        }

        if (isTouchingCeiling && player.CurrentVelocity.y >= 0f && !hasBouncedOffCeiling) {
            BounceOffCeiling();
        }

        if (hasBouncedOffWall) hasBouncedOffWall = false;
        if (hasBouncedOffCeiling) hasBouncedOffCeiling = false;

        if (cumulatedKnockbackTime < playerData.maxKnockbackTime) cumulatedKnockbackTime += Time.deltaTime;

        if (cumulatedKnockbackTime < playerData.minKnockbackTime) return;
        else if (cumulatedKnockbackTime >= playerData.maxKnockbackTime) {
            stateMachine.ChangeState(player.AirborneState);
        }

        if (isGrounded) {
            stateMachine.ChangeState(player.LandState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (bounceOffWall) {
            player.SetVelocityX(-player.CurrentVelocity.x * playerData.wallBounceFalloff);
            bounceOffWall = false;
            hasBouncedOffWall = true;
        }

        if (bounceOffCeiling) {
            player.SetVelocityY(player.CurrentVelocity.y * -1);
            bounceOffCeiling = false;
            hasBouncedOffCeiling = true;
        }

        if (isFalling) {
            playerData.currentFallSpeed = playerData.defaultFallSpeed;
            player.SetVelocityY(player.CurrentVelocity.y, playerData.fallAcceleration, playerData.lerpVerticalVelocity);
        }
        else if (isAscending) player.SetVelocityY(player.CurrentVelocity.y);
    }
}
