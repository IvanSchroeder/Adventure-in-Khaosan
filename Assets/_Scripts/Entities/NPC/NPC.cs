using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class NPC : Entity, IInteractable {
    public NPCIdleState IdleState { get; set; }
    public NPCMoveState MoveState { get; set; }
    public NPCInteractedState InteractedState { get; set; }

    [SerializeField] private NPCIdleSOBase NPCIdleSOBase;
    [SerializeField] private NPCMoveSOBase NPCMoveSOBase;
    [SerializeField] private NPCInteractedSOBase NPCInteractedSOBase;

    public NPCIdleSOBase NPCIdleSOBaseInstance { get; set; }
    public NPCMoveSOBase NPCMoveSOBaseInstance { get; set; }
    public NPCInteractedSOBase NPCInteractedSOBaseInstance { get; set; }

    public bool isGrounded { get; set; }
    public bool isOnSolidGround { get; set; }
    public bool isOnSlope { get; set; }
    public bool isOnPlatform { get; set; }
    public bool isAirborne { get; set; }
    public bool isIdle { get; set; }
    public bool isMoving { get; set; }
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
    public bool isInvulnerable { get; set; }
    public bool isInteracted { get; set; }
    public bool isNearPlayer { get; set; }

    [Space(10f), Header("General"), Space(5f)]
    public NPCData npcData;

    [field: SerializeField, Space(10f), Header("Checks"), Space(5f)] public Transform GroundPoint { get; private set; }
    [field: SerializeField] public Transform MidPoint { get; private set; }
    [field: SerializeField] public Transform WallPoint { get; private set; }
    [field: SerializeField] public Transform CeilingPoint { get; private set; }
    [field: SerializeField] public Transform LedgePoint { get; private set; }

    [field: SerializeField] public BoxCollider2D InteractTrigger { get; set; }
    [field: SerializeField] public InteractableSystem InteractableSystem { get; set; }
    [field: SerializeField] public SpriteManager SpriteManager { get; set; }

    public Vector2 spawnPosition;

    private Vector2 workspace;
    private float velocityX;
    private float velocityY;
    private Vector2 velocityXY;

    protected override void Awake() {
        if (Rb.IsNull()) Rb = this.GetComponent<Rigidbody2D>();
        if (Anim.IsNull()) Anim = GetComponentInChildren<Animator>();
        if (Sprite.IsNull()) Sprite = GetComponentInChildren<SpriteRenderer>();
        if (MovementCollider.IsNull()) MovementCollider = GetComponent<BoxCollider2D>();
        if (SpriteManager.IsNull()) SpriteManager = Sprite.GetComponent<SpriteManager>();

        if (NPCIdleSOBase.IsNotNull()) NPCIdleSOBaseInstance = Instantiate(NPCIdleSOBase);
        if (NPCMoveSOBase.IsNotNull()) NPCMoveSOBaseInstance = Instantiate(NPCMoveSOBase);
        if (NPCInteractedSOBase.IsNotNull()) NPCInteractedSOBaseInstance = Instantiate(NPCInteractedSOBase);

        StateMachine = new StateMachine();

        IdleState = new NPCIdleState(this, StateMachine, npcData, "idle");
        MoveState = new NPCMoveState(this, StateMachine, npcData, "move");
        InteractedState = new NPCInteractedState(this, StateMachine, npcData, "interact");
    }
    
    protected override void Start() {
        NPCIdleSOBaseInstance.Initialize(gameObject, this, npcData, IdleState);
        NPCMoveSOBaseInstance.Initialize(gameObject, this, npcData, MoveState);
        NPCInteractedSOBaseInstance.Initialize(gameObject, this, npcData, InteractedState);

        StateMachine.Initialize(IdleState);

        spawnPosition = GroundPoint.transform.position;
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();

        if (npcData.canDetectPlayer) {
            isNearPlayer = Physics2D.CircleCast(MidPoint.position, npcData.playerDetectionRange, default, default, npcData.playerLayer);
            if (isNearPlayer) Debug.Log($"{this.name} is detecting player");
        }
    }

    public override void SetVelocityX(float velocity, float accelAmount = 30f, bool lerpVelocity = false) {
        if (lerpVelocity)
            velocityX = Mathf.MoveTowards(velocityX, velocity, accelAmount * Time.deltaTime);
        else
            velocityX = velocity;
        
        CurrentVelocity = CurrentVelocity.SetXY(velocityX.Clamp(-npcData.maxHorizontalSpeed, npcData.maxHorizontalSpeed), CurrentVelocity.y);
        Rb.velocity = CurrentVelocity;
    }

    public override void SetVelocityY(float velocity, float accelAmount = 30f, bool lerpVelocity = false) {
        if (lerpVelocity)
            velocityY = Mathf.MoveTowards(velocityY, velocity, accelAmount * Time.deltaTime);
        else
            velocityY = velocity;

        CurrentVelocity = CurrentVelocity.SetXY(CurrentVelocity.x, velocityY.Clamp(-npcData.currentFallSpeed, npcData.maxAscendantSpeed));
        Rb.velocity = CurrentVelocity;
    }

    public override void SetVelocity(float velocity, Vector2 angle, int direction, bool resetCurrentVelocity = true) {
        if (resetCurrentVelocity) {
            CurrentVelocity = new Vector2(0f, 0f);
            Rb.velocity = new Vector2(0f, 0f);
        }

        angle.Normalize();
        velocityXY.Set((angle.x.ToInt() * velocity * direction).Clamp(-npcData.maxHorizontalSpeed, npcData.maxHorizontalSpeed), (angle.y.ToInt() * velocity).Clamp(-npcData.defaultFallSpeed, npcData.maxAscendantSpeed));
        CurrentVelocity = velocityXY;
        Rb.velocity = CurrentVelocity;
    }

    public override bool CheckGround(LayerMask mask) {
        return Physics2D.Raycast(GroundPoint.position.ToVector2() + (Vector2.right * npcData.groundCheckOffset.x) + (Vector2.up * npcData.groundCheckOffset.y), Vector2.down, npcData.groundCheckDistance, mask)
            || Physics2D.Raycast(GroundPoint.position.ToVector2() + (Vector2.left * npcData.groundCheckOffset.x) + (Vector2.up * npcData.groundCheckOffset.y), Vector2.down, npcData.groundCheckDistance, mask)
            || Physics2D.Raycast(GroundPoint.position.ToVector2() + (Vector2.up * npcData.groundCheckOffset.y), Vector2.down, npcData.groundCheckDistance, mask);
    }

    public override bool CheckWall() {
        return Physics2D.Raycast(WallPoint.position.ToVector2() + (Vector2.up * npcData.wallCheckOffset.y), Vector2.right * FacingDirection, npcData.wallCheckDistance, npcData.wallLayer);
    }

    public override bool CheckBackWall() {
        return Physics2D.Raycast(WallPoint.position.ToVector2() + (Vector2.up * npcData.wallCheckOffset.y), Vector2.right * -FacingDirection, npcData.wallCheckDistance, npcData.wallLayer);
    }

    public override bool CheckLedge() {
        return Physics2D.Raycast(LedgePoint.position.ToVector2() + (Vector2.up * npcData.ledgeCheckOffset.y), Vector2.right * FacingDirection, npcData.ledgeCheckDistance, npcData.wallLayer);
    }

    public override bool CheckLedgeFoot() {
        return Physics2D.Raycast(GroundPoint.position.ToVector2() + (Vector2.right * FacingDirection * npcData.ledgeFootCheckOffset.x) + (Vector2.up * npcData.ledgeFootCheckOffset.y), Vector2.down, npcData.ledgeFootCheckDistance, npcData.solidsLayer);
    }

    public bool drawGizmos;

    private void OnDrawGizmos() {
        if (!drawGizmos) return;
        Gizmos.color = Color.green;
        // Ground Check Raycasts
        Gizmos.DrawRay(GroundPoint.position.ToVector2() + (Vector2.right * npcData.groundCheckOffset.x) + (Vector2.up * npcData.groundCheckOffset.y), Vector2.down * npcData.groundCheckDistance);
        Gizmos.DrawRay(GroundPoint.position.ToVector2() + (Vector2.left * npcData.groundCheckOffset.x) + (Vector2.up * npcData.groundCheckOffset.y), Vector2.down * npcData.groundCheckDistance);
        Gizmos.DrawRay(GroundPoint.position.ToVector2() + (Vector2.up * npcData.groundCheckOffset.y), Vector2.down * npcData.groundCheckDistance);

        // Wall Check Raycasts
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(WallPoint.position.ToVector2() + (Vector2.up * npcData.wallCheckOffset.y), Vector2.right * FacingDirection * npcData.wallCheckDistance);
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(WallPoint.position.ToVector2() + (Vector2.up * npcData.wallCheckOffset.y), Vector2.right * -FacingDirection * npcData.wallCheckDistance);
        
        // Ledge Check Raycasts
        // Wall
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(LedgePoint.position.ToVector2() + (Vector2.right * FacingDirection * npcData.ledgeCheckOffset.x) + (Vector2.up * npcData.ledgeCheckOffset.y), Vector2.right * FacingDirection * npcData.ledgeCheckDistance);

        // Foot
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(GroundPoint.position.ToVector2() + (Vector2.right * FacingDirection * npcData.ledgeFootCheckOffset.x) + (Vector2.up * npcData.ledgeFootCheckOffset.y), Vector2.down * npcData.ledgeFootCheckDistance);

        if (npcData.hasLimitWander) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnPosition, npcData.wanderLimitRadius);
        }

        if (npcData.canDetectPlayer) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(MidPoint.position, npcData.playerDetectionRange);
        }
    }
}
