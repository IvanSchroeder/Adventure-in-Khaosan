using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using UnityEngine.Tilemaps;
using System;

[Serializable]
public class PlayerState {
    protected static Player player;
    protected static PlayerStateMachine stateMachine;
    protected static PlayerData playerData;

    protected static bool isGrounded;
    protected static bool isOnSolidGround;
    protected static bool isOnSlope;
    protected static bool isOnPlatform;
    protected static bool isIgnoringPlatforms;
    protected static bool isAirborne;
    protected static bool isIdle;
    protected static bool isMoving;
    protected static bool isRunning;
    protected static bool isRunningAtMaxSpeed;
    protected static bool isSprinting;
    protected static bool isSprintingAtMaxSpeed;
    protected static bool isChangingDirections;
    protected static bool isCrouching;
    protected static bool isGroundSliding;
    protected static bool stopSlide;
    protected static bool isJumping;
    protected static bool isAscending;
    protected static bool isFalling;
    protected static bool isFastFalling;
    protected static bool isTouchingCeiling;
    protected static bool isTouchingWall;
    protected static bool isTouchingBackWall;
    protected static bool hasTouchedWall;
    protected static bool hasTouchedWallBack;
    protected static bool isTouchingLedge;
    protected static bool isTouchingLedgeWithFoot;
    protected static bool isWallSliding;
    protected static bool isWallGrabing;
    protected static bool isWallClimbing;
    protected static bool isWallJumping;
    protected static bool isHanging;
    protected static bool isClimbing;
    protected static bool isDamaged;
    protected static bool isDead;
    protected static bool isDeadOnGround;
    protected static bool isInvulnerable;
    protected static bool isAnimationFinished;
    protected static bool isExitingState;
    protected static bool isAbilityDone;
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
    protected static float startTime;
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

    private string animBoolName;

