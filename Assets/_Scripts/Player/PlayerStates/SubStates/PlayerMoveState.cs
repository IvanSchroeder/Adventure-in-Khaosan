using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerMoveState : PlayerGroundedState {
    protected float sprintStopTimer;

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

        if (isRunning && !isSprinting && player.CurrentVelocity.x.AbsoluteValue() >= playerData.runSpeed * playerData.maxRunSpeedThreshold)
            isRunningAtMaxSpeed = true;
        else
            isRunningAtMaxSpeed = false;

        if (isSprinting && player.CurrentVelocity.x.AbsoluteValue() >= playerData.sprintSpeed * playerData.maxSprintSpeedThreshold) {
            isSprintingAtMaxSpeed = true;
        }
        else {
            isSprintingAtMaxSpeed = false;
        }

        if (playerData.canSprint) {
            if (attackInputHold) {
                isRunning = false;
                isSprinting = true;
                sprintStopTimer = 0f;
            }
            else if (isSprinting && !attackInputHold && sprintStopTimer < playerData.sprintStopDelay) {
                sprintStopTimer += Time.deltaTime;

                if (sprintStopTimer >= playerData.sprintStopDelay) {
                    isSprinting = false;
                    isSprintingAtMaxSpeed = false;
                    isRunning = true;
                    isRunningAtMaxSpeed = true;
                }
            }
        }

        // if (playerData.canSprint) {
        //     if (attackInputHold) {
        //         if (isRunningAtMaxSpeed && player.CurrentVelocity.x.AbsoluteValue() >= (playerData.runSpeed + playerData.sprintSpeed) * 0.5f) {
        //             isRunning = false;
        //             isRunningAtMaxSpeed = false;

        //             isSprinting = true;

        //             sprintStopTimer = 0f;

        //             Debug.Log($"Started sprinting");
        //         }
                
        //         if (isSprinting && player.CurrentVelocity.x.AbsoluteValue() >= playerData.sprintSpeed * playerData.maxSprintSpeedThreshold)
        //             isSprintingAtMaxSpeed = true;
        //         else
        //             isSprintingAtMaxSpeed = false;
        //     }

        //     if (isRunningAtMaxSpeed && attackInputHold && player.CurrentVelocity.x.AbsoluteValue() > 0f) {
        //         isRunning = false;
        //         isRunningAtMaxSpeed = false;

        //         isSprinting = true;

        //         Debug.Log($"Started sprinting");
        //     }
        //     else if (isSprinting && xInput == 0 && !isChangingDirections && player.CurrentVelocity.x.AbsoluteValue() < playerData.sprintSpeed) {
        //         isSprinting = false;
        //         isSprintingAtMaxSpeed = false;

        //         isRunning = true;

        //         Debug.Log($"Stopped sprinting");
        //     }
        // }

        if ((xInput == 0 && player.CurrentVelocity.x == 0f) || (xInput != 0 && (isTouchingWall || isTouchingLedge))) {
            player.SetVelocityX(0f);
            stateMachine.ChangeState(player.IdleState);
        }
        else if (playerData.canCrouch && playerData.canMove && yInput == -1) {
            if (playerData.canGroundSlide && !isChangingDirections && (isRunningAtMaxSpeed || isSprinting)) {
                stateMachine.ChangeState(player.GroundSlideState);
            }
            else {
                stateMachine.ChangeState(player.CrouchMoveState);
            }
        }

        // player.Anim.SetBool("changingDirections", isChangingDirections);
        // player.Anim.SetBool("running", isRunning);
        // player.Anim.SetBool("sprinting", isSprintingAtMaxSpeed);
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (isExitingState) return;

        if (isRunning) {
            if (isChangingDirections)
                player.SetVelocityX(xInput * playerData.runSpeed, playerData.runDirectionChangeAcceleration, playerData.lerpVelocity);
            else if (xInput == 0) {
                player.SetVelocityX(0f, playerData.runDecceleration, playerData.lerpVelocity);
            }
            else {
                if (player.CurrentVelocity.x.AbsoluteValue() > playerData.runSpeed) {
                    player.SetVelocityX(xInput * playerData.runSpeed, playerData.sprintDecceleration, playerData.lerpVelocity);
                }
                else {
                    player.SetVelocityX(xInput * playerData.runSpeed, playerData.runAcceleration, playerData.lerpVelocity);
                }
            }
        }
        else if (isSprinting) {
            if (isChangingDirections)
                player.SetVelocityX(xInput * playerData.sprintSpeed, playerData.sprintDirectionChangeAcceleration, playerData.lerpVelocity);
            else if (xInput == 0)
                player.SetVelocityX(0f, playerData.sprintDecceleration, playerData.lerpVelocity);
            else
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
