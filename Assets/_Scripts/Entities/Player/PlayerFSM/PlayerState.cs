using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using UnityEngine.Tilemaps;
using System;

[Serializable]
public class PlayerState : State {
    protected static Player player;
    protected static PlayerData playerData;

    // public static bool isGrounded { get; protected set; }
    // public static bool isOnSolidGround { get; protected set; }
    // public static bool isOnSlope { get; protected set; }
    // public static bool isOnPlatform { get; protected set; }
    // public static bool isIgnoringPlatforms { get; protected set; }
    // public static bool isAirborne { get; protected set; }
    // public static bool isIdle { get; protected set; }
    // public static bool isMoving { get; protected set; }
    // public static bool isRunning { get; protected set; }
    // public static bool isRunningAtMaxSpeed { get; protected set; }
    // public static bool isChangingDirections { get; protected set; }
    // public static bool isJumping { get; protected set; }
    // public static bool isAscending { get; protected set; }
    // public static bool isFalling { get; protected set; }
    // public static bool isTouchingCeiling { get; protected set; }
    // public static bool isTouchingWall { get; protected set; }
    // public static bool isTouchingBackWall { get; protected set; }
    // public static bool hasTouchedWall { get; protected set; }
    // public static bool hasTouchedWallBack { get; protected set; }
    // public static bool isTouchingLedge { get; protected set; }
    // public static bool isTouchingLedgeWithFoot { get; protected set; }
    // public static bool isDamaged { get; protected set; }
    // public static bool isDead { get; protected set; }
    // public static bool isInvulnerable { get; protected set; }
    // public static bool isAbilityDone { get; protected set; }

    protected static bool isSprinting;
    protected static bool isSprintingAtMaxSpeed;
    protected static bool isCrouching;
    protected static bool isGroundSliding;
    protected static bool stopSlide;
    protected static bool isFastFalling;
    protected static bool isWallSliding;
    protected static bool isWallGrabing;
    protected static bool isWallClimbing;
    protected static bool isWallJumping;
    protected static bool isHanging;
    protected static bool isClimbing;
    protected static bool isDeadOnGround;
    protected static bool coyoteTime;
    protected static bool wallJumpCoyoteTime;
    protected static bool groundSlideTime;
    protected static bool hasSpace;

    protected static int xInput;
    protected static int lastXInput;
    protected static int yInput;
    protected static bool interactInput;
    protected static bool interactInputHold;
    protected static bool interactInputStop;
    protected static bool unplatformInput;
    protected static bool crouchInput;
    protected static bool crouchInputHold;
    protected static bool crouchInputStop;
    protected static bool jumpInput;
    protected static bool jumpInputStop;
    protected static bool jumpInputHold;
    protected static bool attackInput;
    protected static bool attackInputHold;
    protected static bool attackInputStop;
    protected static bool grabInput;

    protected static int amountOfJumpsLeft;
    protected static float cumulatedJumpCoyoteTime;
    protected static float cumulatedWallJumpCoyoteTime;
    protected static float cumulatedKnockbackTime;
    protected static float cumulatedGroundSlideCooldown;
    protected static float cumulatedGroundSlideTime;
    protected static float cumulatedDeathTime;

    protected static Vector2 lastContactPoint;
    protected static int lastKnockbackFacingDirection;
    protected static bool bounceOffGround;
    protected static bool bounceOffWall;
    protected static bool bounceOffCeiling;
    protected static bool hasBouncedOffGround;
    protected static bool hasBouncedOffWall;
    protected static bool hasBouncedOffCeiling;

    public PlayerState(Player player, StateMachine stateMachine, PlayerData playerData, string animBoolName) {
        Init(player, stateMachine, playerData, animBoolName);
        player.isDead = false;
        isDeadOnGround = false;
    }

    public void Init(Player pl, StateMachine sM, PlayerData pD, string animBoolName) {
        entity = pl;
        stateMachine = sM;
        playerData = pD;
        this.animBoolName = animBoolName;

        player = pl;
    }

