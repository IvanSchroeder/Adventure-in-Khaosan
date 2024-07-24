using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State {
    protected Entity entity;
    protected StateMachine stateMachine;
    protected string animBoolName;

    public bool isAnimationFinished { get; set; }
    public bool isExitingState { get; set; }
    public float startTime { get; protected set; }

    public virtual void Enter() {
        entity.isAnimationFinished = false;
        entity.isExitingState = false;
        startTime = Time.time;
        entity.Anim.SetBool(animBoolName, true);
    }

    public virtual void Exit() {
        entity.Anim.SetBool(animBoolName, false);
        entity.isExitingState = true;
    }

    public virtual void LogicUpdate() { if (entity.isExitingState) return; }

    public virtual void PhysicsUpdate() {}

    public virtual void AnimationTrigger() {}

    public virtual void AnimationFinishTrigger() => entity.isAnimationFinished = true;
}