    public PlayerState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) {
        Init(player, stateMachine, playerData, animBoolName);
        isDead = false;
        isDeadOnGround = false;
    }

    public void Init(Player pl, PlayerStateMachine sM, PlayerData pD, string animBoolName) {
        player = pl;
        stateMachine = sM;
        playerData = pD;
        this.animBoolName = animBoolName;
    }

    public virtual void Enter() {
        CheckInputs();
        CheckHorizontalMovement();
        CheckVerticalMovement();
        UpdatePlayerStates();
        UpdateAnimator();
        player.Anim.SetBool(animBoolName, true);
        startTime = Time.time;
        isAnimationFinished = false;
        isExitingState = false;
    }

    public virtual void Exit() {
        CheckInputs();
        CheckRaycasts();
        CheckHorizontalMovement();
        CheckVerticalMovement();
        UpdatePlayerStates();
        UpdateAnimator();
        player.Anim.SetBool(animBoolName, false);
        isExitingState = true;
    }

    public virtual void LogicUpdate() {
        CheckInputs();
        CheckHorizontalMovement();
        CheckVerticalMovement();
        UpdatePlayerStates();
        UpdateAnimator();

        if ((unplatformInput && ((isOnPlatform && jumpInputHold) || isWallSliding || isFalling)) || isWallClimbing || isWallGrabing || isDead) {
            player.InputHandler.UseJumpInput();
            isIgnoringPlatforms = true;
            Physics2D.IgnoreLayerCollision(player.gameObject.layer, LayerMask.NameToLayer("Platform"), true);
            player.gameObject.layer = LayerMask.NameToLayer("IgnorePlatforms");
        }
        else if ((!unplatformInput && !isAirborne && !isWallSliding) || isOnSolidGround || !isWallClimbing || !isWallGrabing || !isDead) {
            isIgnoringPlatforms = false;
            player.gameObject.layer = LayerMask.NameToLayer("Player");
            Physics2D.IgnoreLayerCollision(player.gameObject.layer, LayerMask.NameToLayer("Platform"), false);
        }
    }

    public virtual void PhysicsUpdate() {
        CheckRaycasts();
    }

    public virtual void AnimationTrigger() { }

    public virtual void AnimationFinishTrigger() => isAnimationFinished = true;

    public void CheckHorizontalMovement() {
        if (!isGroundSliding && cumulatedGroundSlideCooldown < playerData.groundSlideDelay) cumulatedGroundSlideCooldown += Time.deltaTime;
        
        if (!groundSlideTime && cumulatedGroundSlideCooldown >= playerData.groundSlideDelay) {
            groundSlideTime = true;
            cumulatedGroundSlideCooldown = playerData.groundSlideDelay;
        }

        isChangingDirections = player.CheckChangingDirections();
    }

    public void CheckVerticalMovement() {
        isAscending = player.CheckAscending() && !isGrounded;
        isFalling = player.CheckFalling() && !isGrounded;
    }

    public void CheckRaycasts() {
        if (!isIgnoringPlatforms) isGrounded = player.CheckGround(playerData.groundLayer);
        else if (isIgnoringPlatforms) isGrounded = player.CheckGround(playerData.solidsLayer);
        isOnSolidGround = player.CheckGround(playerData.solidsLayer);
        isAirborne = !player.CheckGround(playerData.groundLayer);
        if (!isIgnoringPlatforms) isOnPlatform = player.CheckGround(playerData.platformLayer);
        isTouchingCeiling = player.CheckCeiling();
        isTouchingWall = player.CheckWall();
        isTouchingBackWall = player.CheckBackWall();
        isTouchingLedge = player.CheckLedge();
        isTouchingLedgeWithFoot = player.CheckLedgeFoot();

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
        player.Anim.SetBool("changingDirections", isChangingDirections);
        player.Anim.SetBool("running", isRunning && player.CurrentVelocity.x.AbsoluteValue() > 0f);
        player.Anim.SetBool("sprinting", isSprintingAtMaxSpeed && player.CurrentVelocity.x.AbsoluteValue() > 0f);
    }

    public void UpdatePlayerStates() {
        // playerData.xInput = xInput;
        // playerData.lastXInput = lastXInput;
        // playerData.yInput = yInput;
        // playerData.jumpInput = jumpInput;
        // playerData.jumpInputStop = jumpInputStop;
        // playerData.jumpInputHold = jumpInputHold;
        // playerData.attackInput = attackInput;
        // playerData.attackInputHold = attackInputHold;
        // playerData.attackInputStop = attackInputStop;
        // playerData.grabInput = grabInput;
        // playerData.crouchInput = crouchInput;
        // playerData.crouchInputHold = crouchInputHold;
        // playerData.crouchInputStop = crouchInputStop;
        // playerData.interactInput = interactInput;
        // playerData.interactInputHold = interactInputHold;
        // playerData.interactInputStop = interactInputStop;
        // playerData.unplatformInput = unplatformInput;

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

        playerData.isGrounded = isGrounded;
        playerData.isOnSolidGround = isOnSolidGround;
        playerData.isOnPlatform = isOnPlatform;
        playerData.isIgnoringPlatforms = isIgnoringPlatforms;
        playerData.isOnSlope = isOnSlope;
        playerData.isIdle = isIdle;
        playerData.isMoving = isMoving;
        playerData.isRunning = isRunning;
        playerData.isRunningAtMaxSpeed = isRunningAtMaxSpeed;
        playerData.isSprinting = isSprinting;
        playerData.isSprintingAtMaxSpeed = isSprintingAtMaxSpeed;
        playerData.isChangingDirections = isChangingDirections;
        playerData.isCrouching = isCrouching;
        playerData.isGroundSliding = isGroundSliding;
        playerData.stopSlide = stopSlide;
        playerData.isAirborne = isAirborne;
        playerData.isJumping = isJumping;
        playerData.isAscending = isAscending;
        playerData.isFalling = isFalling;
        playerData.isFastFalling = isFalling;
        playerData.isTouchingCeiling = isTouchingCeiling;
        playerData.isTouchingWall = isTouchingWall;
        playerData.isTouchingBackWall = isTouchingBackWall;
        playerData.hasTouchedWall = hasTouchedWall;
        playerData.hasTouchedWallBack = hasTouchedWallBack;
        playerData.isTouchingLedge = isTouchingLedge;
        // playerData.isTouchingLedgeWithFoot = isTouchingLedgeWithFoot;
        playerData.isWallSliding = isWallSliding;
        playerData.isWallGrabing = isWallGrabing;
        playerData.isWallClimbing = isWallClimbing;
        playerData.isWallJumping = isWallJumping;
        playerData.isHanging = isHanging;
        playerData.isClimbing = isClimbing;
        playerData.isDamaged = isDamaged;
        playerData.isDead = isDead;
        playerData.isDeadOnGround = isDeadOnGround;
        playerData.isInvulnerable = isInvulnerable;
        playerData.isAnimationFinished = isAnimationFinished;
        playerData.isExitingState = isExitingState;
        playerData.isAbilityDone = isAbilityDone;
        playerData.hasCoyoteTime = coyoteTime;
        playerData.hasWallJumpCoyoteTime = wallJumpCoyoteTime;
        playerData.hasGroundSlideTime = groundSlideTime;
    }

    protected float slopeDownAngle;
    protected float slopeDownAngleOld;
    protected float slopeSideAngle;
    protected Vector2 slopeNormalPerpendicular;
    protected Vector2 slopeHitPosition;

    private void SlopeCheck() {
        Vector2 checkPos = player.GroundPoint.position;
        // RaycastHit2D slopeHitDown = Physics2D.Raycast(player.groundCheck.position.ToVector2() /*+ (Vector2.right * player.FacingDirection * playerData.groundCheckOffset)*/, Vector2.down, playerData.slopeCheckDistance, playerData.solidsLayer);
        // if (slopeHitDown) {
        //     Vector2 temp = player.groundCheck.position;
        // }
        SlopeCheckVertical(checkPos);
        // SlopeCheckHorizontal(checkPos);
    }

    private void SlopeCheckHorizontal(Vector2 checkPos) {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, Vector2.right, playerData.slopeCheckDistance, playerData.solidsLayer);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -Vector2.right, playerData.slopeCheckDistance, playerData.solidsLayer);

        if (slopeHitFront) {
            isOnSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
        }
        else if (slopeHitBack) {
            isOnSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        }
        else {
            slopeSideAngle = 0.0f;
            isOnSlope = false;
        }
    }

    private void SlopeCheckVertical(Vector2 checkPos) {
        RaycastHit2D slopeHitDown = Physics2D.Raycast(checkPos /*+ (Vector2.right * player.FacingDirection * playerData.groundCheckOffset)*/, Vector2.down, playerData.slopeCheckDistance, playerData.solidsLayer);

        Debug.DrawRay(checkPos, Vector2.down * playerData.slopeCheckDistance, Color.green);

        if (slopeHitDown) {
            slopeNormalPerpendicular = Vector2.Perpendicular(slopeHitDown.normal).normalized;

            slopeHitPosition = slopeHitDown.point;
            slopeDownAngle = Vector2.Angle(slopeHitDown.normal, Vector2.up);

            if (slopeDownAngle != 0f) {
                isOnSlope = true;
                // player.SpriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, slopeDownAngle * slopeNormalPerpendicular.x);
            }
            else {
                isOnSlope = false;
                // player.SpriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }

            Debug.DrawRay(slopeHitDown.point, slopeHitDown.normal, Color.red);
        }
        else isOnSlope = false;

        // if (isOnSlope && xInput == 0) {
        //     player.Rb.sharedMaterial = playerData.fullFrictionMaterial;
        // }
        // else {
        //     player.Rb.sharedMaterial = playerData.noFrictionMaterial;
        // } 
    }

    public void BounceOffGround() {
        isOnSolidGround = false;
        bounceOffGround = true;
    }

    public void BounceOffWall() {
        player.CheckFacingDirection(-player.FacingDirection);

        isTouchingBackWall = false;
        isTouchingWall = false;
        bounceOffWall = true;
    }

    public void BounceOffCeiling() {
        isTouchingCeiling = false;
        bounceOffCeiling = true;
    }
}
