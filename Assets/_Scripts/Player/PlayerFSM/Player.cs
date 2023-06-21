using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;


[Serializable]
public struct ColliderConfiguration {
    [field: SerializeField] public Vector2 Offset { get; private set; }
    [field: SerializeField] public Vector2 Size { get; private set; }
    
    public ColliderConfiguration(Vector2 offset, Vector2 size) {
        Offset = offset;
        Size = size;
    }
}

public class Player : MonoBehaviour {
#region STATES
    public PlayerData playerData;
    public PlayerStateMachine StateMachine { get; private set; }

    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerCrouchIdleState CrouchIdleState { get; private set; }
    public PlayerCrouchMoveState CrouchMoveState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerAirborneState AirborneState { get; private set; }
    public PlayerLandState LandState { get; private set; }
    public PlayerWallSlideState WallSlideState { get; private set; }
    public PlayerWallGrabState WallGrabState { get; private set; }
    public PlayerWallClimbState WallClimbState { get; private set; }
    public PlayerWallJumpState WallJumpState { get; private set; }
    public PlayerLedgeClimbState LedgeClimbState { get; private set; }
    public PlayerKnockbackState KnockbackState { get; private set; }
#endregion

#region COMPONENTS
    [field: SerializeField] public Animator Anim { get; private set; }
    [field: SerializeField] public SpriteRenderer PlayerSprite { get; private set; }
    [field: SerializeField] public PlayerInputHandler InputHandler { get; private set; }
    [field: SerializeField] public Rigidbody2D Rb { get; private set; }
    [field: SerializeField] public BoxCollider2D MovementCollider { get; private set; }
    // [field: SerializeField] public BoxCollider2D HitboxCollider { get; private set; }
    [field: SerializeField] public HealthSystem HealthSystem { get; private set; }
    [field: SerializeField] public Transform CameraTarget { get; private set; }
    [SerializeField] public Transform groundCheck;
    [SerializeField] public Transform ceilingCheck;
    [SerializeField] public Transform wallCheck;
    [SerializeField] public Transform ledgeCheck;
#endregion

#region VARIABLES
    private Vector2 workspace;
    private float velocityX;
    private float velocityY;
    private Vector2 velocityXY;
    public Vector2 CurrentVelocity { get; private set; }
    public int FacingDirection { get; private set; } = 1;

    [SerializeField] private bool drawGizmos;
#endregion

#region EVENTS
    public event Action OnKnockbackEnd;
#endregion

    public void KnockbackStart(Vector2 point, DamageDealer source) {
        lastContactPoint = point;
        KnockbackState.SetLastContactPoint(point);
        StateMachine.ChangeState(KnockbackState);
    }

    public void KnockbackEnd() {
        OnKnockbackEnd?.Invoke();
    }

#region UNITY CALLBACK FUNCTIONS
    private void OnEnable() {
        HealthSystem.OnDamagedSource += KnockbackStart;
    }

    private void OnDisable() {
        HealthSystem.OnDamagedSource -= KnockbackStart;
    }

