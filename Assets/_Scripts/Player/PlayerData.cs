using UnityEngine;
using ExtensionMethods;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "Assets/Data/Entity/Player")]
public class PlayerData : EntityData {
    // [Header("--- Entity Info ---")]
    // [Space(5)]

    // [Header("Values")]

    // public Vector2 currentVelocity;
    // public ColliderConfiguration currentColliderConfiguration;
    // public Direction facingDirection = Direction.Right;
    // // public int currentLives;
    // // public float currentHealth;
    // // public int currentHearts;
    // public string currentLayer;
    // public float currentGravityScale;
    // public float currentFallSpeed;
    // public int amountOfJumpsLeft;
    // public float slopeDownAngle;
    // public float slopeSideAngle;
    // public float cumulatedKnockbackTime;
    
    [Header("--- Inputs Info ---")]
    [Space(5)]
    public int xInput;
    public int lastXInput;
    public int yInput;
    public bool jumpInput;
    public bool jumpInputStop;
    public bool jumpInputHold;
    public bool interactInput;
    public bool interactInputHold;
    public bool interactInputStop;
    public bool crouchInput;
    public bool crouchInputHold;
    public bool crouchInputStop;
    public bool unplatformInput;
    public bool attackInput;
    public bool attackInputHold;
    public bool attackInputStop;
    public bool grabInput;

    [Space(20)]

    [Header("--- States Info ---")]
    [Space(5)]
    public bool isGrounded;
    public bool isOnSolidGround;
    public bool isOnPlatform;
    public bool isIgnoringPlatforms;
    public bool isOnSlope;
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
    public bool isAirborne;
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

    [Space(20)]

    [Header("--- States Locks ---")]
    [Space(5)]
    public bool canMove = true;
    public bool canSprint = true;
    public bool canCrouch = true;
    public bool canGroundSlide = true;
    public bool canJump = true;
    public bool canWallSlide = true;
    public bool canWallClimb = true;
    public bool canLedgeClimb = true;
    public bool canWallJump = true;
    public bool canAttack = true;

    // [Space(20)]

    // [Header("--- References ---")]
    // [Space(5)]
    // public PhysicsMaterial2D noFrictionMaterial;
    // public PhysicsMaterial2D fullFrictionMaterial;

    // [Space(20)]

    // [Header("--- Health Parameters ---")]
    // [Space(5)]
    // public HealthType healthType = HealthType.Hearts;
    // public int maxLives;
    // public float maxHealth;
    // public int maxHearts;
    // public float invulnerabilitySeconds;
    // public float minKnockbackTime;
    // public float maxKnockbackTime;
    // public float deadOnGroundTime;

    [Space(20)]

    [Header("--- Movement Parameters ---")]
    [Space(5)]
    [Header("=== Ground ===")]
    [Space(5)]
    [Header("General")]
    [Space(3)]
    // public bool enableFriction = true;
    public bool stickToGround;
    public bool lerpVelocity = false;
    public bool conserveMomentum = false;
    [Range(0f, 30f)] public float maxHorizontalSpeed;
    [Space(3)]
    [Header("Run")]
    [Space(3)]
    [Range(0f, 10f)] public float runSpeed;
    [Range(0f, 1f)] public float maxRunSpeedThresholdMult;
    public float maxRunSpeedThreshold;
    [Range(0, 100)] public int runAcceleration;
    [Range(0, 100)] public int runDecceleration;
    [Range(0, 100)] public int runDirectionChangeAcceleration;
    [Space(3)]
    [Header("Sprint")]
    [Space(3)]
    [Range(0f, 30f)] public float sprintSpeed;
    [Range(0f, 1f)] public float maxSprintSpeedThresholdMult;
    public float maxSprintSpeedThreshold;
    [Range(0f, 2f)] public float sprintStopDelay;
    [Range(0, 100)] public int sprintAcceleration;
    [Range(0, 100)] public int sprintDecceleration;
    [Range(0, 100)] public int sprintDirectionChangeAcceleration;
    [Space(3)]
    [Header("Crouch")]
    [Space(3)]
    [Range(0f, 5f)] public float crouchWalkSpeed;
    [Range(0, 100)] public int crouchAcceleration;
    [Range(0, 100)] public int crouchDecceleration;
    [Space(3)]
    [Header("Ground Slide")]
    [Space(3)]
    [Range(0.2f, 5f)] public float groundSlideDuration;
    [Range(0.2f, 10f)] public float groundSlideMaxDuration;
    [Range(0, 2f)] public float groundSlideDelay;
    [Range(0f, 15f)] public float groundSlideSpeed;
    [Range(0, 100)] public int groundSlideAcceleration;
    [Range(0, 100)] public int groundSlideDecceleration;
    [Range(0, 100)] public int deathSlideDecceleration;

