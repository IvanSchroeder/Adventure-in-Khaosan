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

public class Player : Entity, IDamageable, IInteractor {
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerCrouchIdleState CrouchIdleState { get; private set; }
    public PlayerCrouchMoveState CrouchMoveState { get; private set; }
    public PlayerGroundSlideState GroundSlideState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerAirborneState AirborneState { get; private set; }
    public PlayerLandState LandState { get; private set; }
    public PlayerWallSlideState WallSlideState { get; private set; }
    public PlayerWallGrabState WallGrabState { get; private set; }
    public PlayerWallClimbState WallClimbState { get; private set; }
    public PlayerWallJumpState WallJumpState { get; private set; }
    public PlayerLedgeClimbState LedgeClimbState { get; private set; }
    public PlayerKnockbackState KnockbackState { get; private set; }
    public PlayerDeathState DeathState { get; private set; }

    [Header("States")]
    [Space(5f)]
    private PlayerState CurrentState;
    private PlayerState PreviousState;
    public string CurrentStateName;
    public string PreviousStateName;

    [Space(10f), Header("General"), Space(5f)]
    public List<BoolSO> PlayerAbilitiesList;
    public PlayerData playerData;
    [field: SerializeField] public Rigidbody2D Rb { get; private set; }
    [field: SerializeField] public BoxCollider2D MovementCollider { get; private set; }
    [field: SerializeField] public Animator Anim { get; private set; }
    [field: SerializeField] public SpriteRenderer PlayerSprite { get; private set; }
    [field: SerializeField] public CameraTarget CameraTarget { get; private set; }

    [field: SerializeField, Space(10f), Header("Damageable"), Space(5f)] public BoxCollider2D HitboxTrigger { get; set; }
    [field: SerializeField] public HealthSystem HealthSystem { get; set; }
    [field: SerializeField] public SpriteManager SpriteManager { get; set; }

    [field: SerializeField, Space(10f), Header("Interactor"), Space(5f)] public BoxCollider2D InteractorTrigger { get; set; }
    [field: SerializeField] public InteractorSystem InteractorSystem { get; set; }
    [field: SerializeField] public PlayerInputHandler InputHandler { get; set; }

    [field: SerializeField, Space(10f), Header("Checks"), Space(5f)] public Transform GroundPoint { get; private set; }
    [field: SerializeField] public Transform MidPoint { get; private set; }
    [field: SerializeField] public Transform CeilingPoint { get; private set; }
    [field: SerializeField] public Transform WallPoint { get; private set; }
    [field: SerializeField] public Transform LedgePoint { get; private set; }

    private Vector2 workspace;
    private float velocityX;
    private float velocityY;
    private Vector2 velocityXY;
    public Vector2 CurrentVelocity { get; private set; }
    public int FacingDirection { get; private set; } = 1;

    [HideInInspector] public Vector2 detectedPos;
    [HideInInspector] public Vector2 cornerPos;
    [HideInInspector] public Vector2 startPos;
    [HideInInspector] public Vector2 stopPos;
    [HideInInspector] public Vector2 groundHitPos;
    [HideInInspector] public Vector2 wallHitPos;
    [HideInInspector] public Vector2 ceilingHitPos;
    [HideInInspector] public Vector2 lastContactPoint;
    [HideInInspector] public Vector2 damageDirection;

    [SerializeField] private bool drawGizmos;

    public static event Action<Player> OnPlayerSpawned;
    public static event Action<Player> OnPlayerDeath;
    public static event Action OnPlayerDamaged;
    
    public Action OnInvulnerability;
    public Action OnPlayerDeathEnd;
    public event Action OnLivesDepleted;

    private void UpdateCurrentState(PlayerState current, PlayerState previous) {
        CurrentState = current;
        PreviousState = previous;

        CurrentStateName = $"{CurrentState.ToString()}";
        if (PreviousState.IsNull()) PreviousStateName = "";
        else PreviousStateName = $"{PreviousState.ToString()}";
    }

