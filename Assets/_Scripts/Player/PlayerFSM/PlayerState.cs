using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using UnityEngine.Tilemaps;

public class PlayerState {
    protected Player player;
    protected PlayerStateMachine stateMachine;
    protected PlayerData playerData;

    protected bool isGrounded;
    protected bool isOnSolidGround;
    protected bool isOnPlatform;
    protected bool isIgnoringPlatforms;
    protected bool isOnSlope;
    protected bool isMoving;
    protected bool isRunning;
    protected bool isRunningAtMaxSpeed;
    protected bool isSprinting;
    protected bool isSprintingAtMaxSpeed;
    protected bool isChangingDirections;
    protected bool isCrouching;
    protected bool isGroundSliding;
    protected bool isAirborne;
    protected bool isJumping;
    protected bool isAscending;
    protected bool isFalling;
    protected bool isFastFalling;
    protected bool isTouchingCeiling;
    protected bool isTouchingWall;
    protected bool isTouchingBackWall;
    protected bool hasTouchedWall;
    protected bool hasTouchedWallBack;
    protected bool isTouchingLedge;
    protected bool isWallSliding;
    protected bool isWallGrabing;
    protected bool isWallClimbing;
    protected bool isHanging;
    protected bool isClimbing;
    protected bool isDamaged;
    protected bool isDead;
    protected bool isDeadOnGround;
    protected bool isInvulnerable;
    protected bool isAnimationFinished;
    protected bool isExitingState;
    protected bool isAbilityDone;
    protected bool coyoteTime;
    protected bool wallJumpCoyoteTime;

    protected int xInput;
    protected int lastXInput;
    protected int yInput;
    protected bool jumpInput;
    protected bool jumpInputStop;
    protected bool jumpInputHold;
    protected bool grabInput;
    protected bool attackInput;
    protected bool attackInputStop;
    protected bool attackInputHold;

    protected int amountOfJumpsLeft;
    protected float startTimeMaxRunSpeed;
    protected float startTime;
    protected float startWallJumpCoyoteTime;
    protected float cumulatedWallJumpCoyoteTime;
    protected float cumulatedKnockbackTime;
    protected float cumulatedDeathTime;

    protected Vector2 lastContactPoint;
    protected int lastKnockbackFacingDirection;
    protected bool bounceOffGround;
    protected bool bounceOffWall;
    protected bool bounceOffCeiling;
    protected bool hasBouncedOffGround;
    protected bool hasBouncedOffWall;
    protected bool hasBouncedOffCeiling;

    private string animBoolName;