    [Space(5)]
    [Header("=== Air ===")]
    [Space(3)]
    public bool lerpVelocityInAir = false;
    [Range(0, 100)] public int airAcceleration;
    [Range(0, 100)] public int airDecceleration;
    [Range(0f, 2f)] public float collisionEnableDelay = 0.5f;

    [Space(5)]
    [Header("Colliders")]
    [Space(3)]
    public ColliderConfiguration standingColliderConfig;
    public ColliderConfiguration crouchColliderConfig;
    public ColliderConfiguration walledColliderConfig;
    public ColliderConfiguration rollColliderConfig;
    public float standColliderHeight;
    public float crouchColliderHeight;

    [Space(20)]

    [Header("--- Airborne Parameters ---")]
    [Space(5)]
    [Header("Jump")]
    [Space(3)]
    public int amountOfJumps = 1;
    public float jumpHeight;
    [Range(0f, 1f)] public float variableJumpHeightMultiplier = 0.5f;
    public float cornerCorrectionRepositionOffset = 0.015f;
    public float maxAscendantSpeed;
    public int maxBouncesOffGround = 3;
    [Range(0f, 1f)] public float wallBounceFalloff = 0.5f;
    [Range(0f, 1f)] public float groundBounceXFalloff = 0.5f;
    [Range(0f, 1f)] public float groundBounceYFalloff = 0.5f;
    [Range(0f, 1f)] public float groundBounceThreshold = 0.5f;

    [Space(5)]
    [Header("Fall")]
    [Space(3)]
    public float defaultFallSpeed = 7f;
    public float fastFallSpeed = 10f;
    public float defaultGravityScale = 4f;
    public float fallThreshold = 0.01f;
    public bool lerpVerticalVelocity = false;
    [Range(0, 100)] public int fallAcceleration;
    [Range(0, 100)] public int ascensionAcceleration;

    [Space(5)]
    [Header("Buffer Timers")]
    [Space(3)]
    [Range(0f, 0.5f)] public float coyoteTime = 0.2f;
    [Range(0f, 0.5f)] public float wallJumpCoyoteTime = 0.15f;
    [Range(0f, 0.5f)] public float jumpBufferTime = 0.2f;

    [Space(20)]

    [Header("--- Wall Parameters ---")]
    [Space(5)]
    public bool autoWallGrab = false;
    public bool autoWallSlide = false;
    public float wallSlideSpeed = 2.5f;
    public float wallClimbSpeed = 3f;
    public float fastWallSlideSpeed = 5f;
    public bool lerpWallVelocity = false;
    [Range(0, 100)] public int wallSlideAcceleration;
    [Range(0, 100)] public int wallGrabAcceleration;
    [Range(0, 100)] public int wallClimbAcceleration;
    public float wallJumpSpeed = 15f;
    public float wallHopSpeed = 10f;
    [Range(0f, 0.5f)] public float wallJumpTime = 0.4f;
    [Range(-90f, 90f)] public float wallJumpAngle;
    [Range(-90f, 90f)] public float wallHopAngle;
    public Vector2 wallJumpDirectionOffAngle;
    public Vector2 wallHopDirectionOffAngle;

    [Space(20)]

    [Header("--- Ledge Climb Parameters ---")]
    [Space(5)]
    [Header("Offsets")]
    [Space(3)]
    public Vector2 startOffset;
    public Vector2 stopOffset;