    private void OnEnable() {
        LevelManager.OnPlayerSpawn += RespawnPlayer;
        LevelManager.OnLevelFinished += StandOnCheckpoint;

        HealthSystem.OnEntityDamaged += KnockbackStart;
        HealthSystem.OnEntityDeath += PlayerDeathStart;
        HealthSystem.OnLivesDepleted += SetLivesDepleted;

        OnEntityDamaged += HealthSystem.ReduceHealth;
        // OnInvulnerable += HealthSystem.SetInvulnerability;
        OnInvulnerability += HealthSystem.SetInvulnerability;

        StateMachine.OnStateChange += UpdateCurrentState;
    }

    private void OnDisable() {
        LevelManager.OnPlayerSpawn -= RespawnPlayer;
        LevelManager.OnLevelFinished -= StandOnCheckpoint;
        
        HealthSystem.OnEntityDamaged -= KnockbackStart;
        HealthSystem.OnEntityDeath -= PlayerDeathStart;
        HealthSystem.OnLivesDepleted -= SetLivesDepleted;

        OnEntityDamaged -= HealthSystem.ReduceHealth;
        // OnInvulnerable -= HealthSystem.SetInvulnerability;
        OnInvulnerability -= HealthSystem.SetInvulnerability;

        StateMachine.OnStateChange -= UpdateCurrentState;
    }

    private void Awake() {
        StateMachine = new PlayerStateMachine();

        // IdleState = PlayerState.CreateInstance<PlayerIdleState>(this, StateMachine, playerData, "idle");
        // MoveState = PlayerState.CreateInstance<PlayerMoveState>(this, StateMachine, playerData, "move");
        // CrouchIdleState = PlayerState.CreateInstance<PlayerCrouchIdleState>(this, StateMachine, playerData, "crouchIdle");
        // CrouchMoveState = PlayerState.CreateInstance<PlayerCrouchMoveState>(this, StateMachine, playerData, "crouchMove");
        // GroundSlideState = PlayerState.CreateInstance<PlayerGroundSlideState>(this, StateMachine, playerData, "groundSlide");
        // JumpState = PlayerState.CreateInstance<PlayerJumpState>(this, StateMachine, playerData, "airborne");
        // AirborneState = PlayerState.CreateInstance<PlayerAirborneState>(this, StateMachine, playerData, "airborne");
        // LandState = PlayerState.CreateInstance<PlayerLandState>(this, StateMachine, playerData, "land");
        // WallSlideState = PlayerState.CreateInstance<PlayerWallSlideState>(this, StateMachine, playerData, "wallSlide");
        // WallGrabState = PlayerState.CreateInstance<PlayerWallGrabState>(this, StateMachine, playerData, "wallGrab");
        // WallClimbState = PlayerState.CreateInstance<PlayerWallClimbState>(this, StateMachine, playerData, "wallClimb");
        // WallJumpState = PlayerState.CreateInstance<PlayerWallJumpState>(this, StateMachine, playerData, "roll");
        // LedgeClimbState = PlayerState.CreateInstance<PlayerLedgeClimbState>(this, StateMachine, playerData, "ledgeClimbState");
        // KnockbackState = PlayerState.CreateInstance<PlayerKnockbackState>(this, StateMachine, playerData, "damaged");
        // DeathState = PlayerState.CreateInstance<PlayerDeathState>(this, StateMachine, playerData, "dead");

        IdleState = new PlayerIdleState(this, StateMachine, playerData, "idle");
        MoveState = new PlayerMoveState(this, StateMachine, playerData, "move");
        CrouchIdleState = new PlayerCrouchIdleState(this, StateMachine, playerData, "crouchIdle");
        CrouchMoveState = new PlayerCrouchMoveState(this, StateMachine, playerData, "crouchMove");
        GroundSlideState = new PlayerGroundSlideState(this, StateMachine, playerData, "groundSlide");
        JumpState = new PlayerJumpState(this, StateMachine, playerData, "airborne");
        AirborneState = new PlayerAirborneState(this, StateMachine, playerData, "airborne");
        LandState = new PlayerLandState(this, StateMachine, playerData, "land");
        WallSlideState = new PlayerWallSlideState(this, StateMachine, playerData, "wallSlide");
        WallGrabState = new PlayerWallGrabState(this, StateMachine, playerData, "wallGrab");
        WallClimbState = new PlayerWallClimbState(this, StateMachine, playerData, "wallClimb");
        WallJumpState = new PlayerWallJumpState(this, StateMachine, playerData, "roll");
        LedgeClimbState = new PlayerLedgeClimbState(this, StateMachine, playerData, "ledgeClimbState");
        KnockbackState = new PlayerKnockbackState(this, StateMachine, playerData, "damaged");
        DeathState = new PlayerDeathState(this, StateMachine, playerData, "dead");

        if (Anim == null) Anim = GetComponentInChildren<Animator>();
        if (PlayerSprite == null) PlayerSprite = GetComponentInChildren<SpriteRenderer>();
        if (InputHandler == null) InputHandler = GetComponent<PlayerInputHandler>();
        if (Rb == null) Rb = GetComponent<Rigidbody2D>();
        if (MovementCollider == null) MovementCollider = GetComponent<BoxCollider2D>();
        if (CameraTarget == null) CameraTarget = GetComponentInChildren<CameraTarget>();

        if (HealthSystem == null) HealthSystem = GetComponentInChildren<HealthSystem>();
        if (HitboxTrigger == null) HitboxTrigger = HealthSystem.GetComponent<BoxCollider2D>();
        if (SpriteManager == null) SpriteManager = PlayerSprite.GetComponent<SpriteManager>();

        if (InteractorSystem == null) InteractorSystem = this.GetComponentInHierarchy<InteractorSystem>();
        if (InteractorTrigger == null) InteractorTrigger = InteractorSystem.GetComponent<BoxCollider2D>();

        CurrentState = null;
        PreviousState = null;
    }

