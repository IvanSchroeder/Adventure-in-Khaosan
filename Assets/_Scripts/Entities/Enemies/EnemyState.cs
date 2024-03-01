using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState : State {
    protected Enemy enemy;
    protected EnemyData enemyData;

    // public bool isGrounded { get; protected set; }
    // public bool isOnSolidGround { get; protected set; }
    // public bool isOnSlope { get; protected set; }
    // public bool isOnPlatform { get; protected set; }
    // public bool isIgnoringPlatforms { get; protected set; }
    // public bool isAirborne { get; protected set; }
    // public bool isIdle { get; protected set; }
    // public bool isMoving { get; protected set; }
    // public bool isRunning { get; protected set; }
    // public bool isRunningAtMaxSpeed { get; protected set; }
    // public bool isChangingDirections { get; protected set; }
    // public bool isJumping { get; protected set; }
    // public bool isAscending { get; protected set; }
    // public bool isFalling { get; protected set; }
    // public bool isTouchingCeiling { get; protected set; }
    // public bool isTouchingWall { get; protected set; }
    // public bool isTouchingBackWall { get; protected set; }
    // public bool hasTouchedWall { get; protected set; }
    // public bool hasTouchedWallBack { get; protected set; }
    // public bool isTouchingLedge { get; protected set; }
    // public bool isTouchingLedgeWithFoot { get; protected set; }
    // public bool isDamaged { get; protected set; }
    // public bool isDead { get; protected set; }
    // public bool isInvulnerable { get; protected set; }
    // public bool isAbilityDone { get; protected set; }

    // public bool isPatroling { get; protected set; }
    // public bool isChasing { get; protected set; }

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
        enemy.isAscending = enemy.CheckAscending() && !enemy.isGrounded;
        enemy.isFalling = enemy.CheckFalling() && !enemy.isGrounded;
    }

    public void CheckRaycasts() {
        enemy.isGrounded = enemy.CheckGround(enemyData.groundLayer);
        enemy.isOnSolidGround = enemy.CheckGround(enemyData.solidsLayer);
        enemy.isAirborne = !enemy.CheckGround(enemyData.groundLayer);
        enemy.isOnPlatform = enemy.CheckGround(enemyData.platformLayer);
        enemy.isTouchingWall = enemy.CheckWall();
        enemy.isTouchingBackWall = enemy.CheckBackWall();
        enemy.isTouchingLedge = enemy.CheckLedge();
        enemy.isTouchingLedgeWithFoot = enemy.CheckLedgeFoot();
    }

    public void UpdateEnemyStates() {
        enemyData.currentVelocity = enemy.CurrentVelocity;
        enemyData.facingDirection = enemy.FacingDirection == 1 ? Direction.Right : Direction.Left;
        enemyData.currentGravityScale = enemy.Rb.gravityScale;
        enemyData.currentLayer = LayerMask.LayerToName(enemy.gameObject.layer);

        enemyData.isGrounded = enemy.isGrounded;
        enemyData.isOnSolidGround = enemy.isOnSolidGround;
        enemyData.isOnPlatform = enemy.isOnPlatform;
        enemyData.isIgnoringPlatforms = enemy.isIgnoringPlatforms;
        enemyData.isOnSlope = enemy.isOnSlope;
        enemyData.isIdle = enemy.isIdle;
        enemyData.isMoving = enemy.isMoving;
        enemyData.isRunning = enemy.isRunning;
        enemyData.isRunningAtMaxSpeed = enemy.isRunningAtMaxSpeed;
        enemyData.isChangingDirections = enemy.isChangingDirections;
        enemyData.isAirborne = enemy.isAirborne;
        enemyData.isJumping = enemy.isJumping;
        enemyData.isAscending = enemy.isAscending;
        enemyData.isFalling = enemy.isFalling;
        enemyData.isFastFalling = enemy.isFalling;
        enemyData.isTouchingCeiling = enemy.isTouchingCeiling;
        enemyData.isTouchingWall = enemy.isTouchingWall;
        enemyData.isTouchingBackWall = enemy.isTouchingBackWall;
        enemyData.hasTouchedWall = enemy.hasTouchedWall;
        enemyData.hasTouchedWallBack = enemy.hasTouchedWallBack;
        enemyData.isTouchingLedge = enemy.isTouchingLedge;
        enemyData.isTouchingLedgeWithFoot = enemy.isTouchingLedgeWithFoot;
        enemyData.isDamaged = enemy.isDamaged;
        enemyData.isDead = enemy.isDead;
        enemyData.isInvulnerable = enemy.isInvulnerable;
        enemyData.isAnimationFinished = isAnimationFinished;
        enemyData.isExitingState = isExitingState;
        enemyData.isAbilityDone = enemy.isAbilityDone;
        enemyData.isPatroling = enemy.isPatroling;
        enemyData.isChasing = enemy.isChasing;
    }
}
