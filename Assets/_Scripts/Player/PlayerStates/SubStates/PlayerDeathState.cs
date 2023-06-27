using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerDeathState : PlayerState {
    protected bool deathLock;
    protected bool groundLock;
    protected int bouncesOffGroundCount;
    protected bool isOutOfBounces;
    protected float currentBounceXSpeed;
    protected float lastBounceXSpeed;
    protected float currentBounceYSpeed;
    protected float lastBounceYSpeed;

    public PlayerDeathState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void AnimationTrigger() {
        base.AnimationTrigger();

        isDeadOnGround = true;
    }

    public override void Enter() {
        base.Enter();

        player.SetVelocity(playerData.jumpHeight, playerData.wallJumpDirectionOffAngle, -player.FacingDirection);

        isDead = true;
        isDeadOnGround = false;
        deathLock = false;
        groundLock = false;
        cumulatedDeathTime = 0f;
        hasBouncedOffGround = false;
        hasBouncedOffWall = false;
        hasBouncedOffCeiling = false;

        bouncesOffGroundCount = 0;
        isOutOfBounces = false;
        
        playerData.currentFallSpeed = playerData.defaultFallSpeed;

        currentBounceXSpeed = playerData.runSpeed;
        currentBounceYSpeed = playerData.jumpHeight;

        player.Anim.SetBool("deadSpin", true);
        player.Anim.SetBool("deadOnGround", false);
        player.Anim.SetBool("deadOnSlide", false);
        player.Anim.SetBool("deadOnFall", false);
    }

    public override void Exit() {
        base.Exit();

        isDead = false;
        isDeadOnGround = false;

        player.Anim.SetBool("deadSpin", false);
        player.Anim.SetBool("deadOnGround", false);
        player.Anim.SetBool("deadOnSlide", false);
        player.Anim.SetBool("deadOnFall", false);
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        if (isOnSolidGround && !isOutOfBounces && !hasBouncedOffGround) {
            // player.SetVelocityY(0f);
            player.Anim.SetBool("deadSpin", true);
            player.Anim.SetBool("deadOnFall", false);
            BounceOffGround();
        }

        float dotProduct = Vector2.Dot(Vector2.right * player.FacingDirection, Vector2.right * player.CurrentVelocity.x.Sign());
        Debug.Log($"Dot product {dotProduct}");

        if (isTouchingWall && !hasBouncedOffWall && ((dotProduct == 1 && isTouchingWall) || (dotProduct == -1 && isTouchingBackWall))) {
            Debug.Log($"Dot product {dotProduct}");
            player.Anim.SetBool("deadSpin", true);
            player.Anim.SetBool("deadOnFall", false);
            BounceOffWall();
        }

        if (isTouchingCeiling && !hasBouncedOffCeiling) {
            player.Anim.SetBool("deadSpin", true);
            player.Anim.SetBool("deadOnFall", false);
            player.SetVelocityY(0f);
            BounceOffCeiling();
        }

        if (isOnSolidGround && isOutOfBounces) {
            player.Anim.SetBool("deadOnSlide", true);
            player.Anim.SetBool("deadOnFall", false);
            player.Anim.SetBool("deadSpin", false);

            if (player.CurrentVelocity.x == 0f ) {
                if (!groundLock) {
                    groundLock = true;
                    player.Anim.SetBool("deadOnGround", true);
                    player.SetColliderParameters(player.MovementCollider, playerData.crouchColliderConfig);
                    player.KnockbackEnd(null, null);
                }
            }
        }
        else if (!isOutOfBounces && isFalling)  {
            player.CheckFacingDirection(player.CurrentVelocity.x.Sign());
            player.Anim.SetBool("deadOnFall", true);
            player.Anim.SetBool("deadSpin", false);
            player.Anim.SetBool("deadOnSlide", false);
        }

        if (isFalling && hasBouncedOffGround) {
            hasBouncedOffGround = false;
        }

        if (isAscending && hasBouncedOffCeiling) {
            hasBouncedOffCeiling = false;
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

        if (isExitingState) return;

        if (isOnSolidGround && isOutOfBounces) {
            if (isTouchingWall) {
                player.CheckFacingDirection(-player.FacingDirection);
                player.SetVelocityX(-player.CurrentVelocity.x * playerData.wallBounceFalloff);
                isTouchingWall = false;
            }

            player.SetVelocityX(0f, playerData.deathSlideDecceleration, playerData.lerpVelocity);
            player.SetVelocityY(player.CurrentVelocity.y, playerData.fallAcceleration, playerData.lerpVerticalVelocity);
        }
        else {
            player.SetVelocityX(player.CurrentVelocity.x);

            if (!bounceOffGround && !bounceOffCeiling) {
                if (isFalling) player.SetVelocityY(player.CurrentVelocity.y, playerData.fallAcceleration, playerData.lerpVerticalVelocity);
                else if (isAscending) player.SetVelocityY(player.CurrentVelocity.y);
            }

            bool hasAlreadyBouncedOffWall = false;

            if (bounceOffWall) {
                Debug.Log("Bounced off Wall");

                lastBounceXSpeed = currentBounceXSpeed;
                currentBounceXSpeed = lastBounceXSpeed * playerData.wallBounceFalloff;
                player.SetVelocityX(currentBounceXSpeed);
                bounceOffWall = false;

                hasBouncedOffWall = false;
                hasAlreadyBouncedOffWall = true;
            }

            if (bounceOffGround) {
                Debug.Log("Bounced off Ground");

                if (bounceOffWall && !hasAlreadyBouncedOffWall) {
                    Debug.Log("Bounced off Wall");

                    lastBounceXSpeed = currentBounceXSpeed;
                    currentBounceXSpeed = lastBounceXSpeed * playerData.wallBounceFalloff;
                    player.SetVelocityX(currentBounceXSpeed * player.CurrentVelocity.x.Sign());
                    bounceOffWall = false;

                    hasBouncedOffWall = false;
                    hasAlreadyBouncedOffWall = true;
                }
                else {
                    lastBounceXSpeed = currentBounceXSpeed;
                    currentBounceXSpeed = lastBounceXSpeed * playerData.bounceOffGroundXFalloff;
                    player.SetVelocityX(currentBounceXSpeed * player.CurrentVelocity.x.Sign());
                }

                lastBounceYSpeed = currentBounceYSpeed;
                currentBounceYSpeed = lastBounceYSpeed * playerData.bounceOffGroundYFalloff;
                player.SetVelocityY(currentBounceYSpeed);

                bouncesOffGroundCount += 1;
                Debug.Log($"Bounces left: {playerData.maxBouncesOffGround - bouncesOffGroundCount}");
                if (bouncesOffGroundCount >= playerData.maxBouncesOffGround) isOutOfBounces = true;

                hasBouncedOffGround = true;
                bounceOffGround = false;
            }

            if (bounceOffCeiling) {
                Debug.Log("Bounced off Ceiling");

                if (bounceOffWall && !hasAlreadyBouncedOffWall) {
                    Debug.Log("Bounced off Wall");

                    lastBounceXSpeed = currentBounceXSpeed;
                    currentBounceXSpeed = lastBounceXSpeed * playerData.wallBounceFalloff;
                    player.SetVelocityX(currentBounceXSpeed * player.CurrentVelocity.x.Sign());
                    bounceOffWall = false;

                    hasBouncedOffWall = false;
                    hasAlreadyBouncedOffWall = true;
                }
                else {
                    lastBounceXSpeed = currentBounceXSpeed;
                    currentBounceXSpeed = lastBounceXSpeed * playerData.bounceOffGroundXFalloff;
                    player.SetVelocityX(currentBounceXSpeed * player.CurrentVelocity.x.Sign());
                }

                lastBounceYSpeed = currentBounceYSpeed;
                currentBounceYSpeed = lastBounceYSpeed * playerData.bounceOffGroundYFalloff;
                player.SetVelocityY(currentBounceYSpeed * Vector2.down.y);

                hasBouncedOffCeiling = true;
                bounceOffCeiling = false;
            }
        }

    }
}