    public void RespawnPlayer() {
        if (CurrentState == null) StateMachine.Initialize(IdleState);
        else StateMachine.ChangeState(IdleState);

        JumpState.ResetAmountOfJumpsLeft();
        HealthSystem.InitializeHealth();
        playerData.Init();

        FacingDirection = 1;
        Rb.gravityScale = playerData.defaultGravityScale;
        CheckFacingDirection(FacingDirection);
        PlayerSprite.flipX = false;
        CameraTarget.transform.SetParent(CameraTarget.CameraCenter);
        CameraTarget.transform.localPosition = Vector3.zero;

        SetColliderParameters(MovementCollider, playerData.standingColliderConfig);
        SetColliderParameters(HitboxTrigger, playerData.standingColliderConfig);

        OnInvulnerability?.Invoke();
        OnPlayerSpawned?.Invoke(this);
    }

    private void Update() {
        CurrentVelocity = Rb.velocity;
        StateMachine.CurrentState.LogicUpdate();
    }
    
    private void FixedUpdate() {
        StateMachine.CurrentState.PhysicsUpdate();

        Debug.DrawRay(MidPoint.position, playerData.wallHopDirectionOffAngle * playerData.wallHopSpeed, Color.cyan);
        Debug.DrawRay(MidPoint.position, playerData.wallJumpDirectionOffAngle * playerData.wallJumpSpeed, Color.magenta);
        Debug.DrawRay(MidPoint.position, playerData.deathJumpDirectionOffAngle * playerData.deathJumpSpeed, Color.red);
    }

    public void Damage(object sender, OnEntityDamagedEventArgs entityDamagedArgs) {
        lastContactPoint = entityDamagedArgs.ContactPoint;
        OnEntityDamaged?.Invoke(sender, entityDamagedArgs);
        OnPlayerDamaged?.Invoke();
    }

    public void KnockbackStart(object sender, OnEntityDamagedEventArgs entityDamagedArgs) {
        lastContactPoint = entityDamagedArgs.ContactPoint;
        damageDirection = (MidPoint.position.ToVector2() - lastContactPoint).normalized;
        SetVelocity(playerData.jumpHeight * 0.5f, 45f.AngleFloatToVector2(), -FacingDirection, true);
        StateMachine.ChangeState(KnockbackState);
    }

