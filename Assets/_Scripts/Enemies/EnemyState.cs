using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState : State {
    protected Enemy enemy;
    protected EnemyData enemyData;

    // public bool isGrounded { get; protected set; }
    public bool isOnSolidGround { get; protected set; }
    public bool isOnSlope { get; protected set; }
    public bool isOnPlatform { get; protected set; }
    public bool isIgnoringPlatforms { get; protected set; }
    public bool isAirborne { get; protected set; }
    public bool isIdle { get; protected set; }
    public bool isMoving { get; protected set; }
    public bool isRunning { get; protected set; }
    public bool isRunningAtMaxSpeed { get; protected set; }
    public bool isChangingDirections { get; protected set; }
    public bool isJumping { get; protected set; }
    public bool isAscending { get; protected set; }
    public bool isFalling { get; protected set; }
    public bool isTouchingCeiling { get; protected set; }
    public bool isTouchingWall { get; protected set; }
    public bool isTouchingBackWall { get; protected set; }
    public bool hasTouchedWall { get; protected set; }
    public bool hasTouchedWallBack { get; protected set; }
    public bool isTouchingLedge { get; protected set; }
    public bool isTouchingLedgeWithFoot { get; protected set; }
    public bool isDamaged { get; protected set; }
    public bool isDead { get; protected set; }
    public bool isInvulnerable { get; protected set; }
    public bool isAbilityDone { get; protected set; }

    public bool isPatroling { get; protected set; }
    public bool isChasing { get; protected set; }

    public EnemyState(Enemy enemy, StateMachine stateMachine, EnemyData enemyData, string animBoolName) {
        Init(enemy, stateMachine, enemyData, animBoolName);
    }

    public void Init(Enemy enemy, StateMachine sM, EnemyData enemyData, string animBoolName) {
        entity = enemy;
        stateMachine = sM;
        this.enemyData = enemyData;
        this.animBoolName = animBoolName;

        this.enemy = enemy;
    }

    public override void Enter() {
        base.Enter();
        CheckRaycasts();
        CheckVerticalMovement();
        UpdateEnemyStates();
    }

    public override void Exit() {
        base.Exit();
        CheckRaycasts();
        CheckVerticalMovement();
        UpdateEnemyStates();
    }

    public override void LogicUpdate() {
        base.LogicUpdate();
        CheckVerticalMovement();
        UpdateEnemyStates();
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();
        CheckRaycasts();
    }

    public void CheckVerticalMovement() {
        isAscending = enemy.CheckAscending() && !enemy.isGrounded;
        isFalling = enemy.CheckFalling() && !enemy.isGrounded;
    }

    public void CheckRaycasts() {
        // isGrounded = enemy.CheckGround(enemyData.groundLayer);
        enemy.isGrounded = enemy.CheckGround(enemyData.groundLayer);
        isOnSolidGround = enemy.CheckGround(enemyData.solidsLayer);
        isAirborne = !enemy.CheckGround(enemyData.groundLayer);
        isOnPlatform = enemy.CheckGround(enemyData.platformLayer);
        isTouchingWall = enemy.CheckWall();
        isTouchingBackWall = enemy.CheckBackWall();
        isTouchingLedge = enemy.CheckLedge();
        isTouchingLedgeWithFoot = enemy.CheckLedgeFoot();
    }

    public void UpdateEnemyStates() {
        enemyData.currentVelocity = enemy.CurrentVelocity;
        enemyData.facingDirection = enemy.FacingDirection == 1 ? Direction.Right : Direction.Left;
        enemyData.currentGravityScale = enemy.Rb.gravityScale;
        enemyData.currentLayer = LayerMask.LayerToName(enemy.gameObject.layer);

        enemyData.isGrounded = enemy.isGrounded;
        enemyData.isOnSolidGround = isOnSolidGround;
        enemyData.isOnPlatform = isOnPlatform;
        enemyData.isIgnoringPlatforms = isIgnoringPlatforms;
        enemyData.isOnSlope = isOnSlope;
        enemyData.isIdle = isIdle;
        enemyData.isMoving = isMoving;
        enemyData.isRunning = isRunning;
        enemyData.isRunningAtMaxSpeed = isRunningAtMaxSpeed;
        enemyData.isChangingDirections = isChangingDirections;
        enemyData.isAirborne = isAirborne;
        enemyData.isJumping = isJumping;
        enemyData.isAscending = isAscending;
        enemyData.isFalling = isFalling;
        enemyData.isFastFalling = isFalling;
        enemyData.isTouchingCeiling = isTouchingCeiling;
        enemyData.isTouchingWall = isTouchingWall;
        enemyData.isTouchingBackWall = isTouchingBackWall;
        enemyData.hasTouchedWall = hasTouchedWall;
        enemyData.hasTouchedWallBack = hasTouchedWallBack;
        enemyData.isTouchingLedge = isTouchingLedge;
        enemyData.isTouchingLedgeWithFoot = isTouchingLedgeWithFoot;
        enemyData.isDamaged = isDamaged;
        enemyData.isDead = isDead;
        enemyData.isInvulnerable = isInvulnerable;
        enemyData.isAnimationFinished = isAnimationFinished;
        enemyData.isExitingState = isExitingState;
        enemyData.isAbilityDone = isAbilityDone;
        enemyData.isPatroling = isPatroling;
        enemyData.isChasing = isChasing;
    }
}
