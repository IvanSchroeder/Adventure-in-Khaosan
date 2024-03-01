using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerMoveState : PlayerGroundedState {
    protected float sprintStopTimer;

    public PlayerMoveState(Player player, StateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        player.isMoving = true;
        player.isRunning = true;
        isSprinting = false;
        player.isRunningAtMaxSpeed = false;
        isSprintingAtMaxSpeed = false;
        elapsedTimeSinceStandup = 0f;

        player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig, true);
    }

    public override void Exit() {
        base.Exit();

        player.isMoving = false;
        player.isRunning = false;
        player.isRunningAtMaxSpeed = false;
        isSprintingAtMaxSpeed = false;

        player.DustParticleSystem.Stop();
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (elapsedTimeSinceStandup < playerData.standupDelay) {
            elapsedTimeSinceStandup += Time.deltaTime;

            if (elapsedTimeSinceStandup >= playerData.standupDelay) {
                player.SetColliderParameters(player.HitboxTrigger, playerData.standingColliderConfig);
            }
        }

        player.CheckFacingDirection(xInput);

        if (isExitingState) return;

        if (xInput == 0 && player.CurrentVelocity.x.AbsoluteValue() < playerData.runSpeed * 0.1f) {
            isSprinting = false;
            stateMachine.ChangeState(player.IdleState);
        }
        else if (playerData.CanCrouch.Value && playerData.CanMove.Value && crouchInputHold) {
            if (player.GroundSlideState.CanGroundSlide() && !player.isChangingDirections && (player.isRunningAtMaxSpeed || isSprintingAtMaxSpeed)) {
                stateMachine.ChangeState(player.GroundSlideState);
            }
            else {
                stateMachine.ChangeState(player.CrouchMoveState);
            }
        }
        else if (playerData.CanWallClimb.Value && player.isTouchingWall && player.isTouchingLedge) {
                if (((playerData.autoWallGrab && xInput == player.FacingDirection) || (!playerData.autoWallGrab && grabInput)) && yInput == 0) 
                    stateMachine.ChangeState(player.WallGrabState);
                else if ((playerData.autoWallGrab || (!playerData.autoWallGrab && grabInput)) && (/*(isOnPlatform && yInput != 0) ||*/ (player.isGrounded && yInput == 1)))
                    stateMachine.ChangeState(player.WallClimbState);
            }

        if (player.isRunning && !isSprinting && player.CurrentVelocity.x.AbsoluteValue() >= playerData.maxRunSpeedThreshold) {
            player.isRunningAtMaxSpeed = true;
        }
        else {
            player.isRunningAtMaxSpeed = false;
        }

        if (playerData.CanSprint.Value) {
            if (player.isRunningAtMaxSpeed && attackInputHold) {
                player.isRunning = false;
                player.isRunningAtMaxSpeed = false;
                isSprinting = true;
                sprintStopTimer = 0f;
            }
            else if (isSprinting && !attackInputHold) {
                if (sprintStopTimer < playerData.sprintStopDelay) sprintStopTimer += Time.deltaTime;

                if (sprintStopTimer >= playerData.sprintStopDelay || playerData.sprintStopDelay == 0) {
                    player.InputHandler.UseAttackStopInput();
                    isSprinting = false;
                    isSprintingAtMaxSpeed = false;
                    player.isRunning = true;
                    player.isRunningAtMaxSpeed = true;

                    player.DustParticleSystem.Stop();
                }
            }

            if (isSprinting && player.CurrentVelocity.x.AbsoluteValue() >= playerData.maxSprintSpeedThreshold) {
                isSprintingAtMaxSpeed = true;
                player.DustParticleSystem.Play();
            }
            else if (isSprinting && player.CurrentVelocity.x.AbsoluteValue() < playerData.maxSprintSpeedThreshold) {
                isSprintingAtMaxSpeed = false;
                if (xInput != 0) player.isRunningAtMaxSpeed = true;
                player.DustParticleSystem.Stop();
            }
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (isExitingState) return;

        if (playerData.correctLedgeOnGround) {
            float velocityBeforeHit = player.CurrentVelocity.x;

            if (xInput != 0) {
                bool hasCheckedFootCorner = false;

                if (!player.isTouchingLedge && player.isTouchingLedgeWithFoot && !hasCheckedFootCorner) {
                    hasCheckedFootCorner = true;

                    bool correctLedge = player.CheckFootLedgeCorrection();

                    if (correctLedge) {
                        correctLedge = false;
                        player.CorrectFootLedge(velocityBeforeHit);
                        player.SetVelocityY(0f);
                    }
                    else {
                        player.SetVelocityX(0f);
                    }
                }
            }
        }

        if (player.isRunning) {
            if (player.isChangingDirections && xInput != 0)
                if (playerData.runSpeedCutoff && player.CurrentVelocity.x.AbsoluteValue() < playerData.runSpeed * playerData.runSpeedCutoffThreshold)
                    player.SetVelocityX(xInput * playerData.runSpeed * playerData.runSpeedCutoffAmount);
                else
                    player.SetVelocityX(xInput * playerData.runSpeed, playerData.runDirectionChangeAcceleration, playerData.lerpVelocity);
            else if (player.isChangingDirections && xInput == 0) {
                player.SetVelocityX(0f, playerData.runDecceleration, playerData.lerpVelocity);
            }
            else if (!player.isChangingDirections && xInput != 0) {
                if (player.CurrentVelocity.x.AbsoluteValue() > playerData.runSpeed) {
                    player.SetVelocityX(xInput * playerData.runSpeed, playerData.sprintDecceleration, playerData.lerpVelocity);
                }
                else {
                    player.SetVelocityX(xInput * playerData.runSpeed, playerData.runAcceleration, playerData.lerpVelocity);
                }
            }
            else if (!player.isChangingDirections && xInput == 0) {
                player.SetVelocityX(0f, playerData.runDecceleration, playerData.lerpVelocity);
            }
        }
        else if (isSprinting) {
            if (player.isChangingDirections && xInput != 0)
                if (playerData.sprintSpeedCutoff && player.CurrentVelocity.x.AbsoluteValue() < playerData.sprintSpeed * playerData.sprintSpeedCutoffThreshold)
                    player.SetVelocityX(xInput * playerData.sprintSpeed * playerData.sprintSpeedCutoffAmount);
                else
                    player.SetVelocityX(xInput * playerData.sprintSpeed, playerData.sprintDirectionChangeAcceleration, playerData.lerpVelocity);
            else if (player.isChangingDirections && xInput == 0)
                player.SetVelocityX(0f, playerData.sprintDecceleration, playerData.lerpVelocity);
            else if (!player.isChangingDirections && xInput != 0)
                player.SetVelocityX(xInput * playerData.sprintSpeed, playerData.sprintAcceleration, playerData.lerpVelocity);
            else if (!player.isChangingDirections && xInput == 0)
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

    public override void AnimationTrigger() {
        AudioManager.instance.PlaySFX("Footstep");
    }
}