    public void KnockbackEnd() {
        OnInvulnerability?.Invoke();
    }

    public void PlayerDeathStart(object sender, OnEntityDamagedEventArgs args) {
        damageDirection = (MidPoint.position.ToVector2() - lastContactPoint).normalized;
        SetVelocity(playerData.deathJumpSpeed, playerData.deathJumpDirectionOffAngle, -FacingDirection, true);
        CheckFacingDirection(FacingDirection);
        OnPlayerDeath?.Invoke(this);
        StateMachine.ChangeState(DeathState);
    }

    public void SetLivesDepleted(object sender, EventArgs args) {
        OnLivesDepleted?.Invoke();
    }

    public void PlayerDeathEnd() {
        OnPlayerDeathEnd?.Invoke();
    }

    public void SetVelocityX(float velocity, float accelAmount = 30f, bool lerpVelocity = false) {
        if (lerpVelocity)
            velocityX = Mathf.MoveTowards(velocityX, velocity, accelAmount * Time.deltaTime);
        else
            velocityX = velocity;
        
        CurrentVelocity = CurrentVelocity.SetXY(velocityX.Clamp(-playerData.maxHorizontalSpeed, playerData.maxHorizontalSpeed), CurrentVelocity.y);
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

        CurrentVelocity = CurrentVelocity.SetXY(CurrentVelocity.x, velocityY.Clamp(-playerData.maxHorizontalSpeed, playerData.maxHorizontalSpeed));
        Rb.velocity = CurrentVelocity;
    }

    public void SetVelocity(float velocity, Vector2 angle, int direction, bool resetCurrentVelocity = true) {
        if (resetCurrentVelocity) {
            CurrentVelocity = new Vector2(0f, 0f);
            Rb.velocity = new Vector2(0f, 0f);
        }

        angle.Normalize();
        velocityXY.Set((angle.x.ToInt() * velocity * direction).Clamp(-playerData.maxHorizontalSpeed, playerData.maxHorizontalSpeed), (angle.y.ToInt() * velocity).Clamp(-playerData.defaultFallSpeed, playerData.maxAscendantSpeed));
        CurrentVelocity = velocityXY;
        Rb.velocity = CurrentVelocity;
    }

    // public void LaunchInDirection(float velocity, Vector2 angle, bool clamp, bool resetCurrentVelocity = true) {
    //     if (resetCurrentVelocity) {
    //         CurrentVelocity = new Vector2(0f, 0f);
    //         Rb.velocity = new Vector2(0f, 0f);
    //     }

    //     angle.Normalize();
    //     Vector2 forceDirection = new Vector2((angle.x * velocity).Clamp(-playerData.maxHorizontalSpeed, playerData.maxHorizontalSpeed),
    //     (angle.y * velocity).Clamp(-playerData.currentFallSpeed, playerData.maxAscendantSpeed));
        
    //     if (clamp) 
    //         Rb.AddForce(forceDirection.Clamp(new Vector2 (-playerData.maxHorizontalSpeed, playerData.maxHorizontalSpeed), new Vector2 (-playerData.currentFallSpeed, playerData.maxAscendantSpeed)), ForceMode2D.Impulse);
    //     else
    //         Rb.AddForce(forceDirection, ForceMode2D.Impulse);

    //     Debug.DrawRay(MidPoint.position, forceDirection, Color.red, 3f);

    //     CurrentVelocity = Rb.velocity;

    //     CheckFacingDirection(FacingDirection);
    // }

    public void CheckFacingDirection(int direction) {
        if (direction != 0 && direction != FacingDirection) {
            Flip();
        }

        if (FacingDirection == 1) PlayerSprite.flipX = false;
        else if (FacingDirection == -1) PlayerSprite.flipX = true;
    }

