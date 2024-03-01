using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCState : State {
    protected NPC npc;
    protected NPCData npcData;

    public NPCState(NPC npc, StateMachine stateMachine, NPCData npcData, string animBoolName) {
        Init(npc, stateMachine, npcData, animBoolName);
    }

    public void Init(NPC npc, StateMachine sM, NPCData npcData, string animBoolName) {
        entity = npc;
        stateMachine = sM;
        this.npcData = npcData;
        this.animBoolName = animBoolName;

        this.npc = npc;
    }

    public override void Enter() {
        base.Enter();
        CheckRaycasts();
        CheckVerticalMovement();
        UpdateNPCStates();
    }

    public override void Exit() {
        base.Exit();
        CheckRaycasts();
        CheckVerticalMovement();
        UpdateNPCStates();
    }

    public override void LogicUpdate() {
        base.LogicUpdate();
        CheckVerticalMovement();
        UpdateNPCStates();
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();
        CheckRaycasts();
    }

    public void CheckVerticalMovement() {
        npc.isAscending = npc.CheckAscending() && !npc.isGrounded;
        npc.isFalling = npc.CheckFalling() && !npc.isGrounded;
    }

    public void CheckRaycasts() {
        npc.isGrounded = npc.CheckGround(npcData.groundLayer);
        npc.isOnSolidGround = npc.CheckGround(npcData.solidsLayer);
        npc.isAirborne = !npc.CheckGround(npcData.groundLayer);
        npc.isOnPlatform = npc.CheckGround(npcData.platformLayer);
        npc.isTouchingWall = npc.CheckWall();
        npc.isTouchingBackWall = npc.CheckBackWall();
        npc.isTouchingLedge = npc.CheckLedge();
        npc.isTouchingLedgeWithFoot = npc.CheckLedgeFoot();
    }

    public void UpdateNPCStates() {
        npcData.currentVelocity = npc.CurrentVelocity;
        npcData.facingDirection = npc.FacingDirection == 1 ? Direction.Right : Direction.Left;
        npcData.currentGravityScale = npc.Rb.gravityScale;
        npcData.currentLayer = LayerMask.LayerToName(npc.gameObject.layer);

        npcData.isGrounded = npc.isGrounded;
        npcData.isOnSolidGround = npc.isOnSolidGround ;
        npcData.isOnSlope = npc.isOnSlope;
        npcData.isOnPlatform = npc.isOnPlatform;
        npcData.isAirborne = npc.isAirborne;
        npcData.isIdle = npc.isIdle;
        npcData.isMoving = npc.isMoving;
        npcData.isChangingDirections = npc.isChangingDirections;
        npcData.isJumping = npc.isJumping;
        npcData.isAscending = npc.isAscending;
        npcData.isFalling = npc.isFalling;
        npcData.isTouchingCeiling = npc.isTouchingCeiling;
        npcData.isTouchingWall = npc.isTouchingWall;
        npcData.isTouchingBackWall = npc.isTouchingBackWall;
        npcData.hasTouchedWall = npc.hasTouchedWall;
        npcData.hasTouchedWallBack = npc.hasTouchedWallBack;
        npcData.isTouchingLedge = npc.isTouchingLedge;
        npcData.isTouchingLedgeWithFoot = npc.isTouchingLedgeWithFoot;
        npcData.isInvulnerable = npc.isInvulnerable;
        npcData.isInteracted = npc.isInteracted;
        npcData.isNearPlayer = npc.isNearPlayer;
    }
}
