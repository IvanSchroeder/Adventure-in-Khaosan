using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public class Enemy : Entity, IDamageable, IDamageDealer {
    public EnemyIdleState IdleState { get; set; }
    public EnemyPatrolState PatrolState { get; set; }
    public EnemyChaseState ChaseState { get; set; }
    public EnemyDeathState DeathState { get; set; }

    [SerializeField] private EnemyIdleSOBase EnemyIdleSOBase;
    [SerializeField] private EnemyPatrolSOBase EnemyPatrolSOBase;
    [SerializeField] private EnemyChaseSOBase EnemyChaseSOBase;
    [SerializeField] private EnemyDeathSOBase EnemyDeathSOBase;

    public EnemyIdleSOBase EnemyIdleSOBaseInstance { get; set; }
    public EnemyPatrolSOBase EnemyPatrolSOBaseInstance { get; set; }
    public EnemyChaseSOBase EnemyChaseSOBaseInstance { get; set; }
    public EnemyDeathSOBase EnemyDeathSOBaseInstance { get; set; }

    public bool isGrounded { get; set; }
    public bool isOnSolidGround { get; set; }
    public bool isOnSlope { get; set; }
    public bool isOnPlatform { get; set; }
    public bool isIgnoringPlatforms { get; set; }
    public bool isAirborne { get; set; }
    public bool isIdle { get; set; }
    public bool isMoving { get; set; }
    public bool isRunning { get; set; }
    public bool isRunningAtMaxSpeed { get; set; }
    public bool isChangingDirections { get; set; }
    public bool isJumping { get; set; }
    public bool isAscending { get; set; }
    public bool isFalling { get; set; }
    public bool isTouchingCeiling { get; set; }
    public bool isTouchingWall { get; set; }
    public bool isTouchingBackWall { get; set; }
    public bool hasTouchedWall { get; set; }
    public bool hasTouchedWallBack { get; set; }
    public bool isTouchingLedge { get; set; }
    public bool isTouchingLedgeWithFoot { get; set; }
    public bool isDamaged { get; set; }
    public bool isDead { get; set; }
    public bool isInvulnerable { get; set; }
    public bool isAbilityDone { get; set; }

    public bool isPatroling { get; set; }
    public bool isChasing { get; set; }

    [Space(10f), Header("General"), Space(5f)]
    public EnemyData enemyData;

    [field: SerializeField, Space(10f), Header("Damageable"), Space(5f)] public HealthSystem HealthSystem { get; set; }
    [field: SerializeField] public BoxCollider2D HitboxTrigger { get; set; }
    [field: SerializeField] public SpriteManager SpriteManager { get; set; }

    [field: SerializeField, Space(10f), Header("Checks"), Space(5f)] public Transform GroundPoint { get; private set; }
    [field: SerializeField] public Transform MidPoint { get; private set; }
    [field: SerializeField] public Transform WallPoint { get; private set; }
    [field: SerializeField] public Transform CeilingPoint { get; private set; }
    [field: SerializeField] public Transform LedgePoint { get; private set; }
    
    [field: SerializeField] public Collider2D DamageHitbox { get; set; }
    [field: SerializeField] public LayerMask DamageablesLayers { get; set; }
    [field: SerializeField] public int DamageDealerLayer { get; set; }
    [field: SerializeField] public float DamageAmount { get; set; }
    [field: SerializeField] public IntSO DamageInHearts { get; set; }

    private Vector2 workspace;
    private float velocityX;
    private float velocityY;
    private Vector2 velocityXY;

    public Action<Enemy> OnEnemySpawn;
    public Action<Enemy> OnEnemyDeath;
    
    protected override void Awake() {
        if (Rb.IsNull()) Rb = this.GetComponent<Rigidbody2D>();
        if (Anim.IsNull()) Anim = GetComponentInChildren<Animator>();
        if (Sprite.IsNull()) Sprite = GetComponentInChildren<SpriteRenderer>();
        if (MovementCollider.IsNull()) MovementCollider = GetComponent<BoxCollider2D>();
        if (HealthSystem.IsNull()) HealthSystem = GetComponentInChildren<HealthSystem>();
        if (HitboxTrigger.IsNull()) HitboxTrigger = HealthSystem.GetComponent<BoxCollider2D>();
        if (SpriteManager.IsNull()) SpriteManager = Sprite.GetComponent<SpriteManager>();

        if (EnemyIdleSOBase.IsNotNull()) EnemyIdleSOBaseInstance = Instantiate(EnemyIdleSOBase);
        if (EnemyPatrolSOBase.IsNotNull()) EnemyPatrolSOBaseInstance = Instantiate(EnemyPatrolSOBase);
        if (EnemyChaseSOBase.IsNotNull()) EnemyChaseSOBaseInstance = Instantiate(EnemyChaseSOBase);
        if (EnemyDeathSOBase.IsNotNull()) EnemyDeathSOBaseInstance = Instantiate(EnemyDeathSOBase);

        StateMachine = new StateMachine();

        IdleState = new EnemyIdleState(this, StateMachine, enemyData, "idle");
        PatrolState = new EnemyPatrolState(this, StateMachine, enemyData, "move");
        ChaseState = new EnemyChaseState(this, StateMachine, enemyData, "move");
        DeathState = new EnemyDeathState(this, StateMachine, enemyData, "dead");

        if (DamageHitbox.IsNull()) DamageHitbox = this.GetComponentInHierarchy<Collider2D>();

        DamageDealerLayer = this.gameObject.layer;
    }

    protected override void Start() {
        EnemyIdleSOBaseInstance?.Initialize(gameObject, this, enemyData, IdleState);
        EnemyPatrolSOBaseInstance?.Initialize(gameObject, this, enemyData, PatrolState);
        EnemyChaseSOBaseInstance?.Initialize(gameObject, this, enemyData, ChaseState);
        EnemyDeathSOBaseInstance?.Initialize(gameObject, this, enemyData, DeathState);

        StateMachine.Initialize(IdleState);
    }

    public void OnTriggerEnter2D(Collider2D collision) {
        IDamageable damagedEntity = collision.GetComponentInHierarchy<IDamageable>();

        if (damagedEntity.IsNull()) return;

        HealthSystem entityHealthSystem = damagedEntity.HealthSystem;

        OnEntityDamagedEventArgs entityArgs = new OnEntityDamagedEventArgs(this, DamageAmount, DamageInHearts, collision.ClosestPoint(this.transform.position), entityHealthSystem.DamagedFlash);
        damagedEntity.Damage(this, entityArgs);

        Debug.Log($"{this.gameObject.name} damaged {entityHealthSystem.Entity.name}");
    }

    public override void Damage(object sender, OnEntityDamagedEventArgs entityDamagedArgs) {}

    public override void SetVelocityX(float velocity, float accelAmount = 30f, bool lerpVelocity = false) {
        if (lerpVelocity)
            velocityX = Mathf.MoveTowards(velocityX, velocity, accelAmount * Time.deltaTime);
        else
            velocityX = velocity;
        
        CurrentVelocity = CurrentVelocity.SetXY(velocityX.Clamp(-enemyData.maxHorizontalSpeed, enemyData.maxHorizontalSpeed), CurrentVelocity.y);
        Rb.velocity = CurrentVelocity;
    }

    public override void SetVelocityY(float velocity, float accelAmount = 30f, bool lerpVelocity = false) {
        if (lerpVelocity)
            velocityY = Mathf.MoveTowards(velocityY, velocity, accelAmount * Time.deltaTime);
        else
            velocityY = velocity;

        CurrentVelocity = CurrentVelocity.SetXY(CurrentVelocity.x, velocityY.Clamp(-enemyData.currentFallSpeed, enemyData.maxAscendantSpeed));
        Rb.velocity = CurrentVelocity;
    }

    public override void SetVelocity(float velocity, Vector2 angle, int direction, bool resetCurrentVelocity = true) {
        if (resetCurrentVelocity) {
            CurrentVelocity = new Vector2(0f, 0f);
            Rb.velocity = new Vector2(0f, 0f);
        }

        angle.Normalize();
        velocityXY.Set((angle.x.ToInt() * velocity * direction).Clamp(-enemyData.maxHorizontalSpeed, enemyData.maxHorizontalSpeed), (angle.y.ToInt() * velocity).Clamp(-enemyData.defaultFallSpeed, enemyData.maxAscendantSpeed));
        CurrentVelocity = velocityXY;
        Rb.velocity = CurrentVelocity;
    }

    public override bool CheckGround(LayerMask mask) {
        return Physics2D.Raycast(GroundPoint.position.ToVector2() + (Vector2.right * enemyData.groundCheckOffset.x) + (Vector2.up * enemyData.groundCheckOffset.y), Vector2.down, enemyData.groundCheckDistance, mask)
            || Physics2D.Raycast(GroundPoint.position.ToVector2() + (Vector2.left * enemyData.groundCheckOffset.x) + (Vector2.up * enemyData.groundCheckOffset.y), Vector2.down, enemyData.groundCheckDistance, mask)
            || Physics2D.Raycast(GroundPoint.position.ToVector2() + (Vector2.up * enemyData.groundCheckOffset.y), Vector2.down, enemyData.groundCheckDistance, mask);
    }

    public override bool CheckWall() {
        return Physics2D.Raycast(WallPoint.position.ToVector2() + (Vector2.up * enemyData.wallCheckOffset.y), Vector2.right * FacingDirection, enemyData.wallCheckDistance, enemyData.wallLayer);
    }

    public override bool CheckBackWall() {
        return Physics2D.Raycast(WallPoint.position.ToVector2() + (Vector2.up * enemyData.wallCheckOffset.y), Vector2.right * -FacingDirection, enemyData.wallCheckDistance, enemyData.wallLayer);
    }

    public override bool CheckLedge() {
        return Physics2D.Raycast(LedgePoint.position.ToVector2() + (Vector2.up * enemyData.ledgeCheckOffset.y), Vector2.right * FacingDirection, enemyData.ledgeCheckDistance, enemyData.wallLayer);
    }

    public override bool CheckLedgeFoot() {
        return Physics2D.Raycast(GroundPoint.position.ToVector2() + (Vector2.right * FacingDirection * enemyData.ledgeFootCheckOffset.x) + (Vector2.up * enemyData.ledgeFootCheckOffset.y), Vector2.down, enemyData.ledgeFootCheckDistance, enemyData.solidsLayer);
    }

    public bool drawGizmos;

    private void OnDrawGizmos() {
        if (!drawGizmos) return;
        Gizmos.color = Color.green;
        // Ground Check Raycasts
        Gizmos.DrawRay(GroundPoint.position.ToVector2() + (Vector2.right * enemyData.groundCheckOffset.x) + (Vector2.up * enemyData.groundCheckOffset.y), Vector2.down * enemyData.groundCheckDistance);
        Gizmos.DrawRay(GroundPoint.position.ToVector2() + (Vector2.left * enemyData.groundCheckOffset.x) + (Vector2.up * enemyData.groundCheckOffset.y), Vector2.down * enemyData.groundCheckDistance);
        Gizmos.DrawRay(GroundPoint.position.ToVector2() + (Vector2.up * enemyData.groundCheckOffset.y), Vector2.down * enemyData.groundCheckDistance);

        // Wall Check Raycasts
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(WallPoint.position.ToVector2() + (Vector2.up * enemyData.wallCheckOffset.y), Vector2.right * FacingDirection * enemyData.wallCheckDistance);
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(WallPoint.position.ToVector2() + (Vector2.up * enemyData.wallCheckOffset.y), Vector2.right * -FacingDirection * enemyData.wallCheckDistance);
        
        // Ledge Check Raycasts
        // Wall
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(LedgePoint.position.ToVector2() + (Vector2.right * FacingDirection * enemyData.ledgeCheckOffset.x) + (Vector2.up * enemyData.ledgeCheckOffset.y), Vector2.right * FacingDirection * enemyData.ledgeCheckDistance);

        // Foot
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(GroundPoint.position.ToVector2() + (Vector2.right * FacingDirection * enemyData.ledgeFootCheckOffset.x) + (Vector2.up * enemyData.ledgeFootCheckOffset.y), Vector2.down * enemyData.ledgeFootCheckDistance);
    }
}
