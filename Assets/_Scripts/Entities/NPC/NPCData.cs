using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCData", menuName = "Assets/Data/Entity Data/NPC ")]
public class NPCData : EntityData {
    [Space(20)]

    [Header("--- States Info ---")]
    [Space(5)]
    public bool isGrounded;
    public bool isOnSolidGround;
    public bool isOnSlope;
    public bool isOnPlatform;
    public bool isAirborne;
    public bool isIdle;
    public bool isMoving;
    public bool isChangingDirections;
    public bool isJumping;
    public bool isAscending;
    public bool isFalling;
    public bool isTouchingCeiling;
    public bool isTouchingWall;
    public bool isTouchingBackWall;
    public bool hasTouchedWall;
    public bool hasTouchedWallBack;
    public bool isTouchingLedge;
    public bool isTouchingLedgeWithFoot;
    public bool isInvulnerable;
    public bool isInteracted;
    public bool isNearPlayer;

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
    public bool doesWander = true;
    public bool doesInteract = true;

    [Header("Idle Parameters")]
    [Space(5)]
    public bool infiniteIdle = true;
    public bool infinitePatrol = true;
    public float idleTimeRange = 5f;
    public bool hasRandomIdle = false;

    [Space(20)]

    [Header("Idle Parameters")]
    [Space(5)]
    public float wanderTimeRange = 7.5f;
    public bool hasLimitWander = true;
    public float wanderLimitRadius = 10f;

    public bool canDetectPlayer;
    public float playerDetectionRange;
    public LayerMask playerLayer;

    public override void OnEnable() {
        this.Init();
    }

    public override void Init() {
        currentVelocity = Vector2.zero;
        facingDirection = Direction.Right;
        currentLayer = "NPC";
    }
}
