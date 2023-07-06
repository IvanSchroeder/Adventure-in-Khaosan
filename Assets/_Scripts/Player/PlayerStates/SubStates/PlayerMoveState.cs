using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerMoveState : PlayerGroundedState {
    protected float sprintStopTimer;

    public PlayerMoveState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        isMoving = true;
        isRunning = true;
        isSprinting = false;
        isRunningAtMaxSpeed = false;
        isSprintingAtMaxSpeed = false;

        player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig);
    }

    public override void Exit() {
        base.Exit();

        isMoving = false;
        isRunning = false;
        isRunningAtMaxSpeed = false;
        isSprintingAtMaxSpeed = false;
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        player.CheckFacingDirection(xInput);

        if (isExitingState) return;

        if ((xInput == 0) || (xInput != 0 && (isTouchingWall || isTouchingLedge))) {
            isSprinting = false;
            stateMachine.ChangeState(player.IdleState);
        }
        else if (playerData.canCrouch && playerData.canMove && crouchInputHold) {
            if (player.GroundSlideState.CanGroundSlide() && !isChangingDirections && (isRunningAtMaxSpeed || isSprintingAtMaxSpeed)) {
                stateMachine.ChangeState(player.GroundSlideState);
            }
            else {
                stateMachine.ChangeState(player.CrouchMoveState);
            }
        }

        if (isRunning && !isSprinting && player.CurrentVelocity.x.AbsoluteValue() >= playerData.maxRunSpeedThreshold) {
            isRunningAtMaxSpeed = true;
        }
        else {
            isRunningAtMaxSpeed = false;
        }

        if (playerData.canSprint) {
            if (isRunningAtMaxSpeed && attackInputHold) {
                isRunning = false;
                isRunningAtMaxSpeed = false;
                isSprinting = true;
                sprintStopTimer = 0f;
            }
            else if (isSprinting && !attackInputHold) {
                if (sprintStopTimer < playerData.sprintStopDelay) sprintStopTimer += Time.deltaTime;

                if (sprintStopTimer >= playerData.sprintStopDelay || playerData.sprintStopDelay == 0) {
                    player.InputHandler.UseAttackStopInput();
                    isSprinting = false;
                    isSprintingAtMaxSpeed = false;
                    isRunning = true;
                    isRunningAtMaxSpeed = true;
                }
            }

            if (isSprinting && player.CurrentVelocity.x.AbsoluteValue() >= playerData.maxSprintSpeedThreshold) {
                isSprintingAtMaxSpeed = true;
            }
            else if (isSprinting && player.CurrentVelocity.x.AbsoluteValue() < playerData.maxSprintSpeedThreshold) {
                isSprintingAtMaxSpeed = false;
                if (xInput != 0 && !isTouchingWall && !isTouchingLedge) isRunningAtMaxSpeed = true;
            }
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (isExitingState) return;

        if (isRunning) {
            if (isChangingDirections && xInput != 0)
                player.SetVelocityX(xInput * playerData.runSpeed, playerData.runDirectionChangeAcceleration, playerData.lerpVelocity);
            else if (isChangingDirections && xInput == 0) {
                player.SetVelocityX(0f, playerData.runDecceleration, playerData.lerpVelocity);
            }
            else if (!isChangingDirections && xInput != 0) {
                if (player.CurrentVelocity.x.AbsoluteValue() > playerData.runSpeed) {
                    player.SetVelocityX(xInput * playerData.runSpeed, playerData.sprintDecceleration, playerData.lerpVelocity);
                }
                else {
                    player.SetVelocityX(xInput * playerData.runSpeed, playerData.runAcceleration, playerData.lerpVelocity);
                }
            }
            else if (!isChangingDirections && xInput == 0) {
                player.SetVelocityX(0f, playerData.runDecceleration, playerData.lerpVelocity);
            }
        }
        else if (isSprinting) {
            if (isChangingDirections && xInput != 0)
                player.SetVelocityX(xInput * playerData.sprintSpeed, playerData.sprintDirectionChangeAcceleration, playerData.lerpVelocity);
            else if (isChangingDirections && xInput == 0)
                player.SetVelocityX(0f, playerData.sprintDecceleration, playerData.lerpVelocity);
            else if (!isChangingDirections && xInput != 0)
                player.SetVelocityX(xInput * playerData.sprintSpeed, playerData.sprintAcceleration, playerData.lerpVelocity);
            else if (!isChangingDirections && xInput == 0)
                player.SetVelocityX(0f, playerData.sprintDecceleration, playerData.lerpVelocity);
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
