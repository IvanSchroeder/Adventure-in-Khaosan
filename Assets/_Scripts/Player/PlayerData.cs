using UnityEngine;
using ExtensionMethods;

public enum Direction {
    Left,
    Right
}

[CreateAssetMenu(fileName = "newPlayerData", menuName = "Assets/Data/Player Data")]
public class PlayerData : ScriptableObject {
    [Header("--- Player Info ---")]

    [Space(5)]

    [Header("Values")]
    public Vector2 currentVelocity;
    public Direction facingDirection = Direction.Right;
    public string currentLayer;
    public float currentGravityScale;
    public float currentFallSpeed;
    public int amountOfJumpsLeft;
    public float slopeDownAngle;
    public float slopeSideAngle;

    [Space(5)]

    [Header("Inputs")]
    public int xInput;
    public int lastXInput;
    public int yInput;
    public bool jumpInput;
    public bool jumpInputStop;
    public bool jumpInputHold;
    public bool grabInput;

    [Space(5)]

    [Header("States")]
    public bool isGrounded;
    public bool isOnSolidGround;
    public bool isOnPlatform;
    public bool isIgnoringPlatforms;
    public bool isOnSlope;
    public bool isCrouching;
    public bool isAirborne;
    public bool isJumping;
    public bool isAscending;
    public bool isFalling;
    public bool isFallingThrough;
    public bool isTouchingCeiling;
    public bool isTouchingWall;
    public bool isTouchingBackWall;
    public bool hasTouchedWall;
    public bool hasTouchedWallBack;
    public bool isTouchingLedge;
    public bool isWallSliding;
    public bool isWallGrabing;
    public bool isWallClimbing;
    public bool isHanging;
    public bool isClimbing;
    public bool isAnimationFinished;
    public bool isExitingState;
    public bool isAbilityDone;
    public bool hasCoyoteTime;
    public bool hasWallJumpCoyoteTime;

    [Space(20)]

    [Header("--- References ---")]

    [Space(5)]

    public PhysicsMaterial2D noFrictionMaterial;
    public PhysicsMaterial2D fullFrictionMaterial;

    [Space(20)]

    [Header("--- Movement Values ---")]

    [Space(5)]

    [Header("Ground")]
    // public bool enableFriction = true;
    [Range(0f, 10f)] public float runSpeed;
    [Range(0f, 10f)] public float maxRunSpeed;
    [Range(0f, 5f)] public float crouchWalkSpeed;
    public bool lerpVelocity = false;
    [Range(0, 100)] public int runAcceleration;
    [Range(0, 100)] public int runDecceleration;
    [Range(0, 100)] public int crouchAcceleration;
    [Range(0, 100)] public int crouchDecceleration;

    [Space(5)]

    [Header("Air")]
    public bool lerpVelocityInAir = false;
    [Range(0, 100)] public int airDecceleration;
    [Range(0, 100)] public int airAcceleration;
    [Range(0f, 2f)] public float collisionEnableDelay = 0.5f;

    [Space(5)]

    [Header("Colliders")]
    public float standColliderHeight;
    public float crouchColliderHeight;

    [Space(20)]

    [Header("--- Jump Values ---")]
    public int amountOfJumps = 1;
    public float jumpHeight;
    [Range(0f, 1f)] public float variableJumpHeightMultiplier = 0.5f;
    public float cornerCorrectionRepositionOffset = 0.015f;
    [Range(0f, 1f)] public float platformIgnoreVerticalVelocityThreshold = 0.5f;

    [Space(20)]

    [Header("--- Airborne State ---")]

    [Space(5)]

    [Header("Fall")]
    public float defaultFallSpeed = 7f;
    public float fastFallSpeed = 10f;
    public float defaultGravityScale = 4f;
    public float fallThreshold = 0.01f;
    public bool lerpVerticalVelocity = false;
    [Range(0, 100)] public int fallAcceleration;
    [Range(0, 100)] public int ascensionAcceleration;

    [Space(5)]

    [Header("Buffer Timers")]
    [Range(0f, 0.5f)] public float coyoteTime = 0.2f;
    [Range(0f, 0.5f)] public float wallJumpCoyoteTime = 0.15f;
    [Range(0f, 0.5f)] public float jumpBufferTime = 0.2f;
    public float cumulatedWallJumpCoyoteTime;

    [Header("--- Wall States ---")]
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
    [Range(0f, 0.5f)] public float wallJumpTime = 0.4f;
    [Range(-90f, 90f)] public float wallJumpAngle;
    [Range(-90f, 90f)] public float wallHopAngle;
    public Vector2 wallJumpDirectionOffAngle;
    public Vector2 wallHopDirectionOffAngle;

    [Header("--- Ledge Climb State ---")]

    [Space(5)]

    [Header("Offsets")]
    public Vector2 startOffset;
    public Vector2 stopOffset;

    [Header("--- Check Variables ---")]

    [Space(5)]