    public bool CheckGround(LayerMask mask) {
        return Physics2D.Raycast(GroundPoint.position.ToVector2() + (Vector2.right * playerData.groundCheckOffset.x) + (Vector2.up * playerData.groundCheckOffset.y), Vector2.down, playerData.groundCheckDistance, mask)
            || Physics2D.Raycast(GroundPoint.position.ToVector2() + (Vector2.left * playerData.groundCheckOffset.x) + (Vector2.up * playerData.groundCheckOffset.y), Vector2.down, playerData.groundCheckDistance, mask)
            || Physics2D.Raycast(GroundPoint.position.ToVector2() + (Vector2.up * playerData.groundCheckOffset.y), Vector2.down, playerData.groundCheckDistance, mask);
    }

    public bool CheckCeiling() {
        return Physics2D.Raycast(CeilingPoint.position.ToVector2() + (Vector2.right * playerData.ceilingCheckOffset) + (Vector2.down * playerData.ceilingCheckOffset.y), Vector2.up, playerData.ceilingCheckDistance, playerData.solidsLayer)
            || Physics2D.Raycast(CeilingPoint.position.ToVector2() + (Vector2.left * playerData.ceilingCheckOffset) + (Vector2.down * playerData.ceilingCheckOffset.y), Vector2.up, playerData.ceilingCheckDistance, playerData.solidsLayer)
            || Physics2D.Raycast(CeilingPoint.position.ToVector2() + (Vector2.down * playerData.ceilingCheckOffset.y), Vector2.up, playerData.ceilingCheckDistance, playerData.solidsLayer);
    }

    public bool CheckWall() {
        return Physics2D.Raycast(WallPoint.position.ToVector2() + (Vector2.up * playerData.wallCheckOffset.y), Vector2.right * FacingDirection, playerData.wallCheckDistance, playerData.wallLayer);
    }

    public bool CheckBackWall() {
        return Physics2D.Raycast(WallPoint.position.ToVector2() + (Vector2.up * playerData.wallCheckOffset.y), Vector2.right * -FacingDirection, playerData.wallCheckDistance, playerData.wallLayer);
    }

    public bool CheckLedge() {
        return Physics2D.Raycast(LedgePoint.position.ToVector2() + (Vector2.up * playerData.ledgeCheckOffset.y), Vector2.right * FacingDirection, playerData.ledgeCheckDistance, playerData.wallLayer);
    }

    public bool CheckLedgeFoot() {
        return Physics2D.Raycast(GroundPoint.position.ToVector2(), Vector2.right * FacingDirection, playerData.ledgeCheckDistance, playerData.wallLayer);
    }

    public bool CheckChangingDirections() {
        return ((InputHandler.NormInputX != 0 && InputHandler.NormInputX.Sign() != CurrentVelocity.x.Sign())
            || (InputHandler.NormInputX == 0 && FacingDirection.Sign() != CurrentVelocity.x.Sign())) && CurrentVelocity.x != 0;
    }

    public bool CheckFalling() {
        return CurrentVelocity.y <= playerData.fallThreshold && !playerData.isGrounded;
    }

    public bool CheckAscending() {
        return CurrentVelocity.y > 0.0f && !playerData.isGrounded;
    }

    public bool CheckForSpace(Vector2 originPoint, Vector2 direction) {
        bool space = !Physics2D.Raycast(originPoint, direction, 1.25f, playerData.wallLayer);

        if (space) Debug.DrawRay(originPoint, direction * 1.25f, Color.green, 1f);
        else Debug.DrawRay(originPoint, direction * 1.25f, Color.red, 1f);

        return space;
    }

    public bool CheckHazards(Vector2 originPoint) {
        bool hazard = Physics2D.Raycast(originPoint, Vector2.up, 1.25f, playerData.hazardsLayer);

        if (hazard) Debug.DrawRay(originPoint, Vector2.up * 1.25f, Color.red, 1f);
        else Debug.DrawRay(originPoint, Vector2.up * 1.25f, Color.green, 1f);

        return hazard;
    }

    public bool CheckCeilingCornerCorrection() {
        RaycastHit2D edgeLeftHit = GetCeilingHit(-playerData.cornerEdgeCheckOffset);
        RaycastHit2D innerLeftHit = GetCeilingHit(-playerData.cornerInnerCheckOffset);
        RaycastHit2D edgeRightHit = GetCeilingHit(playerData.cornerEdgeCheckOffset);
        RaycastHit2D innerRightHit = GetCeilingHit(playerData.cornerInnerCheckOffset);

        bool correctCorner = (edgeLeftHit && !innerLeftHit) || (edgeRightHit && !innerRightHit);
        return correctCorner;
    }

