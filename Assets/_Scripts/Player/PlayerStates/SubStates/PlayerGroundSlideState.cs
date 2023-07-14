using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerGroundSlideState : PlayerGroundedState {
    public PlayerGroundSlideState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        cumulatedGroundSlideTime = 0f;
        stopSlide = false;

        isGroundSliding = true;

        if (playerData.conserveMomentum && player.CurrentVelocity.x.AbsoluteValue() > playerData.groundSlideSpeed) {
            player.SetVelocityX(player.CurrentVelocity.x);
        }
        else {
            player.SetVelocityX(playerData.groundSlideSpeed * player.FacingDirection);
        }

        player.SetColliderParameters(player.MovementCollider, playerData.crouchColliderConfig);
        player.SetColliderParameters(player.HitboxTrigger, playerData.crouchColliderConfig);
    }

    public override void Exit() {
        base.Exit();

        stopSlide = false;
        groundSlideTime = false;
        cumulatedGroundSlideCooldown = 0f;

        isGroundSliding = false;
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        cumulatedGroundSlideTime += Time.deltaTime;
        
        if (!stopSlide && (((cumulatedGroundSlideTime >= playerData.groundSlideDuration || yInput == 1) && !isTouchingCeiling) || cumulatedGroundSlideTime >= playerData.groundSlideMaxDuration || player.CurrentVelocity.x.AbsoluteValue() == 0f || isTouchingWall)) {
            stopSlide = true;
        }

        if (stopSlide) {
            if (isTouchingCeiling || crouchInputHold || !player.CheckForSpace(player.GroundPoint.position.ToVector2() + Vector2.up * 0.015f, Vector2.up) || player.CheckHazards(player.MidPoint.position)) {
                player.SetColliderParameters(player.MovementCollider, playerData.crouchColliderConfig);
                player.SetColliderParameters(player.HitboxTrigger, playerData.crouchColliderConfig);

                if ((player.CurrentVelocity.x.AbsoluteValue() == 0f || isTouchingWall) && xInput == 0f || player.FacingDirection == -player.CurrentVelocity.x.Sign()) {
                    stateMachine.ChangeState(player.CrouchIdleState);
                }
                else if (xInput == player.FacingDirection && player.CurrentVelocity.x.AbsoluteValue() <= playerData.crouchWalkSpeed) {
                    stateMachine.ChangeState(player.CrouchMoveState);
                }
            }
            else {
                player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig);
                // player.SetColliderParameters(player.HitboxTrigger, playerData.standingColliderConfig);

                if (player.CurrentVelocity.x.AbsoluteValue() == 0f && xInput == 0 || player.FacingDirection == -player.CurrentVelocity.x.Sign()) {
                    stateMachine.ChangeState(player.IdleState);
                }
                else if (xInput == player.FacingDirection && player.CurrentVelocity.x.AbsoluteValue() <= playerData.runSpeed) {
                    stateMachine.ChangeState(player.MoveState);
                }
            }

            // if (player.CurrentVelocity.x.AbsoluteValue() == 0f || isTouchingWall || player.FacingDirection == -player.CurrentVelocity.x.Sign()) {
            //     if (isTouchingCeiling || yInput == -1 || !player.CheckForSpace(player.MidPoint.position) || player.CheckHazards(player.MidPoint.position)) {
            //         player.SetColliderParameters(player.MovementCollider, playerData.crouchColliderConfig);
            //         player.SetColliderParameters(player.HitboxTrigger, playerData.crouchColliderConfig);
            //         stateMachine.ChangeState(player.CrouchIdleState);
            //     }
            //     else {
            //         player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig);
            //         // player.SetColliderParameters(player.HitboxTrigger, playerData.standingColliderConfig);
            //         stateMachine.ChangeState(player.IdleState);
            //     }
            // }
            // else if (xInput == player.FacingDirection && (yInput == -1 || isTouchingCeiling) && player.CurrentVelocity.x.AbsoluteValue() == playerData.crouchWalkSpeed) {
            //     stateMachine.ChangeState(player.CrouchMoveState);
            // }
            // else if (xInput == player.FacingDirection && player.CurrentVelocity.x.AbsoluteValue() >= playerData.runSpeed) {
            //     player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig);
            //     // player.SetColliderParameters(player.HitboxTrigger, playerData.standingColliderConfig);
            //     stateMachine.ChangeState(player.MoveState);
            // }
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (isExitingState) return;

        if (isTouchingWall) {
            player.SetVelocityX(0f);
        }

        if (stopSlide) {
            if (yInput == -1 && (xInput == 0 && player.CurrentVelocity.x.AbsoluteValue() >= 0f))
                player.SetVelocityX(0f, playerData.crouchDecceleration, playerData.lerpVelocity);
            else if (yInput == -1 && xInput != 0 && player.CurrentVelocity.x.AbsoluteValue() >= playerData.crouchWalkSpeed)
                player.SetVelocityX(playerData.crouchWalkSpeed * xInput, playerData.crouchDecceleration, playerData.lerpVelocity);
            else if (xInput == 0 && player.CurrentVelocity.x.AbsoluteValue() >= 0f)
                player.SetVelocityX(0f, playerData.runDecceleration, playerData.lerpVelocity);
            else if (xInput != 0 && player.CurrentVelocity.x.AbsoluteValue() >= 0f)
                player.SetVelocityX(playerData.runSpeed * xInput, playerData.runDecceleration, playerData.lerpVelocity);
        }
        else {
            if (playerData.conserveMomentum && xInput != -player.CurrentVelocity.x.Sign() && player.CurrentVelocity.x.AbsoluteValue() > playerData.groundSlideSpeed)
                player.SetVelocityX(player.CurrentVelocity.x, playerData.groundSlideAcceleration, playerData.lerpVelocity);
            else if (xInput == -player.CurrentVelocity.x.Sign())
                player.SetVelocityX(0f, playerData.groundSlideDecceleration, playerData.lerpVelocity);
            // else if (xInput == 0 && player.CurrentVelocity.x.AbsoluteValue() > playerData.groundSlideSpeed)
            //     player.SetVelocityX(playerData.groundSlideSpeed * player.FacingDirection, playerData.groundSlideAcceleration, playerData.lerpVelocity);
            else if (xInput == 0 && player.CurrentVelocity.x.AbsoluteValue() <= playerData.groundSlideSpeed) {
                player.SetVelocityX(player.CurrentVelocity.x, playerData.groundSlideAcceleration, playerData.lerpVelocity);
            }
        }
        
        player.SetVelocityY(player.CurrentVelocity.y);
    }

    public bool CanGroundSlide() {
        if (playerData.CanGroundSlide.Value && playerData.CanCrouch.Value && playerData.CanMove.Value && groundSlideTime) return true;
        
        return false;
    }
}