    public override void Enter() {
        base.Enter();
        CheckInputs();
        CheckHorizontalMovement();
        CheckVerticalMovement();
        UpdatePlayerStates();
        UpdateAnimator();
    }

    public override void Exit() {
        base.Exit();
        CheckInputs();
        CheckRaycasts();
        CheckHorizontalMovement();
        CheckVerticalMovement();
        UpdatePlayerStates();
        UpdateAnimator();
    }

    public override void LogicUpdate() {
        CheckInputs();
        CheckHorizontalMovement();
        CheckVerticalMovement();
        UpdatePlayerStates();
        UpdateAnimator();

        if ((unplatformInput && ((player.isOnPlatform && jumpInputHold) || isWallSliding || player.isFalling)) || isWallClimbing || isWallGrabing || player.isDead) {
            player.InputHandler.UseJumpInput();
            player.isIgnoringPlatforms = true;
            Physics2D.IgnoreLayerCollision(player.gameObject.layer, LayerMask.NameToLayer("Platform"), true);
            player.gameObject.layer = LayerMask.NameToLayer("IgnorePlatforms");
        }
        else if ((!unplatformInput && !player.isAirborne && !isWallSliding) || player.isOnSolidGround || !isWallClimbing || !isWallGrabing || !player.isDead) {
            player.isIgnoringPlatforms = false;
            player.gameObject.layer = LayerMask.NameToLayer("Player");
            Physics2D.IgnoreLayerCollision(player.gameObject.layer, LayerMask.NameToLayer("Platform"), false);
        }
    }

    public override void PhysicsUpdate() {
        CheckRaycasts();
    }

    public override void AnimationTrigger() { }

    public override void AnimationFinishTrigger() => player.isAnimationFinished = true;

    public void CheckHorizontalMovement() {
        if (!isGroundSliding && cumulatedGroundSlideCooldown < playerData.groundSlideDelay) cumulatedGroundSlideCooldown += Time.deltaTime;
        
        if (!groundSlideTime && cumulatedGroundSlideCooldown >= playerData.groundSlideDelay) {
            groundSlideTime = true;
            cumulatedGroundSlideCooldown = playerData.groundSlideDelay;
        }

        player.isChangingDirections = player.CheckChangingDirections();
    }

    public void CheckVerticalMovement() {
        player.isAscending = player.CheckAscending() && !player.isGrounded;
        player.isFalling = player.CheckFalling() && !player.isGrounded;
    }

    public void CheckRaycasts() {
        if (!player.isIgnoringPlatforms) player.isGrounded = player.CheckGround(playerData.groundLayer) && (!player.isJumping || !player.isAscending);
        else if (player.isIgnoringPlatforms) player.isGrounded = player.CheckGround(playerData.solidsLayer) && (!player.isJumping || !player.isAscending);
        player.isOnSolidGround = player.CheckGround(playerData.solidsLayer) && (!player.isJumping || !player.isAscending);
        player.isAirborne = !player.CheckGround(playerData.groundLayer);
        if (!player.isIgnoringPlatforms) player.isOnPlatform = player.CheckGround(playerData.platformLayer);
        player.isTouchingCeiling = player.CheckCeiling();
        player.isTouchingWall = player.CheckWall();
        player.isTouchingBackWall = player.CheckBackWall();
        player.isTouchingLedge = player.CheckLedge();
        player.isTouchingLedgeWithFoot = player.CheckLedgeFoot();

        // if (isOnSolidGround) SlopeCheck();
    }

    public void CheckInputs() {
        xInput = player.InputHandler.NormInputX;
        lastXInput = player.InputHandler.LastXInput;
        yInput = player.InputHandler.NormInputY;

        jumpInput = player.InputHandler.JumpInput;
        jumpInputStop = player.InputHandler.JumpInputStop;
        jumpInputHold = player.InputHandler.JumpInputHold;

        unplatformInput = player.InputHandler.UnplatformInput;
        crouchInput = player.InputHandler.CrouchInput;
        crouchInputHold = player.InputHandler.CrouchInputHold;
        crouchInputStop = player.InputHandler.CrouchInputStop;

        interactInput = player.InputHandler.InteractInput;
        interactInputHold = player.InputHandler.InteractInputHold;
        interactInputStop = player.InputHandler.InteractInputStop;

        attackInput = player.InputHandler.AttackInput;
        attackInputStop = player.InputHandler.AttackInputStop;
        attackInputHold = player.InputHandler.AttackInputHold;

        if (!playerData.autoWallGrab) grabInput = player.InputHandler.GrabInput;
    }

