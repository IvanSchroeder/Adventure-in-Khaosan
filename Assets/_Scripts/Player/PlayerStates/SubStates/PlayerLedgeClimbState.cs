using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLedgeClimbState : PlayerState {
    private Vector2 detectedPos;
    private Vector2 cornerPos;
    private Vector2 startPos;
    private Vector2 stopPos;

    private bool hasSpace;

    public PlayerLedgeClimbState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void AnimationFinishTrigger() {
        base.AnimationFinishTrigger();

        player.Anim.SetBool("climbLedge", false);
    }

    public override void AnimationTrigger() {
        base.AnimationTrigger();

        isHanging = true;
    }

    public override void Enter() {
        base.Enter();

        if (playerData.autoWallGrab && yInput != 0)
            yInput = 0;

        hasSpace = true;
        // player.Rb.gravityScale = 0f;
        player.SetVelocityX(0f);
        player.SetVelocityY(0f);
        player.transform.position = detectedPos;
        cornerPos = player.GetCornerPosition();

        startPos.Set(cornerPos.x - (player.FacingDirection * playerData.startOffset.x), cornerPos.y - playerData.startOffset.y);
        stopPos.Set(cornerPos.x + (player.FacingDirection * playerData.stopOffset.x), cornerPos.y + playerData.stopOffset.y);

        player.transform.position = startPos;
    }

    public override void Exit() {
        base.Exit();

        // player.Rb.gravityScale = playerData.defaultGravityScale;
        isHanging = false;

        player.Anim.SetBool("ledgeHoldBack", false);

        if (isClimbing) {
            player.transform.position = stopPos + (Vector2.up * 0.05f);
            isClimbing = false;
            player.SetVelocityX(0f);
            // player.SetVelocityY(0f);
        }
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        startPos.Set(cornerPos.x - (player.FacingDirection * playerData.startOffset.x), cornerPos.y - playerData.startOffset.y);
        stopPos.Set(cornerPos.x + (player.FacingDirection * playerData.stopOffset.x), cornerPos.y + playerData.stopOffset.y);
        player.detectedPos = detectedPos;
        player.cornerPos = cornerPos;
        player.startPos = startPos;
        player.stopPos = stopPos;

        if (isAnimationFinished) {
            hasSpace = CheckForSpace();

            if (hasSpace) {
                player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig);
                // player.CalculateColliderHeight(playerData.standColliderHeight);
                stateMachine.ChangeState(player.IdleState);
            }
            else {
                player.SetColliderParameters(player.MovementCollider, playerData.crouchColliderConfig);
                // player.CalculateColliderHeight(playerData.crouchColliderHeight);
                stateMachine.ChangeState(player.CrouchIdleState);
            }
        }
        else {
            player.transform.position = startPos;

            if (isHanging && !isClimbing) {
                if (xInput == -player.FacingDirection) player.Anim.SetBool("ledgeHoldBack", true);
                else player.Anim.SetBool("ledgeHoldBack", false);
            }

            if (isHanging && !isClimbing && xInput == player.FacingDirection) {
                // CheckForSpace();
                isClimbing = true;
                player.Anim.SetBool("climbLedge", true);
            }
            else if (isHanging && !isClimbing && jumpInput && xInput == -player.FacingDirection) {
                player.WallJumpState.GetWallJumpDirection(isTouchingWall);
                stateMachine.ChangeState(player.WallJumpState);
            }
            else if (isHanging && !isClimbing && jumpInput && xInput == 0) {
                coyoteTime = false;
                player.transform.position += Vector3.up * 0.2f;
                stateMachine.ChangeState(player.JumpState);
            }
            else if (isHanging && !isClimbing && !grabInput && yInput == -1) {
                player.transform.position += Vector3.down * 0.2f;
                stateMachine.ChangeState(player.AirborneState);
            }
            else if (isHanging && !isClimbing && grabInput && yInput == -1) {
                player.transform.position += Vector3.down * 0.2f;
                stateMachine.ChangeState(player.WallClimbState);
            }
        }

    }

    public void SetDetectedPosition(Vector2 pos) => detectedPos = pos;

    private bool CheckForSpace() {
        bool space = !Physics2D.Raycast(stopPos + Vector2.up * 0.2f, Vector2.up, 1f, playerData.wallLayer);
        if (space) Debug.DrawRay(stopPos + Vector2.up * 0.2f, Vector2.up * 1f, Color.green, 1f);
        else Debug.DrawRay(stopPos + Vector2.up * 0.2f, Vector2.up * 1f, Color.red, 1f);
        return space;
    }
}
