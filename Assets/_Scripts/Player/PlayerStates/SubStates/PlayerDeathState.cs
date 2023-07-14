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
        player.Rb.gravityScale = playerData.defaultGravityScale;

        currentBounceXSpeed = playerData.runSpeed;
        currentBounceYSpeed = playerData.jumpHeight;
        lastBounceXSpeed = currentBounceXSpeed;
        lastBounceYSpeed = currentBounceYSpeed;

        player.InteractorSystem.CanInteract = false;

        player.Anim.SetBool("deadSpin", true);
        player.Anim.SetBool("deadOnGround", false);
        player.Anim.SetBool("deadOnSlide", false);
        // player.Anim.SetBool("deadOnFall", false);
        player.Anim.SetBool("deadOnFall", false);
    }

    public override void Exit() {
        base.Exit();

        isDead = false;
        isDeadOnGround = false;

        player.InteractorSystem.CanInteract = true;

        player.Anim.SetBool("deadSpin", false);
        player.Anim.SetBool("deadOnGround", false);
        player.Anim.SetBool("deadOnSlide", false);
        player.Anim.SetBool("deadOnFall", false);
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        if (isOnSolidGround) {
            player.Anim.SetBool("deadSpin", false);
            player.Anim.SetBool("deadOnSlide", true);

            if (player.CurrentVelocity.x == 0f) {
                if (!groundLock) {
                    groundLock = true;
                    player.Anim.SetBool("deadOnGround", true);
                    player.Anim.SetBool("deadOnSlide", false);
                    player.SetColliderParameters(player.MovementCollider, playerData.crouchColliderConfig, true);
                    player.SetColliderParameters(player.HitboxTrigger, playerData.crouchColliderConfig);
                    // player.KnockbackEnd(null, null);
                }
            }
        }
        else {
            player.Anim.SetBool("deadSpin", true);
            player.Anim.SetBool("deadOnSlide", false);

            if (isTouchingWall && !hasBouncedOffWall) {
                BounceOffWall();
            }

            if (isTouchingCeiling && !hasBouncedOffCeiling) {
                BounceOffCeiling();
            }
        }

        if (hasBouncedOffWall) hasBouncedOffWall = false;
        if (hasBouncedOffCeiling) hasBouncedOffCeiling = false;

        if (isDeadOnGround) {
            cumulatedDeathTime += Time.deltaTime;

            if (cumulatedDeathTime >= playerData.deadOnGroundTime && !deathLock) {
                deathLock = true;
                player.PlayerDeathEnd();
            }
        }

        // if (isOnSolidGround && !isOutOfBounces && !hasBouncedOffGround) {
        //     player.Anim.SetBool("deadSpin", true);
        //     player.Anim.SetBool("deadOnFall", false);
        //     BounceOffGround();
        // }

        // if (isTouchingWall && !hasBouncedOffWall) {
        //     player.Anim.SetBool("deadSpin", true);
        //     player.Anim.SetBool("deadOnFall", false);
        //     BounceOffWall();
        // }

        // if (isTouchingCeiling && !hasBouncedOffCeiling) {
        //     player.Anim.SetBool("deadSpin", true);
        //     player.Anim.SetBool("deadOnFall", false);
        //     BounceOffCeiling();
        // }

        // if ((isOutOfBounces || currentBounceYSpeed == 0f) && isOnSolidGround) {
        //     player.Anim.SetBool("deadOnSlide", true);
        //     player.Anim.SetBool("deadOnFall", false);
        //     player.Anim.SetBool("deadSpin", false);

        //     if (player.CurrentVelocity.x == 0f) {
        //         if (!groundLock) {
        //             groundLock = true;
        //             player.Anim.SetBool("deadOnGround", true);
        //             player.SetColliderParameters(player.MovementCollider, playerData.crouchColliderConfig);
        //             player.SetColliderParameters(player.HitboxTrigger, playerData.crouchColliderConfig);
        //             player.KnockbackEnd(null, null);
        //         }
        //     }
        // }
        // else if ((isOutOfBounces && isFalling) || (!isOutOfBounces && isFalling)) {
        //     player.CheckFacingDirection(player.CurrentVelocity.x.Sign());
        //     player.Anim.SetBool("deadOnFall", true);
        //     player.Anim.SetBool("deadOnSlide", false);
        //     player.Anim.SetBool("deadSpin", false);
        // }

        // if (isFalling && hasBouncedOffGround) {
        //     hasBouncedOffGround = false;
        // }

        // if (isAscending && hasBouncedOffCeiling) {
        //     hasBouncedOffCeiling = false;
        // }

        // if (hasBouncedOffWall) hasBouncedOffWall = false;

        // if (isDeadOnGround) {
        //     cumulatedDeathTime += Time.deltaTime;

        //     if (cumulatedDeathTime >= playerData.deadOnGroundTime && !deathLock) {
        //         deathLock = true;
        //         player.PlayerDeathEnd();
        //     }
        // }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        Debug.DrawRay(player.MidPoint.position, player.Rb.velocity.normalized * 3f, Color.green);

        // if (isOnSolidGround && isOutOfBounces) {
        //     if (isTouchingWall) {
        //         player.CheckFacingDirection(-player.FacingDirection);
        //         player.SetVelocityX(player.CurrentVelocity.x * player.FacingDirection);
        //         isTouchingWall = false;
        //     }

        //     player.SetVelocityX(0f, playerData.deathSlideDecceleration, playerData.lerpVelocity);
        //     player.SetVelocityY(player.CurrentVelocity.y, playerData.fallAcceleration * 2, playerData.lerpVerticalVelocity);
        // }
        // else {
        //     player.SetVelocityX(player.CurrentVelocity.x);
        //     player.SetVelocityY(player.CurrentVelocity.y, playerData.fallAcceleration, playerData.lerpVerticalVelocity);

        //     if (bounceOffWall) {
        //         Debug.Log("Bounced off Wall");

        //         currentBounceXSpeed = lastBounceXSpeed * playerData.wallBounceFalloff;
        //         lastBounceXSpeed = currentBounceXSpeed;
        //         player.SetVelocityX(currentBounceXSpeed * player.FacingDirection);

        //         hasBouncedOffWall = true;
        //         bounceOffWall = false;
        //     }

        //     if (bounceOffGround) {
        //         Debug.Log("Bounced off Ground");

        //         if (player.CurrentVelocity.x.AbsoluteValue() < playerData.groundBounceThreshold * playerData.runSpeed) {
        //             currentBounceXSpeed = playerData.runSpeed;
        //             lastBounceXSpeed = playerData.runSpeed;
        //         }
        //         else {
        //             currentBounceXSpeed = lastBounceXSpeed * playerData.groundBounceXFalloff;
        //             lastBounceXSpeed = currentBounceXSpeed;
        //         }
        //         player.SetVelocityX(currentBounceXSpeed * player.FacingDirection);

        //         currentBounceYSpeed = lastBounceYSpeed * playerData.groundBounceYFalloff;
        //         lastBounceYSpeed = currentBounceYSpeed;
        //         player.SetVelocityY(currentBounceYSpeed);

        //         bouncesOffGroundCount += 1;
        //         Debug.Log($"Bounces left: {playerData.maxBouncesOffGround - bouncesOffGroundCount}");
        //         if (bouncesOffGroundCount >= playerData.maxBouncesOffGround) isOutOfBounces = true;

        //         hasBouncedOffGround = true;
        //         bounceOffGround = false;
        //     }

        //     if (bounceOffCeiling) {
        //         Debug.Log("Bounced off Ceiling");

        //         currentBounceXSpeed = lastBounceXSpeed * playerData.groundBounceXFalloff;
        //         lastBounceXSpeed = currentBounceXSpeed;
        //         player.SetVelocityX(currentBounceXSpeed * player.CurrentVelocity.x.Sign());

        //         player.SetVelocityY((player.CurrentVelocity * Vector2.down).y);

        //         hasBouncedOffCeiling = true;
        //         bounceOffCeiling = false;
        //     }
        // }

        if (isOnSolidGround) {
            if (isTouchingWall) {
                player.SetVelocityX(player.CurrentVelocity.x * -player.FacingDirection);
                player.CheckFacingDirection(player.CurrentVelocity.x.Sign());
                isTouchingWall = false;
            }

            player.SetVelocityX(0f * player.FacingDirection, playerData.deathSlideDecceleration, playerData.lerpVelocity);
            player.SetVelocityY(player.CurrentVelocity.y);
        }
        else {
            if (bounceOffWall) {
                Debug.Log("Bounced off Wall");

                player.SetVelocityX(player.CurrentVelocity.x * player.FacingDirection * playerData.wallBounceFalloff);

                hasBouncedOffWall = true;
                bounceOffWall = false;
            }

            if (bounceOffCeiling) {
                Debug.Log("Bounced off Ceiling");

                player.SetVelocityY(player.CurrentVelocity.y * -1);

                hasBouncedOffCeiling = true;
                bounceOffCeiling = false;
            }
            
            player.SetVelocityX(player.CurrentVelocity.x, playerData.airAcceleration, playerData.lerpVelocityInAir);
            player.SetVelocityY(player.CurrentVelocity.y, playerData.fallAcceleration, playerData.lerpVerticalVelocity);
        }
    }
}
