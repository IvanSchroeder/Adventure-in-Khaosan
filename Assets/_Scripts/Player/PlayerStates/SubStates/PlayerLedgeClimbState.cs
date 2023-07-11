using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerLedgeClimbState : PlayerState {
    private Vector2 detectedPos;
    private Vector2 cornerPos;
    private Vector2 startPos;
    private Vector2 stopPos;

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

        player.SetVelocityX(0f);
        player.SetVelocityY(0f);
        player.transform.position = detectedPos;
        cornerPos = player.GetCornerPosition();

        startPos.Set(cornerPos.x - (player.FacingDirection * playerData.startOffset.x), cornerPos.y - playerData.startOffset.y);
        stopPos.Set(cornerPos.x + (player.FacingDirection * playerData.stopOffset.x), cornerPos.y + playerData.stopOffset.y);

        player.transform.position = startPos;

        hasSpace = player.CheckForSpace(stopPos + Vector2.up * 0.05f);

        player.CameraTarget.SetTargetPosition(Vector3.zero, 0f, true);
        //player.CameraTarget.OffsetTargetTowards(Vector3.zero, 0f, true);
    }

    public override void Exit() {
        base.Exit();

        isHanging = false;

        player.Anim.SetBool("ledgeHoldBack", false);
        player.CameraTarget.SetTargetPosition(Vector3.zero, 0f, true);
        // player.CameraTarget.OffsetTargetTowards(Vector3.zero, 0f, true);

        if (isClimbing) {
            player.transform.position = stopPos + (Vector2.up * 0.05f);
            isClimbing = false;
            player.SetVelocityX(0f);
            player.SetVelocityY(0f);
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
            if (hasSpace) {
                player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig);
                player.SetColliderParameters(player.HitboxTrigger, playerData.standingColliderConfig);
                stateMachine.ChangeState(player.IdleState);
            }
            else {
                player.SetColliderParameters(player.MovementCollider, playerData.crouchColliderConfig);
                player.SetColliderParameters(player.HitboxTrigger, playerData.crouchColliderConfig);
                stateMachine.ChangeState(player.CrouchIdleState);
            }
        }
        else {
            player.transform.position = startPos;

            if (isHanging && !isClimbing) {
                if (yInput == 1) {
                    player.CameraTarget.SetTargetPosition(Vector3.up, 3f, true);
                }
                else if (xInput == -player.FacingDirection) {
                    player.Anim.SetBool("ledgeHoldBack", true);
                    player.CameraTarget.SetTargetPosition(Vector3.right * xInput, 3f, true);
                }
                else {
                    player.Anim.SetBool("ledgeHoldBack", false);
                    player.CameraTarget.SetTargetPosition(Vector3.zero, 0f, true);
                }

            }

            if (isHanging && playerData.CanLedgeClimb.Value && !isClimbing && xInput == player.FacingDirection && (hasSpace || (!hasSpace && playerData.CanCrouch.Value))) {
                isClimbing = true;
                player.Anim.SetBool("climbLedge", true);
                player.CameraTarget.SetTargetPosition(stopPos + (Vector2.up * 2f));
            }
            else if (playerData.CanLedgeJump.Value && isHanging && !isClimbing && jumpInput && xInput == -player.FacingDirection) {
                stateMachine.ChangeState(player.WallJumpState);
            }
            else if (playerData.CanJump.Value && isHanging && !isClimbing && jumpInput && xInput == 0) {
                coyoteTime = false;
                player.transform.position += Vector3.up * 0.2f;
                stateMachine.ChangeState(player.JumpState);
            }
            else if (isHanging && !isClimbing && !grabInput && yInput == -1) {
                player.transform.position += Vector3.down * 0.2f;
                stateMachine.ChangeState(player.AirborneState);
            }
            else if (playerData.CanWallClimb.Value && isHanging && !isClimbing && grabInput && yInput == -1) {
                player.transform.position += Vector3.down * 0.2f;
                stateMachine.ChangeState(player.WallClimbState);
            }
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        player.SetVelocityX(0f);
        player.SetVelocityY(0f);
    }

    public void SetDetectedPosition(Vector2 pos) => detectedPos = pos;
}