    public bool CheckFootLedgeCorrection() {
        RaycastHit2D innerFootHit = GetFootLedgeHit(playerData.ledgeInnerCheckOffset);
        RaycastHit2D edgeFootHit = GetFootLedgeHit(playerData.ledgeEdgeCheckOffset);

        bool correctLedge = (!innerFootHit && edgeFootHit);
        return correctLedge;
    }

    private void AnimationTrigger() => StateMachine.CurrentState.AnimationTrigger();

    private void AnimationFinishTrigger() => StateMachine.CurrentState.AnimationFinishTrigger();

    private void Flip() {
        FacingDirection *= -1;
        if (FacingDirection == 1) PlayerSprite.flipX = false;
        else if (FacingDirection == -1) PlayerSprite.flipX = true;
    }

    public void SetColliderParameters(BoxCollider2D collider, ColliderConfiguration colliderConfig) {
        collider.offset = colliderConfig.Offset;
        collider.size = colliderConfig.Size;
        playerData.currentColliderConfiguration = colliderConfig;

        CeilingPoint.position = new Vector2(CeilingPoint.position.x, collider.bounds.max.y);
    }

    public void StandOnCheckpoint() {
        CurrentVelocity = Vector2.zero;
        Rb.velocity = CurrentVelocity;

        if (playerData.isAirborne) {
            transform.position = LevelManager.instance.currentCheckpoint.SpawnpointTransform.position;
        }
        else if (playerData.isGrounded) {
            transform.position = LevelManager.instance.currentCheckpoint.BasepointTransform.position;
        }
    }

    public void CorrectHeadCorner(float currentYVelocity) {
        float newPos = 0f;

        // Push player to the right using left inner check
        RaycastHit2D innerLeftHit = Physics2D.Raycast(CeilingPoint.position.ToVector2() - playerData.cornerInnerCheckOffset + Vector2.up * playerData.cornerCheckDistance, Vector2.left, playerData.topCheckDistance, playerData.solidsLayer);
        
        if (innerLeftHit) {
            newPos = Vector2.Distance(new Vector2(innerLeftHit.point.x, transform.position.y) + Vector2.up * playerData.cornerCheckDistance,
                transform.position.ToVector2() - playerData.cornerEdgeCheckOffset + Vector2.up * playerData.cornerCheckDistance);
            transform.position = new Vector2(transform.position.x + newPos + playerData.cornerCorrectionRepositionOffset, transform.position.y);
            Rb.velocity = new Vector2(Rb.velocity.x, currentYVelocity);
            return;
        }

        // Push player to the left using right inner check
        RaycastHit2D innerRightHit = Physics2D.Raycast(CeilingPoint.position.ToVector2() + playerData.cornerInnerCheckOffset + Vector2.up * playerData.cornerCheckDistance, Vector2.right, playerData.topCheckDistance, playerData.solidsLayer);;
        
        if (innerRightHit) {
            newPos = Vector2.Distance(new Vector2(innerRightHit.point.x, transform.position.y) + Vector2.up * playerData.cornerCheckDistance,
                transform.position.ToVector2() + playerData.cornerEdgeCheckOffset + Vector2.up * playerData.cornerCheckDistance);
            transform.position = new Vector2(transform.position.x - newPos - playerData.cornerCorrectionRepositionOffset, transform.position.y);
            Rb.velocity = new Vector2(Rb.velocity.x, currentYVelocity);
        }

        SetVelocityY(playerData.jumpHeight);
    }

