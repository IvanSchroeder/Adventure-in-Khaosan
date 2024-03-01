using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerAirborneState : PlayerState {
    public PlayerAirborneState(Player player, StateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        player.isTouchingWall = player.CheckWall();
        player.isTouchingLedge = player.CheckLedge();

        if (player.isTouchingWall && !player.isTouchingLedge && !player.isGrounded) {
            player.LedgeClimbState.SetDetectedPosition(player.transform.position);
        }

        player.Rb.gravityScale = playerData.defaultGravityScale;

        player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig, true);
        player.SetColliderParameters(player.HitboxTrigger, playerData.standingColliderConfig);

        player.DustParticleSystem.Stop();
    }

    public override void Exit() {
        base.Exit();

        player.hasTouchedWall = false;
        player.hasTouchedWallBack = false;
        player.isTouchingWall = false;
        player.isTouchingBackWall = false;

        player.Anim.SetBool("fastFall", false);
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        CheckCoyoteTime();
        CheckWallJumpCoyoteTime();
        CheckJumpMultiplier();

        player.CheckFacingDirection(xInput);

        if (player.isFalling) {
            if (yInput >= 0) {
                playerData.currentFallSpeed = playerData.defaultFallSpeed;
                isFastFalling = false;
            }
            else {
                playerData.currentFallSpeed = playerData.fastFallSpeed;
                isFastFalling = true;
            }

        }
        else {
            isFastFalling = false;
        }
        
        player.Anim.SetBool("fastFall", isFastFalling);
        
        if (playerData.CanWallJump.Value && jumpInput && (player.isTouchingWall || wallJumpCoyoteTime || player.hasTouchedWall) && yInput != -1) {
            stateMachine.ChangeState(player.WallJumpState);
        }
        else if (playerData.CanJump.Value && jumpInput && player.JumpState.CanJump() && yInput != -1 && !player.isIgnoringPlatforms) {
            stateMachine.ChangeState(player.JumpState);
        }
        else if (playerData.CanLedgeGrab.Value && player.isTouchingWall && !player.isTouchingLedge && !player.isGrounded && !jumpInputHold && yInput != -1) {
            player.LedgeClimbState.SetDetectedPosition(player.transform.position);
            stateMachine.ChangeState(player.LedgeClimbState);
        }
        else if (player.isOnSolidGround && player.CurrentVelocity.y == 0 && !player.isJumping) {
            player.CreateParticle(player.DustBurstParticleSystemPrefab, player.GroundPoint.position, null);

            RaycastHit2D detectedGround = player.GetGroundHit();

            if (detectedGround) {
                Vector2 position = new Vector2(player.transform.position.x, detectedGround.point.y + 0.05f);
                player.transform.position = position;
            }

            if (xInput != 0 && !player.isTouchingWall) {
                if (player.GroundSlideState.CanGroundSlide() && !player.isChangingDirections && yInput == -1 && player.CurrentVelocity.x.AbsoluteValue() >= playerData.maxRunSpeedThreshold)
                    stateMachine.ChangeState(player.GroundSlideState);
                else if (playerData.CanMove.Value && !player.isTouchingLedge) {
                    AudioManager.instance.PlaySFX("PlayerLand");
                    stateMachine.ChangeState(player.MoveState);
                }
            }
            else stateMachine.ChangeState(player.LandState);
        }
        else if (player.isOnPlatform && !player.isIgnoringPlatforms && player.CurrentVelocity.y == 0 && !player.isJumping) {
            player.CreateParticle(player.DustBurstParticleSystemPrefab, player.GroundPoint.position, null);

            RaycastHit2D detectedPlatform = player.GetPlatformHit();
            
            if (detectedPlatform) {
                Vector2 position = new Vector2(player.transform.position.x, detectedPlatform.point.y + 0.05f);
                player.transform.position = position;
            }

            if (xInput != 0 && !player.isTouchingWall) {
                if (player.GroundSlideState.CanGroundSlide() && !player.isChangingDirections && yInput == -1 && player.CurrentVelocity.x.AbsoluteValue() >= playerData.maxRunSpeedThreshold)
                    stateMachine.ChangeState(player.GroundSlideState);
                else if (playerData.CanMove.Value && !player.isTouchingLedge)
                    AudioManager.instance.PlaySFX("PlayerLand");
                    stateMachine.ChangeState(player.MoveState);
            }
            else stateMachine.ChangeState(player.LandState);
        }
        else if (playerData.CanWallSlide.Value && playerData.autoWallGrab && player.isTouchingWall && player.isTouchingLedge) {
            if (playerData.CanWallClimb.Value && xInput == player.FacingDirection) stateMachine.ChangeState(player.WallGrabState);
            else if ((xInput == 0 || playerData.autoWallSlide) && !player.isJumping) stateMachine.ChangeState(player.WallSlideState);
        }
        else if (playerData.CanWallSlide.Value && !playerData.autoWallGrab && player.isTouchingWall && player.isTouchingLedge) {
            if (playerData.CanWallClimb.Value && grabInput) stateMachine.ChangeState(player.WallGrabState);
            else if (xInput == player.FacingDirection || playerData.autoWallSlide) stateMachine.ChangeState(player.WallSlideState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (playerData.correctCornerOnAir) {
            float yVelocityBeforeHit = player.CurrentVelocity.y;

            if (player.isJumping || player.isAscending) {
                bool hasCheckedHeadCorner = false;
                if (player.CurrentVelocity.y != 0f) yVelocityBeforeHit = player.CurrentVelocity.y;

                if (player.isTouchingCeiling && !hasCheckedHeadCorner) {
                    hasCheckedHeadCorner = true;

                    bool correctCorner = player.CheckCeilingCornerCorrection();

                    if (correctCorner) {
                        correctCorner = false;
                        player.CorrectHeadCorner(yVelocityBeforeHit);
                    }
                    else {
                        player.SetVelocityY(0f);
                    }
                }
            }
        }

        if (playerData.correctLedgeOnAir && player.CurrentVelocity.x.AbsoluteValue() > playerData.runSpeed * 0.3f) {
            float xVelocityBeforeHit = player.CurrentVelocity.x;

            if (xInput != 0) {
                bool hasCheckedFootCorner = false;

                if (player.CurrentVelocity.x != 0f) xVelocityBeforeHit = player.CurrentVelocity.x;

                if (!player.isTouchingLedge && player.isTouchingLedgeWithFoot && !hasCheckedFootCorner) {
                    hasCheckedFootCorner = true;

                    bool correctLedge = player.CheckFootLedgeCorrection();

                    if (correctLedge) {
                        correctLedge = false;
                        player.SetVelocityY(0f);
                        player.CorrectFootLedge(xVelocityBeforeHit);
                    }
                    else {
                        player.SetVelocityX(0f);
                    }
                }
            }
        }
        
        if (xInput == 0f)
            player.SetVelocityX(0f, playerData.airDecceleration, playerData.lerpVelocityInAir);
        else if (xInput != 0f) {
            if (playerData.conserveMomentum && player.CurrentVelocity.x.AbsoluteValue() > playerData.runSpeed && xInput == player.CurrentVelocity.x.Sign())
                player.SetVelocityX(player.CurrentVelocity.x.AbsoluteValue() * xInput, playerData.airAcceleration, playerData.lerpVelocityInAir);
            else
                player.SetVelocityX(xInput * playerData.runSpeed, playerData.airAcceleration, playerData.lerpVelocityInAir);
        }

        if (player.isFalling) {
            player.SetVelocityY(player.CurrentVelocity.y, playerData.fallAcceleration, playerData.lerpVerticalVelocity);
        }
        else if (player.isJumping || player.isAscending) {
            player.SetVelocityY(player.CurrentVelocity.y, playerData.ascensionAcceleration, playerData.lerpVerticalVelocity);
        }
    }

    private void CheckJumpMultiplier() {
        if (!player.isJumping) return;
        
        if (jumpInputStop) {
            player.SetVelocityY(player.CurrentVelocity.y * playerData.variableJumpHeightMultiplier);
            player.isJumping = false;
            player.InputHandler.UseJumpStopInput();
        }
        else if (yInput == -1 && player.CurrentVelocity.y > playerData.jumpHeight * 0.85f) {
            player.SetVelocityY(0f, playerData.fallAcceleration * 2f, playerData.lerpVerticalVelocity);
        }
        else if (player.isFalling) {
            player.isJumping = false;
            player.InputHandler.UseJumpStopInput();
        }
    }

    private void CheckCoyoteTime() {
        if (coyoteTime) {
            cumulatedJumpCoyoteTime += Time.deltaTime;

            if (cumulatedJumpCoyoteTime >= 0f + playerData.coyoteTime) {
                coyoteTime = false;
                if (amountOfJumpsLeft > 0) player.JumpState.DecreaseAmountOfJumpsLeft();
            }
        }
    }

    private void CheckWallJumpCoyoteTime() {
        if (wallJumpCoyoteTime) {
            cumulatedWallJumpCoyoteTime += Time.deltaTime;
            if (cumulatedWallJumpCoyoteTime >= 0f + playerData.wallJumpCoyoteTime) {
                wallJumpCoyoteTime = false;
                player.hasTouchedWall = false;
                player.hasTouchedWallBack = false;
                player.JumpState.DecreaseAmountOfJumpsLeft();
            }
        }
    }

    public void StartCoyoteTime() {
        coyoteTime = true;
        cumulatedJumpCoyoteTime = 0f;
    }

    public void StartWallJumpCoyoteTime() {
        wallJumpCoyoteTime = true;
        cumulatedWallJumpCoyoteTime = 0f;
    }

    public void StopCoyoteTime() => coyoteTime = false;
    public void StopWallJumpCoyoteTime() => wallJumpCoyoteTime = false;
    public void SetIsJumping() => player.isJumping = true;
}
