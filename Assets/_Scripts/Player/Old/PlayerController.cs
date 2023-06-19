using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour {
//     public enum PlayerState {
//         Idle,
//         Running,
//         Airborne,
//         Jumping,
//         Falling,
//         Rolling,
//         Sliding,
//         Hurt
//     }

//     public enum GroundState {
//         Grounded,
//         Airborne
//     }

//     [Header("Actions Check")]
//     [SerializeField, /*ReadOnly*/] private bool isFacingRight = true;
//     [SerializeField, /*ReadOnly*/] private bool isMoving;
//     [SerializeField, /*ReadOnly*/] private bool isMovingRight;
//     [SerializeField, /*ReadOnly*/] private bool isGrounded;
//     [SerializeField, /*ReadOnly*/] private bool isCrouching;
//     [SerializeField, /*ReadOnly*/] private bool isTouchingWall;
//     [SerializeField, /*ReadOnly*/] private bool isTouchingLeftWall;
//     [SerializeField, /*ReadOnly*/] private bool isTouchingRightWall;
//     [SerializeField, /*ReadOnly*/] private bool isPushingLeftWall;
//     [SerializeField, /*ReadOnly*/] private bool isPushingRightWall;
//     [SerializeField, /*ReadOnly*/] private bool isWallSliding;
//     [SerializeField, /*ReadOnly*/] private bool isWallJumping;
//     [SerializeField, /*ReadOnly*/] private bool isJumping;
//     [SerializeField, /*ReadOnly*/] private bool isFalling;
//     [SerializeField, /*ReadOnly*/] private bool isJumpCut;

//     [Header("Gizmos")]
//     [SerializeField] private bool drawGizmos;
//     [Space(5)]
//     [SerializeField] private Color airborneGizmosColor;
//     [SerializeField] private Color groundedGizmosColor;
//     [SerializeField] private Vector2 groundCheckOffset;
//     [SerializeField] private Vector2 groundCheckSize;
//     [Space(5)]
//     [SerializeField] private Color walledGizmosColor;
//     [SerializeField] private Color unwalledGizmosColor;
//     [SerializeField] private Vector2 wallCheckOffset;
//     [SerializeField] private Vector2 wallCheckSize;
//     [Space(5)]
//     [SerializeField] private Vector2 headCheckOffset;
//     [SerializeField] private Vector2 slopeTopCheckOffset;
//     [SerializeField] private Vector2 slopeBaseCheckOffset;
//     [SerializeField] private Vector2 ledgeCheckOffset;
//     [SerializeField] private Vector2 ledgeCheckSize;

//     [SerializeField] private float lastJumpTime;
//     [SerializeField] private float lastGroundedTime;
//     [SerializeField] private float lastWallTime;
//     [SerializeField] private float lastLeftWallTime;
//     [SerializeField] private float lastRightWallTime;
//     [SerializeField] private float wallJumpStartTime;

//     public static event UnityAction<int> StateChangeEvent = delegate { };
//     public static event UnityAction<bool> TurnEvent = delegate { };

//     public InputController inputController;
//     private Rigidbody2D rb;
//     [SerializeField] private LayerMask groundLayer;
//     [SerializeField] private PlayerData playerData;

//     [SerializeField] private PlayerState playerState;
//     [SerializeField] private GroundState groundState;

//     [Header("Movement")]
//     [SerializeField] private bool enableFriction;
//     [SerializeField] private float frictionAmount;

//     public void OnEnable() {
//         inputController.JumpKeyDownEvent += OnJumpInput;
//         inputController.JumpKeyUpEvent += OnJumpUpInput;
//     }

//     public void OnDisable() {
//         inputController.JumpKeyDownEvent -= OnJumpInput;
//         inputController.JumpKeyUpEvent -= OnJumpUpInput;
//     }

//     private void Awake() {
//     }

//     private void Start() {
//         if (rb == null) rb = GetComponent<Rigidbody2D>();
//     }

//     public void Update() {
//         UpdatePlayerState();
//         UpdateTimers();
//         HandleInputs();
//         CheckCollisions();
//         CheckJumps();
//         CheckWallSlide();
//         HandleGravity();
//     }

//     public void FixedUpdate() {
//         if (isWallJumping)
//             Run(playerData.wallJumpRunLerp);
//         else
//             Run(1);

//         if (isWallSliding)
//             Slide();
//     }

//     private void UpdatePlayerState() {
//         int targetAnimationState = 0;

//         switch (playerState) {
//             case PlayerState.Idle:
//                 targetAnimationState = playerData.Idle;
//             break;
//             case PlayerState.Running:
//                 targetAnimationState = playerData.Run;
//             break;
//             case PlayerState.Airborne:
//                 targetAnimationState = playerData.Airborne;
//             break;
//             // case PlayerState.Jumping:
//             //     targetAnimationState = playerData.Jump;
//             // break;
//             // case PlayerState.Falling:
//             //     targetAnimationState = playerData.Fall;
//             // break;
//             case PlayerState.Rolling:
//                 targetAnimationState = playerData.Roll;
//             break;
//             case PlayerState.Sliding:
//                 targetAnimationState = playerData.Slide;
//             break;
//             case PlayerState.Hurt:
//                 targetAnimationState = playerData.Hurt;
//             break;
//         }

//         StateChangeEvent?.Invoke(targetAnimationState);
//     }

//     private void UpdateTimers() {
//         lastJumpTime -= Time.deltaTime;
//         lastWallTime -= Time.deltaTime;
//         lastLeftWallTime -= Time.deltaTime;
//         lastRightWallTime -= Time.deltaTime;
//     }

//     private void HandleInputs() {
//         isMoving = inputController.horizontalInput == 0 ? false : true;
        
//         if (isMoving) {
//             if (inputController.horizontalInput > 0) isMovingRight = true;
//             else if (inputController.horizontalInput < 0) isMovingRight = false;
//             CheckDirectionToFace(inputController.horizontalInput > 0);
//         }
//         else isMovingRight = false;

//         if (inputController.verticalInput < 0) isCrouching = true;
//         else isCrouching = false;
//     }

//     private void CheckCollisions() {
//         isGrounded = Physics2D.OverlapBox(transform.position.ToVector2() + groundCheckOffset, groundCheckSize, 0f, groundLayer);
//         groundState = isGrounded ? GroundState.Grounded : GroundState.Airborne;

//         switch (groundState) {
//             case GroundState.Grounded:
//                 lastGroundedTime = playerData.coyoteTime;
//                 if (isMoving) ChangeState(PlayerState.Running);
//                 else ChangeState(PlayerState.Idle);

//                 if (isCrouching && !isMoving) {
//                     // Crouch
//                     // raycast and detect platform;
//                 }
//             break;
//             case GroundState.Airborne:
//                 if (!isWallSliding && !isWallJumping) ChangeState(PlayerState.Airborne);
//                 lastGroundedTime -= Time.deltaTime;
//                 lastJumpTime -= Time.deltaTime;
//                 wallJumpStartTime -= Time.deltaTime;
//             break;
//         }
//         // RaycastHit2D headHit;
        
//         isTouchingLeftWall
//             = (Physics2D.OverlapBox(transform.position.ToVector2() + new Vector2(-wallCheckOffset.x, wallCheckOffset.y), wallCheckSize, 0f, groundLayer));
//         isTouchingRightWall
//             = (Physics2D.OverlapBox(transform.position.ToVector2() + new Vector2(wallCheckOffset.x, wallCheckOffset.y), wallCheckSize, 0f, groundLayer));
//         isTouchingWall = isTouchingLeftWall || isTouchingRightWall;

//         isPushingLeftWall = isMoving && !isMovingRight && isTouchingLeftWall ? true : false;
//         isPushingRightWall = isMoving && isMovingRight && isTouchingRightWall ? true : false;

//         if (isPushingLeftWall) lastLeftWallTime = playerData.coyoteTime;
//         if (isPushingRightWall) lastRightWallTime = playerData.coyoteTime;
//         lastWallTime = Mathf.Max(lastLeftWallTime, lastRightWallTime);
//     }

//     private void CheckJumps() {
//         if (isJumping && rb.velocity.y < 0) {
//             isJumping = false;
//             if (!isWallJumping) isFalling = true;
//         }

//         if (isWallJumping && wallJumpStartTime < 0) {
//             isWallJumping = false;
//         }

//         if (lastGroundedTime > 0 && !isJumping && !isWallJumping) {
//             isJumpCut = false;
//             if (!isJumping) isFalling = false;
//         }

//         if (CanJump()) {
//             Jump(Vector2.up);
//         }
//         else if (CanWallJump()) {
//             if (isPushingLeftWall) WallJump(Vector2.right);
//             else if (isPushingRightWall) WallJump(Vector2.left);
//         }

//         if (rb.velocity.y < 0) {
//             isJumping = false;
//             isFalling = true;
//         }

//         if (isWallSliding) {
//             isFalling = false;
//         }
//     }

//     private void CheckWallSlide() {
//         if (CanSlide() && ((lastLeftWallTime > 0 && inputController.horizontalInput < 0) || (lastRightWallTime > 0 && inputController.horizontalInput > 0)))
//             isWallSliding = true;
//         else
//             isWallSliding = false;
//     }

//     private void HandleGravity() {
//         float gravityScale = 1f;

//         if (isWallSliding) {
//             gravityScale = 0f;
//         }
//         else if (rb.velocity.y < 0 && inputController.verticalInput < 0) {
//             gravityScale = playerData.gravityScale * playerData.fastFallGravityMult;
//             rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -playerData.maxFastFallSpeed));
//         }
//         else if (isJumpCut) {
//             gravityScale = playerData.gravityScale * playerData.jumpCutGravityMult;
//             rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -playerData.maxFallSpeed));
//         }
//         else if ((isJumping || isFalling || isWallJumping) && Mathf.Abs(rb.velocity.y) < playerData.jumpHangTimeThreshold) {
//             gravityScale = playerData.gravityScale * playerData.jumpHangGravityMult;
//         }
//         else if (rb.velocity.y < 0) {
//             gravityScale = playerData.gravityScale * playerData.fallGravityMult;
//             rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -playerData.maxFallSpeed));
//         }
//         else gravityScale = playerData.gravityScale;

//         if (rb.velocity.y > playerData.maxAscendantSpeed) {
//             rb.velocity = new Vector2(rb.velocity.x, Mathf.Min(rb.velocity.y, playerData.maxAscendantSpeed));
//         }

//         rb.gravityScale = gravityScale;

//         playerData.yVelocity = rb.velocity.y;
//     }

// #region INPUT CALLBACKS
//     public void OnJumpInput() {
//         lastJumpTime = playerData.jumpBufferTime;
//     }

//     public void OnJumpUpInput() {
//         if (CanJumpCut()) {
//             isJumpCut = true;
//         }
//     }
// #endregion
    
// #region GENERAL METHODS
//     public void ChangeState(PlayerState state) {
//         if (state == playerState) return;

//         playerState = state;
//         // StateChangeEvent?.Invoke((int)state);
//     }
// #endregion

// #region RUN METHODS
//     private void Run(float lerpAmount) {
//         float targetSpeed = inputController.horizontalInput * playerData.maxRunSpeed;
//         targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed, lerpAmount);

//     #region Calculate AccelRate
//         float accelRate;

//         if (lastGroundedTime > 0) accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? playerData.runAccelAmount : playerData.runDeccelAmount;
//         else accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? playerData.runAccelAmount * playerData.accelInAir : playerData.runDeccelAmount * playerData.deccelInAir;
//     #endregion

//     #region Add Bonus Jump Apex Acceleration
//         if ((isJumping || isFalling || isWallJumping) && Mathf.Abs(rb.velocity.y) < playerData.jumpHangTimeThreshold) {
//             accelRate *= playerData.jumpHangAccelerationMult;
//             targetSpeed *= playerData.jumpHangMaxSpeedMult;
//         }
//     #endregion

//     #region Conserve Momentum
//         if (playerData.conserveMomentum && Mathf.Abs(rb.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(rb.velocity.x) == Mathf.Sign(targetSpeed)
//         && Mathf.Abs(targetSpeed) > 0.01f && lastGroundedTime < 0) {
//             accelRate = 0;
//         }
//     #endregion

//         float speedDiff = targetSpeed - rb.velocity.x;
//         float movement = speedDiff * accelRate;

//         rb.AddForce(movement * Vector2.right, ForceMode2D.Force);

//         if (!enableFriction) return;
//         if (isGrounded && Mathf.Abs(inputController.horizontalInput) < 0.01f) {
//             float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(frictionAmount));
//             amount *= Mathf.Sign(rb.velocity.x);

//             rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
//         }
//     }

//     private void Turn() {
//         TurnEvent?.Invoke(isFacingRight);
//         isFacingRight = !isFacingRight;
//     }
// #endregion

// #region JUMP METHODS
//     private void Jump(Vector2 jumpDirection) {
//         isJumping = true;
//         isFalling = false;
//         isWallJumping = false;
//         isJumpCut = false;
//         lastJumpTime = 0;
//         lastGroundedTime = 0;

//         float force = playerData.jumpForce;
//         if (rb.velocity.y < 0) force -= rb.velocity.y;

//         rb.AddForce(jumpDirection * force, ForceMode2D.Impulse);
//     }

//     private void WallJump(Vector2 jumpDirection) {
//         isWallJumping = true;
//         isJumping = false;
//         isJumpCut = false;
//         isFalling = false;
//         wallJumpStartTime = playerData.wallJumpTime;
//         lastJumpTime = 0;
//         lastGroundedTime = 0;
//         lastLeftWallTime = 0;
//         lastRightWallTime = 0;

//         Vector2 force = playerData.wallJumpDirectionOffAngle * playerData.wallJumpForce;
//         force.x *= jumpDirection.x;

//         if (Mathf.Sign(rb.velocity.x) != Mathf.Sign(force.x))
//             force.x -= rb.velocity.x;
        
//         if (rb.velocity.y < 0)
//             force.y -= rb.velocity.y;
        
//         rb.AddForce(force, ForceMode2D.Impulse);

//         if (playerData.doTurnOnWallJump) Turn();
//     }

//     private void Slide() {
//         float speedDif = -playerData.slideSpeed - rb.velocity.y;
//         float movement = speedDif * playerData.slideAccel;

//         movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));
//         rb.AddForce(movement * Vector2.up);

//         ChangeState(PlayerState.Sliding);
//     }
// #endregion

// #region CHECK METHODS
//         public void CheckDirectionToFace(bool isMovingRight) {
//             if (isMovingRight != isFacingRight) Turn();
//         }

//         private bool CanJump() {
//             return lastJumpTime > 0 && !isWallJumping && !isJumping && isGrounded;
//         }

//         private bool CanWallJump() {
//             return lastJumpTime > 0 && lastWallTime > 0 && lastGroundedTime <= 0 && !isWallJumping;
//         }

//         private bool CanJumpCut() {
//             return isJumping && !isWallJumping && rb.velocity.y > 0;
//         }

//         private bool CanSlide() {
//             if (lastWallTime > 0 && !isJumping && !isWallJumping && !isGrounded)
//             return true;
//             else
//                 return false;
//         }
// #endregion

// #region EDITOR METHODS
//     private void OnDrawGizmosSelected() {
//         if (!drawGizmos) return;
//         if (isGrounded) Gizmos.color = groundedGizmosColor;
//         else Gizmos.color = airborneGizmosColor;
//         Gizmos.DrawWireCube(transform.position.ToVector2() + groundCheckOffset, groundCheckSize);

//         if (isTouchingWall) Gizmos.color = walledGizmosColor;
//         else Gizmos.color = unwalledGizmosColor;
//         Gizmos.DrawWireCube(transform.position.ToVector2() + wallCheckOffset * new Vector2(1f, 1f), wallCheckSize);
//         Gizmos.DrawWireCube(transform.position.ToVector2() + wallCheckOffset * new Vector2(-1f, 1f), wallCheckSize);

//         Gizmos.color = Color.white;
//         if (isPushingLeftWall) Gizmos.DrawRay(transform.position.ToVector2() + new Vector2(0f, 0.5f), playerData.wallJumpDirectionOffAngle);
//         if (isPushingRightWall) Gizmos.DrawRay(transform.position.ToVector2() + new Vector2(0f, 0.5f), playerData.wallJumpDirectionOffAngle * new Vector2(-1f, 1f));

//         Gizmos.color = Color.green;
//         Vector2 headCheckPos1 = new Vector2(transform.position.x - headCheckOffset.x, transform.position.y + headCheckOffset.y);
//         Vector2 headCheckPos2 = new Vector2(transform.position.x + headCheckOffset.x, transform.position.y + headCheckOffset.y);
//         Gizmos.DrawLine(headCheckPos1, headCheckPos1 + new Vector2(0f, 0.5f));
//         Gizmos.DrawLine(headCheckPos2, headCheckPos2 + new Vector2(0f, 0.5f));
        
//         Vector2 slopeBaseCheckPos1 = new Vector2(transform.position.x - slopeBaseCheckOffset.x, transform.position.y + slopeBaseCheckOffset.y);
//         Vector2 slopeBaseCheckPos2 = new Vector2(transform.position.x + slopeBaseCheckOffset.x, transform.position.y + slopeBaseCheckOffset.y);
//         Gizmos.DrawLine(slopeBaseCheckPos1, slopeBaseCheckPos1 - new Vector2(0.5f, 0f));
//         Gizmos.DrawLine(slopeBaseCheckPos2, slopeBaseCheckPos2 + new Vector2(0.5f, 0f));
//     }
// #endregion
}