    private void Awake() {
        StateMachine = new PlayerStateMachine();

        IdleState = new PlayerIdleState(this, StateMachine, playerData, "idle");
        MoveState = new PlayerMoveState(this, StateMachine, playerData, "move");
        CrouchIdleState = new PlayerCrouchIdleState(this, StateMachine, playerData, "crouchIdle");
        CrouchMoveState = new PlayerCrouchMoveState(this, StateMachine, playerData, "crouchMove");
        JumpState = new PlayerJumpState(this, StateMachine, playerData, "airborne");
        AirborneState = new PlayerAirborneState(this, StateMachine, playerData, "airborne");
        LandState = new PlayerLandState(this, StateMachine, playerData, "land");
        WallSlideState = new PlayerWallSlideState(this, StateMachine, playerData, "wallSlide");
        WallGrabState = new PlayerWallGrabState(this, StateMachine, playerData, "wallGrab");
        WallClimbState = new PlayerWallClimbState(this, StateMachine, playerData, "wallClimb");
        WallJumpState = new PlayerWallJumpState(this, StateMachine, playerData, "roll");
        LedgeClimbState = new PlayerLedgeClimbState(this, StateMachine, playerData, "ledgeClimbState");
        KnockbackState = new PlayerKnockbackState(this, StateMachine, playerData, "damaged");

        if (Anim == null) Anim = GetComponentInChildren<Animator>();
        if (PlayerSprite == null) PlayerSprite = GetComponentInChildren<SpriteRenderer>();
        if (InputHandler == null) InputHandler = GetComponent<PlayerInputHandler>();
        if (Rb == null) Rb = GetComponent<Rigidbody2D>();
        if (MovementCollider == null) MovementCollider = GetComponent<BoxCollider2D>();
        if (HealthSystem == null) HealthSystem = GetComponentInChildren<HealthSystem>();
        // if (HitboxCollider == null) HitboxCollider = HealthSystem.transform.GetComponent<BoxCollider2D>();
        if (CameraTarget == null) CameraTarget = GameObject.Find("CameraTarget").transform;
    }

    private void Start() {
        StateMachine.Initialize(IdleState);

        FacingDirection = 1;
        Rb.gravityScale = playerData.defaultGravityScale;

        SetColliderParameters(MovementCollider, playerData.standingColliderConfig);
        playerData.ResetPlayerInfo();
    }

    private void Update() {
        CurrentVelocity = Rb.velocity;
        UpdatePlayerValues();
        StateMachine.CurrentState.LogicUpdate();
    }
    
    private void FixedUpdate() {
        StateMachine.CurrentState.PhysicsUpdate();
    }
#endregion

#region VELOCITY AND MOVEMENT FUNCTIONS
    public void FreezeVelocity() {
        velocityXY.Set(0f, 0f);
        CurrentVelocity = velocityXY;
        Rb.velocity = CurrentVelocity;
    }

    public void SetVelocityX(float velocity, float accelAmount = 30f, bool lerpVelocity = false) {
        if (lerpVelocity)
            velocityX = Mathf.MoveTowards(velocityX, velocity, accelAmount * Time.deltaTime);
        else
            velocityX = velocity;
        
        CurrentVelocity = CurrentVelocity.SetXY(velocityX.Clamp(-playerData.maxRunSpeed, playerData.maxRunSpeed), CurrentVelocity.y);
        Rb.velocity = CurrentVelocity;
    }

    public void SetVelocityY(float velocity, float accelAmount = 30f, bool lerpVelocity = false) {
        if (lerpVelocity)
            velocityY = Mathf.MoveTowards(velocityY, velocity, accelAmount * Time.deltaTime);
        else
            velocityY = velocity;

        CurrentVelocity = CurrentVelocity.SetXY(CurrentVelocity.x, velocityY.Clamp(-playerData.currentFallSpeed, playerData.maxAscendantSpeed));
        Rb.velocity = CurrentVelocity;
    }

    public void SetVelocityYOnGround(float velocity, float accelAmount = 30f, bool lerpVelocity = false) {
        if (lerpVelocity)
            velocityY = Mathf.MoveTowards(velocityY, velocity, accelAmount * Time.deltaTime);
        else
            velocityY = velocity;

        CurrentVelocity = CurrentVelocity.SetXY(CurrentVelocity.x, velocityY.Clamp(-playerData.maxRunSpeed, playerData.maxRunSpeed));
        Rb.velocity = CurrentVelocity;
    }

    public void SetVelocity(float velocity, Vector2 angle, int direction) {
        angle.Normalize();
        velocityXY.Set((angle.x * velocity * direction).Clamp(-playerData.maxRunSpeed, playerData.maxRunSpeed), (angle.y * velocity).Clamp(-playerData.currentFallSpeed, playerData.maxAscendantSpeed));
        CurrentVelocity = velocityXY;
        Rb.velocity = CurrentVelocity;
    }

