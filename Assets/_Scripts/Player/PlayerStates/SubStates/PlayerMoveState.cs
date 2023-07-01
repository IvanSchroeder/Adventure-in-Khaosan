using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerMoveState : PlayerGroundedState {
    public PlayerMoveState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void DoChecks() {
        base.DoChecks();
    }

    public override void Enter() {
        base.Enter();

        isMoving = true;
        isRunning = true;
        isSprinting = false;

        player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig);
    }

    public override void Exit() {
        base.Exit();

        isMoving = false;
        isRunning = false;
        isSprinting = false;
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        player.CheckFacingDirection(xInput);

        if (isExitingState) return;

        if (player.CurrentVelocity.x.AbsoluteValue() >= playerData.runSpeed * playerData.maxRunSpeedThreshold && isRunning) {
            isRunningAtMaxSpeed = true;
        }
        else {
            isRunningAtMaxSpeed = false;
        }

        if (player.CurrentVelocity.x.AbsoluteValue() >= playerData.sprintSpeed * playerData.maxRunSpeedThreshold && isSprinting) {
            isSprintingAtMaxSpeed = true;
        }
        else {
            isSprintingAtMaxSpeed = false;
        }

        if (playerData.canSprint) {
            if (attackInputHold) {
                isRunning = false;
                isRunningAtMaxSpeed = false;

                isSprinting = true;
            }
            else {
                isSprinting = false;
                isSprintingAtMaxSpeed = false;

                isRunning = true;
            }
        }

        if (xInput == 0) {
            stateMachine.ChangeState(player.IdleState);
        }
        else if (player.CurrentVelocity.x != 0 && (isTouchingWall || isTouchingLedge)) {
            player.SetVelocityX(0f);
            stateMachine.ChangeState(player.IdleState);
        }
        else if (playerData.canCrouch && playerData.canMove && yInput == -1) {
            if (playerData.canGroundSlide && isRunningAtMaxSpeed || isSprinting) {
                stateMachine.ChangeState(player.GroundSlideState);
            }
            else {
                stateMachine.ChangeState(player.CrouchMoveState);
            }
        }

        player.Anim.SetBool("running", isRunning);
        player.Anim.SetBool("sprinting", isSprinting);
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (isRunning) {
            if (player.CurrentVelocity.x.AbsoluteValue() > playerData.runSpeed) {
                player.SetVelocityX(xInput * playerData.runSpeed, playerData.sprintDecceleration, playerData.lerpVelocity);
            }
            else {
                player.SetVelocityX(xInput * playerData.runSpeed, playerData.runAcceleration, playerData.lerpVelocity);
            }
        }
        else if (isSprinting) {
            player.SetVelocityX(xInput * playerData.sprintSpeed, playerData.sprintAcceleration, playerData.lerpVelocity);
        }

        player.SetVelocityY(player.CurrentVelocity.y);

        // if (!isOnSlope) {
        //     player.SetVelocityX(xInput * playerData.runSpeed, playerData.runAcceleration, playerData.lerpVelocity);
        //     player.SetVelocityY(player.CurrentVelocity.y);
        // }
        // else if (isOnSlope) {
        //     // player.SetVelocityX(-xInput * playerData.runSpeed * slopeNormalPerpendicular.x, playerData.runAcceleration, playerData.lerpVelocity);
        //     player.SetVelocityX(-xInput * playerData.runSpeed * slopeNormalPerpendicular.x);
        //     player.SetVelocityYOnGround(-xInput * playerData.runSpeed * slopeNormalPerpendicular.y);
        // }
    }
}