    public void CorrectFootLedge(float currentXVelocity) {
        float newPos = 0f;

        RaycastHit2D innerFootHit = Physics2D.Raycast(GroundPoint.position.ToVector2() + playerData.ledgeInnerCheckOffset + Vector2.right * FacingDirection * playerData.ledgeFootCheckDistance, Vector2.down, playerData.groundUpCheckDistance, playerData.solidsLayer);
        
        if (innerFootHit && CheckForSpace(innerFootHit.point + Vector2.up * 0.015f, Vector2.up)) {
            newPos = Vector2.Distance(new Vector2(transform.position.x, innerFootHit.point.y) + Vector2.down * playerData.groundUpCheckDistance,
                transform.position.ToVector2() - playerData.ledgeEdgeCheckOffset + Vector2.right * FacingDirection * playerData.ledgeFootCheckDistance);
            
            transform.position = new Vector2(transform.position.x, innerFootHit.point.y + playerData.ledgeCorrectionRepositionOffset);
            // transform.position = new Vector2(transform.position.x, transform.position.y - newPos + playerData.ledgeCorrectionRepositionOffset);
            Rb.velocity = new Vector2(currentXVelocity, Rb.velocity.y);
            return;
        }
    }
    
    public Vector2 GetCornerPosition() {
        Vector2 temp = Vector2.zero;
        RaycastHit2D xHit = Physics2D.Raycast(WallPoint.position.ToVector2(), Vector2.right * FacingDirection, playerData.ledgeCheckDistance, playerData.wallLayer);
        float xDist = xHit.distance;
        temp.Set((xDist + 0.05f) * FacingDirection, 0f);
        RaycastHit2D yHit = Physics2D.Raycast(LedgePoint.position.ToVector2() + temp, Vector2.down, LedgePoint.position.y - WallPoint.position.y + 0.05f, playerData.wallLayer);
        float yDist = yHit.distance;

        temp.Set(WallPoint.position.x + xDist * FacingDirection, LedgePoint.position.y - yDist);

        return temp;
    }

    public RaycastHit2D GetGroundHit() {
        RaycastHit2D groundHit = Physics2D.Raycast(GroundPoint.position.ToVector2() + (Vector2.up * playerData.groundCheckOffset.y), Vector2.down, playerData.groundCheckDistance, playerData.groundLayer);
        Debug.DrawRay(GroundPoint.position.ToVector2() + (Vector2.up * playerData.groundCheckOffset.y), Vector2.up * playerData.groundCheckDistance, Color.green, 3f);
        return groundHit;
    }

    public RaycastHit2D GetPlatformHit() {
        RaycastHit2D platformHit = Physics2D.Raycast(MidPoint.position.ToVector2(), Vector2.down, 2f, playerData.platformLayer);
        Debug.DrawRay(MidPoint.position, Vector2.up * 2f, Color.green, 3f);
        return platformHit;
    }

    public RaycastHit2D GetCeilingHit(Vector2 offset) {
        RaycastHit2D headHit = Physics2D.Raycast(CeilingPoint.position.ToVector2() + offset, Vector2.up, playerData.cornerCheckDistance, playerData.solidsLayer);
        Debug.DrawRay(GroundPoint.position.ToVector2() + (Vector2.up * playerData.groundCheckOffset.y), Vector2.down * playerData.ceilingCheckDistance, Color.white, 3f);
        return headHit;
    }

    public RaycastHit2D GetFootLedgeHit(Vector2 offset) {
        RaycastHit2D footHit = Physics2D.Raycast(GroundPoint.position.ToVector2() + offset, Vector2.right * FacingDirection, playerData.cornerCheckDistance, playerData.solidsLayer);
        return footHit;
    }

    public RaycastHit2D GetWallHit(int direction) {
        RaycastHit2D wallHit = Physics2D.Raycast(MidPoint.position.ToVector2(), Vector2.right * direction, 1f, playerData.wallLayer);
        if (wallHit)
            Debug.DrawRay(WallPoint.position.ToVector2(), Vector2.right * direction * playerData.wallCheckDistance, Color.cyan, 3f);
        else
            Debug.DrawRay(WallPoint.position.ToVector2(), Vector2.right * direction * playerData.wallCheckDistance, Color.red, 3f);
        return wallHit;
    }