    public void SetForce(float velocity, Vector2 angle, int xDirection) {
        angle.Normalize();
        Vector2 forceDirection = new Vector2((angle.x * xDirection * velocity), (angle.y * velocity));
        Rb.AddForce(forceDirection, ForceMode2D.Impulse);
        Rb.velocity = new Vector2(Rb.velocity.x.Clamp(-playerData.maxRunSpeed, playerData.maxRunSpeed), Rb.velocity.y.Clamp(-playerData.currentFallSpeed, playerData.maxAscendantSpeed));
        CurrentVelocity = Rb.velocity;
    }

    // public bool lockPlatformDelay;

    // public void SetPlatformCollision(bool enable, float delay = 0f) {
    //     if (!enable) {
    //         if (!lockPlatformDelay) {
    //             StartCoroutine(PlatformCollisionDelayEnableRoutine());
    //             lockPlatformDelay = true;
    //         }
    //     }
    //     else {
    //         if (!lockPlatformDelay) {
    //             StartCoroutine(PlatformCollisionDelayDisableRoutine(delay));
    //             lockPlatformDelay = true;
    //         }
    //     }
    // }

    // public IEnumerator PlatformCollisionDelayEnableRoutine() {
    //     if (!Physics2D.GetIgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Platform"))) Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Platform"), true);
    //     if (gameObject.layer != LayerMask.NameToLayer("IgnorePlatforms")) gameObject.layer = LayerMask.NameToLayer("IgnorePlatforms");
    //     Debug.Log($"Disabled Platform Collision");
    //     yield return null;
    // }

    // public IEnumerator PlatformCollisionDelayDisableRoutine(float delay) {
    //     lockPlatformDelay = true;
    //     yield return new WaitForSeconds(delay);
    //     if (Physics2D.GetIgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Platform"))) Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Platform"), false);
    //     if (gameObject.layer != LayerMask.NameToLayer("Player")) gameObject.layer = LayerMask.NameToLayer("Player");
    //     lockPlatformDelay = false;
    //     Debug.Log($"Enabled Platform Collision");
    //     yield return null;
    // }
#endregion

#region CHECK FUNCTIONS
    public void CheckFacingDirection(int direction) {
        if (direction != 0 && direction != FacingDirection) {
            Flip();
        }
    }

    // public bool CheckGround(LayerMask mask) {
    //     return Physics2D.OverlapBox(groundCheck.position.ToVector2() + playerData.groundCheckOffset, playerData.groundCheckSize, 0f, mask);
    // }

    public bool CheckGround(LayerMask mask) {
        return Physics2D.Raycast(groundCheck.position.ToVector2() + (Vector2.right * playerData.groundCheckOffset.x) + (Vector2.up * playerData.groundCheckOffset.y), Vector2.down, playerData.groundCheckDistance, mask)
            || Physics2D.Raycast(groundCheck.position.ToVector2() + (Vector2.left * playerData.groundCheckOffset.x) + (Vector2.up * playerData.groundCheckOffset.y), Vector2.down, playerData.groundCheckDistance, mask)
            || Physics2D.Raycast(groundCheck.position.ToVector2() + (Vector2.up * playerData.groundCheckOffset.y), Vector2.down, playerData.groundCheckDistance, mask);
    }

    public bool CheckCeiling() {
        return Physics2D.Raycast(ceilingCheck.position.ToVector2() + (Vector2.right * playerData.ceilingCheckOffset) + (Vector2.down * playerData.ceilingCheckOffset.y), Vector2.up, playerData.ceilingCheckDistance, playerData.solidsLayer)
            || Physics2D.Raycast(ceilingCheck.position.ToVector2() + (Vector2.left * playerData.ceilingCheckOffset) + (Vector2.down * playerData.ceilingCheckOffset.y), Vector2.up, playerData.ceilingCheckDistance, playerData.solidsLayer)
            || Physics2D.Raycast(ceilingCheck.position.ToVector2() + (Vector2.down * playerData.ceilingCheckOffset.y), Vector2.up, playerData.ceilingCheckDistance, playerData.solidsLayer);
        // return Physics2D.OverlapBox(ceilingCheck.position.ToVector2() + playerData.ceilingCheckOffset, playerData.ceilingCheckSize, 0f, playerData.solidsLayer);
    }

