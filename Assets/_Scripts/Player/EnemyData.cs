using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Assets/Data/Entity Data/Enemy")]
public class EnemyData : EntityData {
    public override void OnEnable() {
        Init();
    }

    public override void Init() {
        currentVelocity = Vector2.zero;
        facingDirection = Direction.Right;
        currentLives = 0;
        currentHealth = 0f;
        currentHearts = 0;
        currentLayer = "Player";

        // isGrounded = false;
        // isOnSolidGround = false;
        // isOnPlatform = false;
        // isIgnoringPlatforms = false;
        // isOnSlope = false;
        // isCrouching = false;
        // isAirborne = false;
        // isJumping = false;
        // isAscending = false;
        // isFalling = false;
        // isFastFalling = false;
        // isTouchingCeiling = false;
        // isTouchingWall = false;
        // isTouchingBackWall = false;
        // hasTouchedWall = false;
        // hasTouchedWallBack = false;
        // isTouchingLedge = false;
        // isWallSliding = false;
        // isWallGrabing = false;
        // isWallClimbing = false;
        // isHanging = false;
        // isClimbing = false;
        // isDamaged = false;
        // isDead = false;
        // isDeadOnGround = false;
        // isInvulnerable = false;
        // isAnimationFinished = false;
        // isExitingState = false;
        // isAbilityDone = false;
        // hasCoyoteTime = false;
        // hasWallJumpCoyoteTime = false;
    }
}