    public PlayerState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) {
        this.player = player;
        this.stateMachine = stateMachine;
        this.playerData = playerData;
        this.animBoolName = animBoolName;
    }

    public virtual void Enter() {
        DoChecks();
        player.Anim.SetBool(animBoolName, true);
        startTime = Time.time;
        isAnimationFinished = false;
        isExitingState = false;
    }

    public virtual void Exit() {
        UpdateAnimator();
        UpdatePlayerStates();
        CheckHorizontalMovement();
        CheckVerticalMovement();
        player.Anim.SetBool(animBoolName, false);
        isExitingState = true;
    }

    public virtual void LogicUpdate() {
        CheckInputs();
        UpdateAnimator();
        UpdatePlayerStates();
        CheckHorizontalMovement();
        CheckVerticalMovement();

        if ((yInput == -1 && ((isOnPlatform && jumpInputHold) || isWallSliding || isFalling || isAirborne)) || isWallClimbing || isWallGrabing || isDead) {
            player.InputHandler.UseJumpInput();
            isIgnoringPlatforms = true;
            Physics2D.IgnoreLayerCollision(player.gameObject.layer, LayerMask.NameToLayer("Platform"), true);
            player.gameObject.layer = LayerMask.NameToLayer("IgnorePlatforms");
        }
        else if ((yInput != -1 && !isAirborne && !isWallSliding) || isOnSolidGround || !isWallClimbing || !isWallGrabing) {
            isIgnoringPlatforms = false;
            player.gameObject.layer = LayerMask.NameToLayer("Player");
            Physics2D.IgnoreLayerCollision(player.gameObject.layer, LayerMask.NameToLayer("Platform"), false);
        }
    }

    public virtual void PhysicsUpdate() {
        CheckRaycasts();
    }

    public virtual void DoChecks() {
        CheckHorizontalMovement();
        CheckVerticalMovement();
        CheckRaycasts();
    }

    public virtual void AnimationTrigger() { }

    public virtual void AnimationFinishTrigger() => isAnimationFinished = true;

    public void CheckHorizontalMovement() {
        isChangingDirections = player.CheckChangingDirections();
    }

    public void CheckVerticalMovement() {
        isAscending = player.CheckAscending();
        isFalling = player.CheckFalling();
    }

    public void CheckRaycasts() {
        if (!isIgnoringPlatforms) isGrounded = player.CheckGround(playerData.groundLayer);
        else if (isIgnoringPlatforms) isGrounded = player.CheckGround(playerData.solidsLayer);
        isOnSolidGround = player.CheckGround(playerData.solidsLayer);
        if (!isIgnoringPlatforms) isOnPlatform = player.CheckGround(playerData.platformLayer);
        isTouchingCeiling = player.CheckCeiling();
        isTouchingWall = player.CheckWall();
        isTouchingBackWall = player.CheckBackWall();
        if(!isAbilityDone) isTouchingLedge = player.CheckLedge();

        // if (isOnSolidGround) SlopeCheck();
    }

    public void CheckInputs() {
        xInput = player.InputHandler.NormInputX;
        lastXInput = player.InputHandler.LastXInput;
        yInput = player.InputHandler.NormInputY;
        jumpInput = player.InputHandler.JumpInput;
        jumpInputStop = player.InputHandler.JumpInputStop;
        jumpInputHold = player.InputHandler.JumpInputHold;
        attackInput = player.InputHandler.AttackInput;
        attackInputStop = player.InputHandler.AttackInputStop;
        attackInputHold = player.InputHandler.AttackInputHold;
        if (!playerData.autoWallGrab) grabInput = player.InputHandler.GrabInput;
    }

    public void UpdateAnimator() {
        player.Anim.SetFloat("xVelocity", player.CurrentVelocity.x);
        player.Anim.SetFloat("xVelocityNormalized", player.CurrentVelocity.normalized.x);
        player.Anim.SetFloat("yVelocityNormalized", player.CurrentVelocity.normalized.y);
        player.Anim.SetFloat("xInput", xInput);
        player.Anim.SetBool("changingDirections", isChangingDirections);
        player.Anim.SetBool("running", isRunning);
        player.Anim.SetBool("sprinting", isSprintingAtMaxSpeed);
    }

    public void UpdatePlayerStates() {
        playerData.currentVelocity = player.CurrentVelocity;
        playerData.facingDirection = player.FacingDirection == 1 ? Direction.Right : Direction.Left;
        playerData.currentGravityScale = player.Rb.gravityScale;
        playerData.currentLayer = LayerMask.LayerToName(player.gameObject.layer);
        playerData.cumulatedKnockbackTime = cumulatedKnockbackTime;
        playerData.slopeDownAngle = slopeDownAngle;
        playerData.slopeSideAngle = slopeSideAngle;
        playerData.cumulatedWallJumpCoyoteTime = cumulatedWallJumpCoyoteTime;
        playerData.amountOfJumpsLeft = amountOfJumpsLeft;

        playerData.isGrounded = isGrounded;
        playerData.isOnSolidGround = isOnSolidGround;
        playerData.isOnPlatform = isOnPlatform;
        playerData.isIgnoringPlatforms = isIgnoringPlatforms;
        playerData.isOnSlope = isOnSlope;
        playerData.isMoving = isMoving;
        playerData.isRunning = isRunning;
        playerData.isRunningAtMaxSpeed = isRunningAtMaxSpeed;
        playerData.isSprinting = isSprinting;
        playerData.isSprintingAtMaxSpeed = isSprintingAtMaxSpeed;
        playerData.isChangingDirections = isChangingDirections;
        playerData.isCrouching = isCrouching;
        playerData.isGroundSliding = isGroundSliding;
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
        playerData.isWallSliding = isWallSliding;
        playerData.isWallGrabing = isWallGrabing;
        playerData.isWallClimbing = isWallClimbing;
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

        playerData.xInput = xInput;
        playerData.lastXInput = lastXInput;
        playerData.yInput = yInput;
        playerData.jumpInput = jumpInput;
        playerData.jumpInputStop = jumpInputStop;
        playerData.jumpInputHold = jumpInputHold;
        playerData.grabInput = grabInput;

        playerData.wallJumpDirectionOffAngle = playerData.wallJumpAngle.AngleFloatToVector2();
        playerData.wallHopDirectionOffAngle = playerData.wallHopAngle.AngleFloatToVector2();
    }

    protected float slopeDownAngle;
    protected float slopeDownAngleOld;
    protected float slopeSideAngle;
    protected Vector2 slopeNormalPerpendicular;
    protected Vector2 slopeHitPosition;

    private void SlopeCheck() {
        Vector2 checkPos = player.GroundCheck.position;
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
