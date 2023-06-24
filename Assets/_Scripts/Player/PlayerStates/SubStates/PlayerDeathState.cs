using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeathState : PlayerState {
    protected bool deathLock;

    public PlayerDeathState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        isDead = true;
        isDeadOnGround = false;
        deathLock = false;
        cumulatedDeathTime = 0f;

        player.SetVelocity(playerData.wallJumpSpeed, Vector2.up + Vector2.right, -player.FacingDirection);

        player.Anim.SetBool("deadOnAir", true);
    }

    public override void Exit() {
        base.Exit();

        isDead = false;
        isDeadOnGround = false;

        player.Anim.SetBool("deadOnAir", false);
        player.Anim.SetBool("deadOnGround", false);
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        if (isTouchingBackWall) {
            BounceOffWall();
        }

        if (isGrounded) {
            player.Anim.SetBool("deadOnAir", false);

            if (player.CurrentVelocity.x == 0f) {
                if (!isDeadOnGround) {
                    player.Anim.SetBool("deadOnGround", true);
                    player.KnockbackEnd();
                    isDeadOnGround = true;
                }
            }
        }
        else if (isAirborne || isFalling)  {
            player.Anim.SetBool("deadOnAir", true);
        }

        if (isDeadOnGround) {
            cumulatedDeathTime += Time.deltaTime;

            if (cumulatedDeathTime >= playerData.deadOnGroundTime && !deathLock) {
                deathLock = true;
                player.PlayerDeathEnd();
            }
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (isOnSolidGround) {
            player.SetVelocityX(0f, 10f, playerData.lerpVelocity);
        }

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
}