    public void UpdateAnimator() {
        player.Anim.SetFloat("xVelocity", player.CurrentVelocity.x);
        player.Anim.SetFloat("xVelocityNormalized", player.CurrentVelocity.normalized.x);
        player.Anim.SetFloat("yVelocityNormalized", player.CurrentVelocity.normalized.y);
        player.Anim.SetBool("fastFall", isFastFalling);
        player.Anim.SetFloat("xInput", xInput);
        player.Anim.SetBool("changingDirections", player.isChangingDirections);
        player.Anim.SetBool("running", player.isRunning && player.CurrentVelocity.x.AbsoluteValue() > 0f);
        player.Anim.SetBool("sprinting", isSprintingAtMaxSpeed && player.CurrentVelocity.x.AbsoluteValue() > 0f);
    }

    public void UpdatePlayerStates() {
        playerData.currentVelocity = player.CurrentVelocity;
        playerData.facingDirection = player.FacingDirection == 1 ? Direction.Right : Direction.Left;
        playerData.currentGravityScale = player.Rb.gravityScale;
        playerData.currentLayer = LayerMask.LayerToName(player.gameObject.layer);
        playerData.amountOfJumpsLeft = amountOfJumpsLeft;
        // playerData.slopeDownAngle = slopeDownAngle;
        // playerData.slopeSideAngle = slopeSideAngle;
        // playerData.cumulatedKnockbackTime = cumulatedKnockbackTime;
        // playerData.cumulatedWallJumpCoyoteTime = cumulatedWallJumpCoyoteTime;
        // playerData.cumulatedGroundSlideTime = cumulatedGroundSlideTime;
        // playerData.cumulatedGroundSlideCooldown = cumulatedGroundSlideCooldown;

        playerData.maxRunSpeedThreshold = Mathf.Lerp(0f, playerData.runSpeed, playerData.maxRunSpeedThresholdMult);
        playerData.maxSprintSpeedThreshold = Mathf.Lerp(playerData.runSpeed, playerData.sprintSpeed, playerData.maxSprintSpeedThresholdMult);

        playerData.isGrounded = player.isGrounded;
        playerData.isOnSolidGround = player.isOnSolidGround;
        playerData.isOnPlatform = player.isOnPlatform;
        playerData.isIgnoringPlatforms = player.isIgnoringPlatforms;
        playerData.isOnSlope = player.isOnSlope;
        playerData.isIdle = player.isIdle;
        playerData.isMoving = player.isMoving;
        playerData.isRunning = player.isRunning;
        playerData.isRunningAtMaxSpeed = player.isRunningAtMaxSpeed;
        playerData.isSprinting = isSprinting;
        playerData.isSprintingAtMaxSpeed = isSprintingAtMaxSpeed;
        playerData.isChangingDirections = player.isChangingDirections;
        playerData.isCrouching = isCrouching;
        playerData.isGroundSliding = isGroundSliding;
        playerData.stopSlide = stopSlide;
        playerData.isAirborne = player.isAirborne;
        playerData.isJumping = player.isJumping;
        playerData.isAscending = player.isAscending;
        playerData.isFalling = player.isFalling;
        playerData.isFastFalling = player.isFalling;
        playerData.isTouchingCeiling = player.isTouchingCeiling;
        playerData.isTouchingWall = player.isTouchingWall;
        playerData.isTouchingBackWall = player.isTouchingBackWall;
        playerData.hasTouchedWall = player.hasTouchedWall;
        playerData.hasTouchedWallBack = player.hasTouchedWallBack;
        playerData.isTouchingLedge = player.isTouchingLedge;
        playerData.isWallSliding = isWallSliding;
        playerData.isWallGrabing = isWallGrabing;
        playerData.isWallClimbing = isWallClimbing;
        playerData.isWallJumping = isWallJumping;
        playerData.isHanging = isHanging;
        playerData.isClimbing = isClimbing;
        playerData.isDamaged = player.isDamaged;
        playerData.isDead = player.isDead;
        playerData.isDeadOnGround = isDeadOnGround;
        playerData.isInvulnerable = player.isInvulnerable;
        playerData.isAnimationFinished = isAnimationFinished;
        playerData.isExitingState = isExitingState;
        playerData.isAbilityDone = player.isAbilityDone;
        playerData.hasCoyoteTime = coyoteTime;
        playerData.hasWallJumpCoyoteTime = wallJumpCoyoteTime;
        playerData.hasGroundSlideTime = groundSlideTime;
    }

