using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerAirborneState : PlayerState {
    public PlayerAirborneState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        isTouchingWall = player.CheckWall();
        isTouchingLedge = player.CheckLedge();

        if (isTouchingWall && !isTouchingLedge && !isGrounded) {
            player.LedgeClimbState.SetDetectedPosition(player.transform.position);
        }

        player.Rb.gravityScale = playerData.defaultGravityScale;
        isAirborne = true;

        player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig);
    }

    public override void Exit() {
        base.Exit();

        hasTouchedWall = false;
        hasTouchedWallBack = false;
        isTouchingWall = false;
        isTouchingBackWall = false;
        isAirborne = false;

        player.Anim.SetBool("fastFall", false);
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        // hasSpace = player.CheckForSpace(player.transform.position);

        CheckCoyoteTime();
        CheckWallJumpCoyoteTime();
        CheckJumpMultiplier();

        player.CheckFacingDirection(xInput);

        if (isJumping || isAscending) {
            bool hasChecked = false;
            float velocityBeforeHit = player.CurrentVelocity.y;

            if (isTouchingCeiling && !hasChecked) {
                hasChecked = true;

                bool correctCorner = player.CheckCornerCorrection();

                if (correctCorner) {
                    correctCorner = false;
                    player.CorrectCorner(velocityBeforeHit);
                }
                else {
                    player.SetVelocityY(0f);
                }
            }
        }

        if (isFalling) {
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
        
        // if (!hasSpace) {
        //     if (xInput == 0)
        //         stateMachine.ChangeState(player.CrouchIdleState);
        //     else
        //         stateMachine.ChangeState(player.CrouchMoveState);
        // }
        if (playerData.canWallJump && jumpInput && (isTouchingWall || isTouchingBackWall || wallJumpCoyoteTime || hasTouchedWall) && yInput != -1) {
            stateMachine.ChangeState(player.WallJumpState);
        }
        else if (playerData.canJump && jumpInput && player.JumpState.CanJump() && yInput != -1 && !isIgnoringPlatforms) {
            stateMachine.ChangeState(player.JumpState);
        }
        else if (playerData.canLedgeClimb && isTouchingWall && !isTouchingLedge && !isGrounded && !jumpInputHold && yInput != -1) {
            player.LedgeClimbState.SetDetectedPosition(player.transform.position);
            stateMachine.ChangeState(player.LedgeClimbState);
        }
        else if (isOnSolidGround && !isJumping) {
            RaycastHit2D detectedGround = player.GetGroundHit();
            if (detectedGround) {
                Vector2 position = new Vector2(player.transform.position.x, (detectedGround.point + (Vector2.up * 0.05f)).y);
                player.transform.position = position;
            }

            if (xInput != 0 && !isTouchingWall) {
                if (player.GroundSlideState.CanGroundSlide() && !isChangingDirections && yInput == -1 && player.CurrentVelocity.x.AbsoluteValue() >= playerData.maxRunSpeedThreshold)
                    stateMachine.ChangeState(player.GroundSlideState);
                else if (playerData.canMove && !isTouchingLedge)
                    stateMachine.ChangeState(player.MoveState);
            }
            else
                stateMachine.ChangeState(player.LandState);
        }
        else if (isOnPlatform && !isIgnoringPlatforms && !isJumping) {
            RaycastHit2D detectedPlatform = player.GetPlatformHit();
            
            if (detectedPlatform) {
                Vector2 position = new Vector2(player.transform.position.x, (detectedPlatform.point + (Vector2.up * 0.05f)).y);
                player.transform.position = position;
            }

            if (xInput != 0 && !isTouchingWall) {
                if (playerData.canMove)
                    stateMachine.ChangeState(player.MoveState);
                else if (player.GroundSlideState.CanGroundSlide() && !isChangingDirections && yInput == -1 && player.CurrentVelocity.x.AbsoluteValue() >= playerData.maxRunSpeedThreshold)
                    stateMachine.ChangeState(player.GroundSlideState);
            }
            else
                stateMachine.ChangeState(player.LandState);
        }
        else if (playerData.canWallSlide && !playerData.autoWallGrab && isTouchingWall && isTouchingLedge && !isJumping) {
            if (playerData.canWallClimb && grabInput) stateMachine.ChangeState(player.WallGrabState);
            else if (xInput == player.FacingDirection || playerData.autoWallSlide) stateMachine.ChangeState(player.WallSlideState);
        }
        else if (playerData.canWallSlide && playerData.autoWallGrab && isTouchingWall && isTouchingLedge && !isJumping) {
            if (playerData.canWallClimb && xInput == player.FacingDirection) stateMachine.ChangeState(player.WallGrabState);
            else if (xInput == 0 || playerData.autoWallSlide) stateMachine.ChangeState(player.WallSlideState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();
        
        if (xInput == 0f)
            player.SetVelocityX(0f, playerData.airDecceleration, playerData.lerpVelocityInAir);
        else if (xInput != 0f) {
            if (playerData.conserveMomentum && player.CurrentVelocity.x.AbsoluteValue() > playerData.runSpeed && xInput == player.CurrentVelocity.x.Sign())
                player.SetVelocityX(player.CurrentVelocity.x.AbsoluteValue() * xInput, playerData.airAcceleration, playerData.lerpVelocityInAir);
            else
                player.SetVelocityX(xInput * playerData.runSpeed, playerData.airAcceleration, playerData.lerpVelocityInAir);
        }

        if (isFalling) {
            player.SetVelocityY(player.CurrentVelocity.y, playerData.fallAcceleration, playerData.lerpVerticalVelocity);
        }
        else if (isJumping || isAscending) {
            player.SetVelocityY(player.CurrentVelocity.y, playerData.ascensionAcceleration, playerData.lerpVerticalVelocity);
            // player.SetVelocityY(player.CurrentVelocity.y);
        }
    }

    private void CheckJumpMultiplier() {
        if (!isJumping) return;
        
        if (playerData.jumpInputStop) {
            player.SetVelocityY(player.CurrentVelocity.y * playerData.variableJumpHeightMultiplier);
            isJumping = false;
            player.InputHandler.UseJumpStopInput();
        }
        else if (yInput == -1 && player.CurrentVelocity.y > playerData.jumpHeight * 0.85f) {
            player.SetVelocityY(0f, playerData.fallAcceleration * 2f, playerData.lerpVerticalVelocity);
        }
        else if (isFalling) {
            isJumping = false;
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
                hasTouchedWall = false;
                hasTouchedWallBack = false;
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
    public void SetIsJumping() => isJumping = true;
}