    [Header("Values")]
    // public Vector2 groundCheckSize;
    public Vector2 groundCheckOffset;
    public float groundCheckDistance;
    public float slopeCheckDistance;
    // public Vector2 ceilingCheckSize;
    public Vector2 ceilingCheckOffset;
    public float ceilingCheckDistance;
    // public Vector2 wallCheckSize;
    public Vector2 wallCheckOffset;
    public float wallCheckDistance;
    public Vector2 ledgeCheckOffset;
    public float ledgeCheckDistance;
    public Vector2 cornerInnerCheckOffset;
    public Vector2 cornerEdgeCheckOffset;
    public float cornerCheckDistance;
    public float topCheckDistance;

    [Space(5)]

    [Header("Layers")]
    public LayerMask groundLayer;
    public LayerMask solidsLayer;
    public LayerMask wallLayer;
    public LayerMask platformLayer;

    public void ResetPlayerInfo() {
        currentVelocity = Vector2.zero;
        facingDirection = Direction.Right;
        amountOfJumpsLeft = amountOfJumps;
        currentLayer = "Player";
        currentFallSpeed = defaultFallSpeed;
        currentGravityScale = defaultGravityScale;
        currentGravityScale = defaultGravityScale;

        isGrounded = false;
        isOnSolidGround = false;
        isOnPlatform = false;
        isIgnoringPlatforms = false;
        isOnSlope = false;
        isCrouching = false;
        isAirborne = false;
        isJumping = false;
        isAscending = false;
        isFalling = false;
        isFallingThrough = false;
        isTouchingCeiling = false;
        isTouchingWall = false;
        isTouchingBackWall = false;
        hasTouchedWall = false;
        hasTouchedWallBack = false;
        isTouchingLedge = false;
        isWallSliding = false;
        isWallGrabing = false;
        isWallClimbing = false;
        isHanging = false;
        isClimbing = false;
        isAnimationFinished = false;
        isExitingState = false;
        isAbilityDone = false;
        hasCoyoteTime = false;
        hasWallJumpCoyoteTime = false;

        xInput = 0;
        lastXInput = 0;
        yInput = 0;
        jumpInput = false;
        jumpInputStop = false;
        jumpInputHold = false;
        grabInput = false;

        wallJumpDirectionOffAngle = wallJumpAngle.AngleFloatToVector2();
        wallHopDirectionOffAngle = wallHopAngle.AngleFloatToVector2();
    }

    // public void OnValidate() {
    //     // gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);

    //     // gravityScale = gravityStrength / Physics2D.gravity.y;


    //     // jumpForce = (Mathf.Abs(gravityStrength) * jumpTimeToApex);


    //     // maxAscendantSpeed = jumpForce * 2f;
     
    //     // runAccelAmount = (50 * runAcceleration) / maxRunSpeed;
    //     // runDeccelAmount = (50 * runDecceleration) / maxRunSpeed;
    //     // runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, maxRunSpeed);
    //     // runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, maxRunSpeed);
    //     wallJumpDirectionOffAngle = wallJumpAngle.AngleFloatToVector2();
    //     wallHopDirectionOffAngle = wallHopAngle.AngleFloatToVector2();
    // }


    //[Header("Player Info")]
    [HideInInspector] public float yVelocity;

    //[Header("Movement State")]
    [Space(40)]
    [Header("Old")]
    /*[ReadOnly]*/ public float runAccelAmount;
    /*[ReadOnly]*/ public float runDeccelAmount;
    public bool conserveMomentum = true;

    //[Header("Jump State")]
    public float jumpTimeToApex;
    public float jumpForce;

    //[Header("Gravity")]
    public float gravityStrength;
    public float gravityScale;

    public float fallGravityMult;
    public float fastFallGravityMult;
    public float maxAscendantSpeed;

    //[Header("Both Jumps")]
    public float jumpCutGravityMult;
    [Range(0f, 1f)] public float jumpHangGravityMult;
    public float jumpHangTimeThreshold;
    public float jumpHangAccelerationMult;
    public float jumpHangMaxSpeedMult;

    //[Header("Wall Jump")]
	public Vector2 wallJumpForce;
	[Range(0f, 1f)] public float wallJumpRunLerp;
	public bool doTurnOnWallJump;

    //[Header("Slide")]
    public float slideSpeed;
    public float slideAccel;
    
    //[Header("Animation State")]
    public int currentAnimationState;
    public readonly int Idle = Animator.StringToHash("Idle");
    public readonly int Run = Animator.StringToHash("Run");
    public readonly int Airborne = Animator.StringToHash("Airborne");
    public readonly int Jump = Animator.StringToHash("Jump");
    public readonly int Fall = Animator.StringToHash("Fall");
    public readonly int Roll = Animator.StringToHash("Roll");
    public readonly int Slide = Animator.StringToHash("Slide");
    public readonly int Hurt = Animator.StringToHash("Hurt");
}