    protected float slopeDownAngle;
    protected float slopeDownAngleOld;
    protected float slopeSideAngle;
    protected Vector2 slopeNormalPerpendicular;
    protected Vector2 slopeHitPosition;

    // private void SlopeCheck() {
    //     Vector2 checkPos = player.GroundPoint.position;
    //     // RaycastHit2D slopeHitDown = Physics2D.Raycast(player.groundCheck.position.ToVector2() /*+ (Vector2.right * player.FacingDirection * playerData.groundCheckOffset)*/, Vector2.down, playerData.slopeCheckDistance, playerData.solidsLayer);
    //     // if (slopeHitDown) {
    //     //     Vector2 temp = player.groundCheck.position;
    //     // }
    //     SlopeCheckVertical(checkPos);
    //     // SlopeCheckHorizontal(checkPos);
    // }

    // private void SlopeCheckHorizontal(Vector2 checkPos) {
    //     RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, Vector2.right, playerData.slopeCheckDistance, playerData.solidsLayer);
    //     RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -Vector2.right, playerData.slopeCheckDistance, playerData.solidsLayer);

    //     if (slopeHitFront) {
    //         isOnSlope = true;
    //         slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
    //     }
    //     else if (slopeHitBack) {
    //         isOnSlope = true;
    //         slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
    //     }
    //     else {
    //         slopeSideAngle = 0.0f;
    //         isOnSlope = false;
    //     }
    // }

    // private void SlopeCheckVertical(Vector2 checkPos) {
    //     RaycastHit2D slopeHitDown = Physics2D.Raycast(checkPos /*+ (Vector2.right * player.FacingDirection * playerData.groundCheckOffset)*/, Vector2.down, playerData.slopeCheckDistance, playerData.solidsLayer);

    //     Debug.DrawRay(checkPos, Vector2.down * playerData.slopeCheckDistance, Color.green);

    //     if (slopeHitDown) {
    //         slopeNormalPerpendicular = Vector2.Perpendicular(slopeHitDown.normal).normalized;

    //         slopeHitPosition = slopeHitDown.point;
    //         slopeDownAngle = Vector2.Angle(slopeHitDown.normal, Vector2.up);

    //         if (slopeDownAngle != 0f) {
    //             isOnSlope = true;
    //             // player.SpriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, slopeDownAngle * slopeNormalPerpendicular.x);
    //         }
    //         else {
    //             isOnSlope = false;
    //             // player.SpriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    //         }

    //         Debug.DrawRay(slopeHitDown.point, slopeHitDown.normal, Color.red);
    //     }
    //     else isOnSlope = false;

    //     // if (isOnSlope && xInput == 0) {
    //     //     player.Rb.sharedMaterial = playerData.fullFrictionMaterial;
    //     // }
    //     // else {
    //     //     player.Rb.sharedMaterial = playerData.noFrictionMaterial;
    //     // } 
    // }

    public void BounceOffGround() {
        player.isOnSolidGround = false;
        bounceOffGround = true;
    }

    public void BounceOffWall() {
        player.CheckFacingDirection(-player.FacingDirection);

        player.isTouchingBackWall = false;
        player.isTouchingWall = false;
        bounceOffWall = true;
    }

    public void BounceOffCeiling() {
        player.isTouchingCeiling = false;
        bounceOffCeiling = true;
    }
}
