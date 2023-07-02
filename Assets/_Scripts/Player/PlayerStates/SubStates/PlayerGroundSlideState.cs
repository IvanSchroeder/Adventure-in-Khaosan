using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerGroundSlideState : PlayerGroundedState {
    protected float groundSlideTime;
    protected bool stopSlide;

    public PlayerGroundSlideState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void DoChecks() {
        base.DoChecks();
    }

    public override void Enter() {
        base.Enter();

        groundSlideTime = 0f;
        stopSlide = false;

        isGroundSliding = true;

        player.SetColliderParameters(player.MovementCollider, playerData.crouchColliderConfig);
    }

    public override void Exit() {
        base.Exit();

        stopSlide = false;

        isGroundSliding = false;
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        if (groundSlideTime < playerData.groundSlideDuration) {
            groundSlideTime += Time.deltaTime;
        }
        
        if (!stopSlide && (((groundSlideTime >= playerData.groundSlideDuration || yInput == 1) && !isTouchingCeiling) || xInput == -player.FacingDirection || isTouchingWall)) {
            stopSlide = true;
        }

        if (stopSlide) {
            if (yInput == -1 || isTouchingCeiling) {
                if (xInput == 0 && player.CurrentVelocity.x == 0f)
                    stateMachine.ChangeState(player.CrouchIdleState);
                else if (xInput != 0 && player.CurrentVelocity.x.AbsoluteValue() < playerData.crouchWalkSpeed * 0.2f)
                    stateMachine.ChangeState(player.CrouchMoveState);
            }
            else {
                if ((xInput == 0 && player.CurrentVelocity.x == 0f) || isTouchingWall) {
                    player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig);
                    stateMachine.ChangeState(player.IdleState);
                }
                else if (xInput != 0 && player.CurrentVelocity.x.AbsoluteValue() < playerData.runSpeed * 0.2f)
                    player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig);
                    stateMachine.ChangeState(player.MoveState);
            }
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (isExitingState) return;

        if (stopSlide) {
            player.SetVelocityX(0f, playerData.groundSlideDecceleration, playerData.lerpVelocity);
            // if (yInput == -1 && xInput == player.FacingDirection) {
            //     player.SetVelocityX(0f, playerData.groundSlideDecceleration, playerData.lerpVelocity);
            // }
            // else if (yInput != -1 && xInput == player.FacingDirection) {
            //     player.SetVelocityX(0f, playerData.groundSlideDecceleration, playerData.lerpVelocity);
            // }
            // else if (xInput == 0) {
            //     player.SetVelocityX(0f, playerData.groundSlideDecceleration, playerData.lerpVelocity);
            // }
        }
        else {
            if (playerData.conserveMomentum && player.CurrentVelocity.x.AbsoluteValue() > playerData.groundSlideSpeed)
                player.SetVelocityX(player.CurrentVelocity.x, playerData.groundSlideAcceleration, playerData.lerpVelocity);
            else
                player.SetVelocityX(playerData.groundSlideSpeed * player.FacingDirection, playerData.groundSlideAcceleration, playerData.lerpVelocity);
        }
        
        player.SetVelocityY(player.CurrentVelocity.y);
    }
}