    public bool CheckWall() {
        return Physics2D.Raycast(wallCheck.position.ToVector2() /*+ (Vector2.right * playerData.wallCheckOffset.x * FacingDirection)*/ + (Vector2.up * playerData.wallCheckOffset.y), Vector2.right * FacingDirection, playerData.wallCheckDistance, playerData.wallLayer);
    }

    public bool CheckBackWall() {
        return Physics2D.Raycast(wallCheck.position.ToVector2() /*+ (Vector2.left * playerData.wallCheckOffset.x * FacingDirection)*/ + (Vector2.up * playerData.wallCheckOffset.y), Vector2.right * -FacingDirection, playerData.wallCheckDistance, playerData.wallLayer);
    }

    public bool CheckLedge() {
        return Physics2D.Raycast(ledgeCheck.position.ToVector2() /*+ (Vector2.right * playerData.ledgeCheckOffset.x * FacingDirection)*/ + (Vector2.up * playerData.ledgeCheckOffset.y), Vector2.right * FacingDirection, playerData.ledgeCheckDistance, playerData.wallLayer);
    }

    public bool CheckFalling() {
        return Rb.velocity.y <= playerData.fallThreshold && !playerData.isGrounded;
    }

    public bool CheckAscending() {
        return Rb.velocity.y > 0.0f && !playerData.isGrounded;
    }

    public bool CheckCornerCorrection() {
        RaycastHit2D edgeLeftHit = GetHeadHit(-playerData.cornerEdgeCheckOffset);
        RaycastHit2D innerLeftHit = GetHeadHit(-playerData.cornerInnerCheckOffset);
        RaycastHit2D edgeRightHit = GetHeadHit(playerData.cornerEdgeCheckOffset);
        RaycastHit2D innerRightHit = GetHeadHit(playerData.cornerInnerCheckOffset);

        bool correctCorner = (edgeLeftHit && !innerLeftHit) || (edgeRightHit && !innerRightHit);
        return correctCorner;
    }
#endregion

#region OTHER FUNCTIONS
    private void AnimationTrigger() => StateMachine.CurrentState.AnimationTrigger();

    private void AnimationFinishTrigger() => StateMachine.CurrentState.AnimationFinishTrigger();

    private void Flip() {
        FacingDirection *= -1;
        if (FacingDirection == 1) PlayerSprite.flipX = false;
        else if (FacingDirection == -1) PlayerSprite.flipX = true;
    }

    public void UpdatePlayerValues() {
        playerData.currentVelocity = CurrentVelocity;
        playerData.facingDirection = FacingDirection == 1 ? Direction.Right : Direction.Left;
        playerData.wallJumpDirectionOffAngle = playerData.wallJumpAngle.AngleFloatToVector2();
        playerData.wallHopDirectionOffAngle = playerData.wallHopAngle.AngleFloatToVector2();
    }

    public void CalculateColliderHeight(float height) {
        Vector2 center = MovementCollider.offset;
        workspace.Set(MovementCollider.size.x, height);

        center.y += (height - MovementCollider.size.y) / 2;
        MovementCollider.size = workspace;
        MovementCollider.offset = center;

        ceilingCheck.position = new Vector2(ceilingCheck.position.x, MovementCollider.bounds.max.y);
    }

    public void SetColliderParameters(BoxCollider2D collider, ColliderConfiguration colliderConfig) {
        collider.offset = colliderConfig.Offset;
        collider.size = colliderConfig.Size;
        playerData.currentColliderConfiguration = colliderConfig;

        ceilingCheck.position = new Vector2(ceilingCheck.position.x, MovementCollider.bounds.max.y);
    }

    // public void SetCeilingCheckHeight() {
    //     ceilingCheck.transform.position = new Vector2(ceilingCheck.transform.position.x, MovementCollider.bounds.max.y);
    // }

