using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Assets/Data/Entity Data/Enemy")]
public class EnemyData : EntityData {
    [Space(20)]

    [Header("--- States Info ---")]
    [Space(5)]
    public bool isGrounded;
    public bool isOnSolidGround;
    public bool isOnPlatform;
    public bool isIgnoringPlatforms;
    public bool isOnSlope;
    public bool isAirborne;
    public bool isIdle;
    public bool isMoving;
    public bool isRunning;
    public bool isRunningAtMaxSpeed;
    public bool isChangingDirections;
    public bool isSprinting;
    public bool isSprintingAtMaxSpeed;
    public bool isCrouching;
    public bool isGroundSliding;
    public bool stopSlide;
    public bool isJumping;
    public bool isAscending;
    public bool isFalling;
    public bool isFastFalling;
    public bool isTouchingCeiling;
    public bool isTouchingWall;
    public bool isTouchingBackWall;
    public bool hasTouchedWall;
    public bool hasTouchedWallBack;
    public bool isTouchingLedge;
    public bool isTouchingLedgeWithFoot;
    public bool isWallSliding;
    public bool isWallGrabing;
    public bool isWallClimbing;
    public bool isWallJumping;
    public bool isHanging;
    public bool isClimbing;
    public bool isDamaged;
    public bool isDead;
    public bool isDeadOnGround;
    public bool isInvulnerable;
    public bool isAnimationFinished;
    public bool isAbilityDone;
    public bool isExitingState;
    public bool hasCoyoteTime;
    public bool hasWallJumpCoyoteTime;
    public bool hasGroundSlideTime;
    public bool isPatroling;
    public bool isChasing;

    [Range(0f, 30f)] public float maxHorizontalSpeed;
    public float maxAscendantSpeed;
    public float defaultFallSpeed;
    public float moveSpeed;
    public bool lerpVelocity = true;
    public bool enableFriction = true;
    public float frictionAmount = 1f;
    [Range(0, 100)] public int runAcceleration;
    [Range(0, 100)] public int runDecceleration;
    [Range(0, 100)] public int runDirectionChangeAcceleration;

    public Vector2 groundCheckOffset;
    public float groundCheckDistance;
    public Vector2 wallCheckOffset;
    public float wallCheckDistance;
    public Vector2 ledgeCheckOffset;
    public float ledgeCheckDistance;
    public Vector2 ledgeFootCheckOffset;
    public float ledgeFootCheckDistance;

    public LayerMask groundLayer;
    public LayerMask solidsLayer;
    public LayerMask wallLayer;
    public LayerMask platformLayer;

    public bool doesIdle = true;
    public bool doesPatrol = true;

    public bool infiniteIdle = true;
    public bool infinitePatrol = true;
    public float idleTimeRange = 5f;
    public float patrolTimeRange = 7.5f;

    public override void OnEnable() {
        this.Init();
    }

    public override void Init() {
        currentVelocity = Vector2.zero;
        facingDirection = Direction.Right;
        currentLives = 0;
        currentHealth = 0f;
        currentHearts = 0;
        currentLayer = "Enemy";
    }
}