    private void OnDrawGizmos() {
        if (!drawGizmos) return;
        Gizmos.color = Color.green;
        // Ground Check Raycasts
        Gizmos.DrawRay(GroundPoint.position.ToVector2() + (Vector2.right * playerData.groundCheckOffset.x) + (Vector2.up * playerData.groundCheckOffset.y), Vector2.down * playerData.groundCheckDistance);
        Gizmos.DrawRay(GroundPoint.position.ToVector2() + (Vector2.left * playerData.groundCheckOffset.x) + (Vector2.up * playerData.groundCheckOffset.y), Vector2.down * playerData.groundCheckDistance);
        Gizmos.DrawRay(GroundPoint.position.ToVector2() + (Vector2.up * playerData.groundCheckOffset.y), Vector2.down * playerData.groundCheckDistance);
        
        // Ceiling Check Raycasts
        Gizmos.DrawRay(CeilingPoint.position.ToVector2() + (playerData.ceilingCheckOffset * Vector2.right) + (Vector2.down * playerData.ceilingCheckOffset.y), Vector2.up * playerData.ceilingCheckDistance);
        Gizmos.DrawRay(CeilingPoint.position.ToVector2() + (playerData.ceilingCheckOffset * Vector2.left) + (Vector2.down * playerData.ceilingCheckOffset.y), Vector2.up * playerData.ceilingCheckDistance);
        Gizmos.DrawRay(CeilingPoint.position.ToVector2() + (Vector2.down * playerData.ceilingCheckOffset.y), Vector2.up * playerData.ceilingCheckDistance);

        // Corners Check Raycasts
        // Head Corner
        // Edge
        Gizmos.DrawRay(CeilingPoint.position.ToVector2() + playerData.cornerEdgeCheckOffset, Vector2.up * playerData.cornerCheckDistance);
        Gizmos.DrawRay(CeilingPoint.position.ToVector2() - playerData.cornerEdgeCheckOffset, Vector2.up * playerData.cornerCheckDistance);
        // Inner
        Gizmos.color = Color.red;
        Gizmos.DrawRay(CeilingPoint.position.ToVector2() + playerData.cornerInnerCheckOffset, Vector2.up * playerData.cornerCheckDistance);
        Gizmos.DrawRay(CeilingPoint.position.ToVector2() - playerData.cornerInnerCheckOffset, Vector2.up * playerData.cornerCheckDistance);
        // Sides
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(CeilingPoint.position.ToVector2() - playerData.cornerInnerCheckOffset + Vector2.up * playerData.cornerCheckDistance, Vector2.left * playerData.topCheckDistance);
        Gizmos.DrawRay(CeilingPoint.position.ToVector2() + playerData.cornerInnerCheckOffset + Vector2.up * playerData.cornerCheckDistance, Vector2.right * playerData.topCheckDistance);
        
        // Foor Ledge
        // Edge
        Gizmos.color = Color.green;
        Gizmos.DrawRay(GroundPoint.position.ToVector2() + playerData.ledgeEdgeCheckOffset, Vector2.right * FacingDirection * playerData.ledgeFootCheckDistance);
        // Inner
        Gizmos.color = Color.red;
        Gizmos.DrawRay(GroundPoint.position.ToVector2() + playerData.ledgeInnerCheckOffset, Vector2.right * FacingDirection * playerData.ledgeFootCheckDistance);
        // Upward
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(GroundPoint.position.ToVector2() + playerData.ledgeInnerCheckOffset + Vector2.right * FacingDirection * playerData.ledgeFootCheckDistance, Vector2.down * playerData.groundUpCheckDistance);


        // Wall Check Raycasts
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(WallPoint.position.ToVector2() + (Vector2.up * playerData.wallCheckOffset.y), Vector2.right * FacingDirection * playerData.wallCheckDistance);
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(WallPoint.position.ToVector2() + (Vector2.up * playerData.wallCheckOffset.y), Vector2.right * -FacingDirection * playerData.wallCheckDistance);

        // Ledge Check Raycasts
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(LedgePoint.position.ToVector2() + (Vector2.up * playerData.ledgeCheckOffset.y), Vector2.right * FacingDirection * playerData.ledgeCheckDistance);
    
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