    [Space(20)]

    [Header("--- Check Parameters ---")]
    [Space(5)]
    [Header("Ground")]
    [Space(3)]
    // public Vector2 groundCheckSize;
    public Vector2 groundCheckOffset;
    public float groundCheckDistance;
    public float slopeCheckDistance;

    [Space(5)]
    [Header("Walls")]
    [Space(3)]
    // public Vector2 wallCheckSize;
    public Vector2 wallCheckOffset;
    public float wallCheckDistance;
    public Vector2 ledgeCheckOffset;
    public float ledgeCheckDistance;

    [Space(5)]
    [Header("Ceiling")]
    [Space(3)]
    // public Vector2 ceilingCheckSize;
    public Vector2 ceilingCheckOffset;
    public float ceilingCheckDistance;
    public Vector2 cornerInnerCheckOffset;
    public Vector2 cornerEdgeCheckOffset;
    public float cornerCheckDistance;
    public float topCheckDistance;

    [Space(5)]
    [Header("Layers")]
    [Space(3)]
    public LayerMask groundLayer;
    public LayerMask solidsLayer;
    public LayerMask wallLayer;
    public LayerMask platformLayer;

    public override void OnEnable() {
        Init();
    }

    public override void Init() {
        currentVelocity = Vector2.zero;
        currentColliderConfiguration = standingColliderConfig;
        facingDirection = Direction.Right;
        currentLives = 0;
        currentHealth = 0f;
        currentHearts = 0;
        currentLayer = "Player";
        currentFallSpeed = defaultFallSpeed;
        currentGravityScale = defaultGravityScale;
        cumulatedGroundSlideTime = 0f;
        cumulatedGroundSlideCooldown = groundSlideDelay;
        cumulatedKnockbackTime = 0f;
        cumulatedWallJumpCoyoteTime = 0f;
        amountOfJumpsLeft = amountOfJumps;
        maxRunSpeedThreshold = Mathf.Lerp(0f, runSpeed, maxRunSpeedThresholdMult);
        maxSprintSpeedThreshold = Mathf.Lerp(runSpeed, sprintSpeed, maxSprintSpeedThresholdMult);

        xInput = 0;
        lastXInput = 0;
        yInput = 0;
        jumpInput = false;
        jumpInputStop = false;
        jumpInputHold = false;
        attackInput = false;
        attackInputHold = false;
        attackInputStop = false;
        interactInput = false;
        interactInputHold = false;
        interactInputStop = false;
        crouchInput = false;
        crouchInputHold = false;
        crouchInputStop = false;
        unplatformInput = false;
        grabInput = false;

        isGrounded = false;
        isOnSolidGround = false;
        isOnPlatform = false;
        isIgnoringPlatforms = false;
        isOnSlope = false;
        isIdle = true;
        isMoving = false;
        isRunning = false;
        isRunningAtMaxSpeed = false;
        isChangingDirections = false;
        isSprinting = false;
        isSprintingAtMaxSpeed = false;
        isCrouching = false;
        isGroundSliding = false;
        stopSlide = false;
        isAirborne = false;
        isJumping = false;
        isAscending = false;
        isFalling = false;
        isFastFalling = false;
        isTouchingCeiling = false;
        isTouchingWall = false;
        isTouchingBackWall = false;
        hasTouchedWall = false;
        hasTouchedWallBack = false;
        isTouchingLedge = false;
        isWallSliding = false;
        isWallGrabing = false;
        isWallClimbing = false;
        isWallJumping = false;
        isHanging = false;
        isClimbing = false;
        isDamaged = false;
        isDead = false;
        isDeadOnGround = false;
        isInvulnerable = false;
        isAnimationFinished = false;
        isAbilityDone = false;
        isExitingState = false;
        hasCoyoteTime = false;
        hasWallJumpCoyoteTime = false;
        hasGroundSlideTime = false;

        wallJumpDirectionOffAngle = wallJumpAngle.AngleFloatToVector2();
        wallHopDirectionOffAngle = wallHopAngle.AngleFloatToVector2();
    }
}