    public void CorrectCorner(float currentYVelocity) {
        float newPos = 0f;

        // Push player to the right using left inner check
        RaycastHit2D innerLeftHit = Physics2D.Raycast(ceilingCheck.position.ToVector2() - playerData.cornerInnerCheckOffset + Vector2.up * playerData.cornerCheckDistance, Vector2.left, playerData.topCheckDistance, playerData.solidsLayer);
        if (innerLeftHit) {
            newPos = Vector2.Distance(new Vector2(innerLeftHit.point.x, transform.position.y) + Vector2.up * playerData.cornerCheckDistance,
                transform.position.ToVector2() - playerData.cornerEdgeCheckOffset + Vector2.up * playerData.cornerCheckDistance);
            transform.position = new Vector2(transform.position.x + newPos + playerData.cornerCorrectionRepositionOffset, transform.position.y);
            Rb.velocity = new Vector2(Rb.velocity.x, currentYVelocity);
            return;
        }

        // Push player to the left using right inner check
        RaycastHit2D innerRightHit = Physics2D.Raycast(ceilingCheck.position.ToVector2() + playerData.cornerInnerCheckOffset + Vector2.up * playerData.cornerCheckDistance, Vector2.right, playerData.topCheckDistance, playerData.solidsLayer);;
        if (innerRightHit) {
            newPos = Vector2.Distance(new Vector2(innerRightHit.point.x, transform.position.y) + Vector2.up * playerData.cornerCheckDistance,
                transform.position.ToVector2() + playerData.cornerEdgeCheckOffset + Vector2.up * playerData.cornerCheckDistance);
            transform.position = new Vector2(transform.position.x - newPos - playerData.cornerCorrectionRepositionOffset, transform.position.y);
            Rb.velocity = new Vector2(Rb.velocity.x, currentYVelocity);
        }
    }

    public RaycastHit2D GetHeadHit(Vector2 offset) {
        RaycastHit2D headHit = Physics2D.Raycast(ceilingCheck.position.ToVector2() + offset, Vector2.up, playerData.cornerCheckDistance, playerData.solidsLayer);
        return headHit;
    }
    
    public Vector2 GetCornerPosition() {
        Vector2 temp = Vector2.zero;
        RaycastHit2D xHit = Physics2D.Raycast(wallCheck.position.ToVector2(), Vector2.right * FacingDirection, playerData.ledgeCheckDistance, playerData.wallLayer);
        float xDist = xHit.distance;
        temp.Set((xDist + 0.05f) * FacingDirection, 0f);
        RaycastHit2D yHit = Physics2D.Raycast(ledgeCheck.position.ToVector2() + temp, Vector2.down, ledgeCheck.position.y - wallCheck.position.y + 0.05f, playerData.wallLayer);
        float yDist = yHit.distance;

        temp.Set(wallCheck.position.x + xDist * FacingDirection, ledgeCheck.position.y - yDist);

        return temp;
    }

    public RaycastHit2D GetGroundHit() {
        RaycastHit2D groundHit = Physics2D.Raycast(groundCheck.position.ToVector2() + (Vector2.up * playerData.groundCheckOffset.y), Vector2.down, playerData.groundCheckDistance, playerData.groundLayer);
        return groundHit;
    }

    public RaycastHit2D GetWallHit() {
        RaycastHit2D wallHit = Physics2D.Raycast(wallCheck.position, Vector2.right * FacingDirection, playerData.wallCheckDistance, playerData.wallLayer);
        return wallHit;
    }

    public RaycastHit2D GetPlatformHit() {
        RaycastHit2D platformHit = Physics2D.Raycast(groundCheck.position, Vector2.down, playerData.groundCheckDistance, playerData.platformLayer);
        return platformHit;
    }

    // public RaycastHit2D[] GetPlatformHits() {
    //     RaycastHit2D[] platformHits = Physics2D.RaycastAll(groundCheck.position, Vector2.down, playerData.groundCheckDistance, playerData.platformLayer);
    //     return platformHits;
    // }
#endregion

    public Vector2 detectedPos;
    public Vector2 cornerPos;
    public Vector2 startPos;
    public Vector2 stopPos;
    public Vector2 groundHitPos;
    public Vector2 lastContactPoint;

