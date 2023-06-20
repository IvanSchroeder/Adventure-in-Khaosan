using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKnockbackState : PlayerState {
    public PlayerKnockbackState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void DoChecks() {
        base.DoChecks();
    }

    public override void Enter() {
        base.Enter();

        isDamaged = true;
        isInvulnerable = true;

        player.SetVelocity(playerData.jumpHeight * 0.5f, (Vector2.up + Vector2.right).normalized, -player.FacingDirection);

        player.HitboxCollider.enabled = false;
    }

    public override void Exit() {
        base.Exit();

        isDamaged = false;
        // set invulnerability, set sprite transparency flash, return control to player
        player.HitboxCollider.enabled = true;
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        if (isGrounded && isFalling) {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        player.SetVelocityX(player.CurrentVelocity.x);

        if (isFalling) {
            playerData.currentFallSpeed = playerData.defaultFallSpeed;
            player.SetVelocityY(player.CurrentVelocity.y, playerData.fallAcceleration, playerData.lerpVerticalVelocity);
        }
        else if (isAscending) player.SetVelocityY(player.CurrentVelocity.y);
    }
}
