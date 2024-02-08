using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State {
    protected Entity entity;
    protected StateMachine stateMachine;
    protected string animBoolName;

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

    public bool isAnimationFinished { get; protected set; }
    public bool isExitingState { get; protected set; }
    public float startTime { get; protected set; }

    public virtual void Enter() {
        isAnimationFinished = false;
        isExitingState = false;
        startTime = Time.time;
        entity.Anim.SetBool(animBoolName, true);
    }

    public virtual void Exit() {
        entity.Anim.SetBool(animBoolName, false);
        isExitingState = true;
    }

    public virtual void LogicUpdate() { if (isExitingState) return; }

    public virtual void PhysicsUpdate() {}

    public virtual void AnimationTrigger() {}

    public virtual void AnimationFinishTrigger() => isAnimationFinished = true;
}