    private void OnDrawGizmos() {
        if (!drawGizmos) return;
        Gizmos.color = Color.green;
        // Ground Check Raycasts
        Gizmos.DrawRay(groundCheck.position.ToVector2() + (Vector2.right * playerData.groundCheckOffset.x) + (Vector2.up * playerData.groundCheckOffset.y), Vector3.down * playerData.groundCheckDistance);
        Gizmos.DrawRay(groundCheck.position.ToVector2() + (Vector2.left * playerData.groundCheckOffset.x) + (Vector2.up * playerData.groundCheckOffset.y), Vector3.down * playerData.groundCheckDistance);
        Gizmos.DrawRay(groundCheck.position.ToVector2() + (Vector2.up * playerData.groundCheckOffset.y), Vector3.down * playerData.groundCheckDistance);
        
        // Ceiling Check Raycasts
        Gizmos.DrawRay(ceilingCheck.position.ToVector2() + (playerData.ceilingCheckOffset * Vector2.right) + (Vector2.down * playerData.ceilingCheckOffset.y), Vector3.up * playerData.ceilingCheckDistance);
        Gizmos.DrawRay(ceilingCheck.position.ToVector2() + (playerData.ceilingCheckOffset * Vector2.left) + (Vector2.down * playerData.ceilingCheckOffset.y), Vector3.up * playerData.ceilingCheckDistance);
        Gizmos.DrawRay(ceilingCheck.position.ToVector2() + (Vector2.down * playerData.ceilingCheckOffset.y), Vector3.up * playerData.ceilingCheckDistance);
        //Gizmos.DrawWireCube(ceilingCheck.position.ToVector2() + playerData.ceilingCheckOffset, playerData.ceilingCheckSize);

        // Corner Check Raycasts
        // Edge
        Gizmos.DrawRay(ceilingCheck.position.ToVector2() + playerData.cornerEdgeCheckOffset, Vector3.up * playerData.cornerCheckDistance);
        Gizmos.DrawRay(ceilingCheck.position.ToVector2() - playerData.cornerEdgeCheckOffset, Vector3.up * playerData.cornerCheckDistance);
        // Inner
        Gizmos.color = Color.red;
        Gizmos.DrawRay(ceilingCheck.position.ToVector2() + playerData.cornerInnerCheckOffset, Vector3.up * playerData.cornerCheckDistance);
        Gizmos.DrawRay(ceilingCheck.position.ToVector2() - playerData.cornerInnerCheckOffset, Vector3.up * playerData.cornerCheckDistance);
        // Sides
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(ceilingCheck.position.ToVector2() - playerData.cornerInnerCheckOffset + Vector2.up * playerData.cornerCheckDistance, Vector3.left * playerData.topCheckDistance);
        Gizmos.DrawRay(ceilingCheck.position.ToVector2() + playerData.cornerInnerCheckOffset + Vector2.up * playerData.cornerCheckDistance, Vector3.right * playerData.topCheckDistance);
        
        // Wall Check Raycasts
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(wallCheck.position.ToVector2() /*+ (Vector2.right * playerData.wallCheckOffset.x * FacingDirection)*/ + (Vector2.up * playerData.wallCheckOffset.y), Vector2.right * FacingDirection * playerData.wallCheckDistance);
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(wallCheck.position.ToVector2() /*+ (Vector2.left * playerData.wallCheckOffset.x * FacingDirection)*/ + (Vector2.up * playerData.wallCheckOffset.y), Vector2.right * -FacingDirection * playerData.wallCheckDistance);

        // Ledge Check Raycasts
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(ledgeCheck.position.ToVector2() /*+ (Vector2.right * playerData.ledgeCheckOffset.x * FacingDirection)*/ + (Vector2.up * playerData.ledgeCheckOffset.y), Vector3.right * FacingDirection * playerData.ledgeCheckDistance);
    
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(groundHitPos, 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(lastContactPoint, 0.5f);

        if (!playerData.isHanging) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(detectedPos, 0.5f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(cornerPos, 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(startPos, 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(stopPos, 0.5f);
    }
}
