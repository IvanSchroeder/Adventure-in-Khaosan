using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public abstract class Entity : MonoBehaviour {
    public EntityData entityData;

    public EventHandler<OnEntityDamagedEventArgs> OnEntityDamaged;
    public EventHandler<OnEntityDamagedEventArgs> OnEntityDeath;

    [field: SerializeField] public StateMachine StateMachine { get; protected set; }
    [field: SerializeField] public Rigidbody2D Rb { get; protected set; }
    [field: SerializeField] public BoxCollider2D MovementCollider { get; protected set; }
    [field: SerializeField] public Animator Anim { get; protected set; }
    [field: SerializeField] public SpriteRenderer Sprite { get; protected set; }

    [field: SerializeField] public Vector2 CurrentVelocity { get; protected set; }
    [field: SerializeField] public int FacingDirection { get; protected set; } = 1;

    public bool isAnimationFinished { get; set; }
    public bool isExitingState { get; set; }

    protected virtual void OnEnable() {}

    protected virtual void OnDisable() {}

    protected virtual void Awake() {}

    protected virtual void Start() {}

    protected virtual void Update() {
        StateMachine?.CurrentState?.LogicUpdate();
        CurrentVelocity = Rb.velocity;
    }

    protected virtual void FixedUpdate() {
        StateMachine?.CurrentState?.PhysicsUpdate();
    }

    protected virtual void AnimationTrigger() => StateMachine?.CurrentState?.AnimationTrigger();

    protected virtual void AnimationFinishTrigger() => StateMachine?.CurrentState?.AnimationFinishTrigger();

    public virtual void CheckFacingDirection(int direction) {
        if (direction != 0 && direction != FacingDirection) {
            Flip();
        }
    }

    public virtual void Flip() {
        FacingDirection *= -1;

        if (FacingDirection == 1) Sprite.flipX = false;
        else Sprite.flipX = true;

        // Sprite.transform.localScale = new Vector2(FacingDirection, 1f);
    }

    public virtual void SetVelocityX(float velocity, float accelAmount = 30f, bool lerpVelocity = false) {}

    public virtual void SetVelocityY(float velocity, float accelAmount = 30f, bool lerpVelocity = false) {}

    public virtual void SetVelocity(float velocity, Vector2 angle, int direction, bool resetCurrentVelocity = true) {}

    public virtual bool CheckGround(LayerMask mask) { return false; }

    public virtual bool CheckCeiling() { return false; }

    public virtual bool CheckWall() { return false; }

    public virtual bool CheckBackWall() { return false; }

    public virtual bool CheckLedge() { return false; }

    public virtual bool CheckLedgeFoot() { return false; }

    public virtual bool CheckChangingDirections() { return false; }

    public virtual bool CheckFalling() { return false; }

    public virtual bool CheckAscending() { return false; }

    public virtual void Damage(object sender, OnEntityDamagedEventArgs entityDamagedArgs) {}
}
