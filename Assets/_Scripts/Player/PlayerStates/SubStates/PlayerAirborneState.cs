using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerAirborneState : PlayerState {
    public PlayerAirborneState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void DoChecks() {
        base.DoChecks();
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

        CheckCoyoteTime();
        CheckWallJumpCoyoteTime();
        CheckJumpMultiplier();

        player.CheckFacingDirection(xInput);

        if (isJumping || isAscending) {
            bool correctCorner = player.CheckCornerCorrection();
            if (isTouchingCeiling && !correctCorner) {
                // bool correctCorner = player.CheckCornerCorrection();

                if (correctCorner) {
                    Debug.Log($"Correct corner!");
                    player.CorrectCorner(player.CurrentVelocity.y);
                    correctCorner = false;
                }
                else {
                    Debug.Log($"Bonked head!");
                    player.SetVelocityY(0f);
                }
            }
            // else if (isFalling) {
            //     isJumping = false;
            // }
        }

        if (isFalling) {
            if (yInput == -1) {
                playerData.currentFallSpeed = playerData.fastFallSpeed;
                isFastFalling = true;
            }
            else {
                playerData.currentFallSpeed = playerData.defaultFallSpeed;
                isFastFalling = false;
            }

            player.Anim.SetBool("fastFall", isFastFalling);
        }
        
        if (playerData.canWallJump && jumpInput && (isTouchingWall || isTouchingBackWall || wallJumpCoyoteTime || hasTouchedWall) && yInput != -1) {
            StopWallJumpCoyoteTime();
            isTouchingWall = player.CheckWall();
            isTouchingBackWall = player.CheckBackWall();
            player.WallJumpState.GetWallJumpDirection(isTouchingWall || hasTouchedWall);
            stateMachine.ChangeState(player.WallJumpState);
        }
        else if (playerData.canJump && jumpInput && player.JumpState.CanJump() && yInput != -1 && !isIgnoringPlatforms) {
            stateMachine.ChangeState(player.JumpState);
        }
        else if (playerData.canLedgeClimb && isTouchingWall && !isTouchingLedge && !isGrounded && yInput == 0) {
            isTouchingWall = player.CheckWall();
            isTouchingLedge = player.CheckLedge();
            if (isTouchingWall && !isTouchingLedge && !isGrounded) {
                player.LedgeClimbState.SetDetectedPosition(player.transform.position);
                stateMachine.ChangeState(player.LedgeClimbState);
            }
            else return;
        }
        else if (isOnSolidGround && !isJumping) {
            if (playerData.canMove && xInput != 0 && !isTouchingWall) stateMachine.ChangeState(player.MoveState);
            else if (playerData.canGroundSlide && xInput != 0 && !isTouchingWall && yInput == -1) stateMachine.ChangeState(player.GroundSlideState);
            else stateMachine.ChangeState(player.LandState);
        }
        else if (isOnPlatform && isIgnoringPlatforms && !isJumping) {
            return;
        }
        else if (isOnPlatform && !isIgnoringPlatforms && !isJumping) {
            RaycastHit2D detectedPlatform = player.GetPlatformHit();
            if (detectedPlatform) player.transform.position = detectedPlatform.point + new Vector2(0f, 0.05f);

            if (playerData.canMove && xInput != 0 && !isTouchingWall) stateMachine.ChangeState(player.MoveState);
            else if (playerData.canGroundSlide && xInput != 0 && !isTouchingWall && yInput == -1 && player.CurrentVelocity.x.AbsoluteValue() >= playerData.runSpeed * playerData.maxRunSpeedThreshold) stateMachine.ChangeState(player.GroundSlideState);
            else stateMachine.ChangeState(player.LandState);
        }
        else if (playerData.canWallSlide && !playerData.autoWallGrab && isTouchingWall && isTouchingLedge) {
            if (playerData.canWallClimb && grabInput) stateMachine.ChangeState(player.WallGrabState);
            else if (!isJumping && (xInput == player.FacingDirection || playerData.autoWallSlide)) stateMachine.ChangeState(player.WallSlideState);
        }
        else if (playerData.canWallSlide && playerData.autoWallGrab && isTouchingWall && isTouchingLedge && !isJumping) {
            if (playerData.canWallClimb && xInput == player.FacingDirection) stateMachine.ChangeState(player.WallGrabState);
            else if (xInput == 0 || playerData.autoWallSlide) stateMachine.ChangeState(player.WallSlideState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        // if (playerData.conserveMomentum && xInput == player.FacingDirection)
        //     player.SetVelocityX(player.CurrentVelocity.x, playerData.airAcceleration, playerData.lerpVelocityInAir);
        // else if (xInput == 0f)
        //     player.SetVelocityX(0f, playerData.airDecceleration, playerData.lerpVelocityInAir);
        // else
        //     player.SetVelocityX(xInput * playerData.runSpeed, playerData.airAcceleration, playerData.lerpVelocityInAir);

        if (xInput == 0f)
            player.SetVelocityX(0f, playerData.airDecceleration, playerData.lerpVelocityInAir);
        else
            player.SetVelocityX(xInput * playerData.runSpeed, playerData.airAcceleration, playerData.lerpVelocityInAir);

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
        
        if (jumpInputStop) {
            player.SetVelocityY(player.CurrentVelocity.y * playerData.variableJumpHeightMultiplier);
            isJumping = false;
            player.InputHandler.UseJumpStopInput();
        }
        else if (yInput == -1 && player.CurrentVelocity.y > playerData.jumpHeight * 0.5f) {
            player.SetVelocityY(0f, playerData.fallAcceleration * 2f, playerData.lerpVerticalVelocity);
        }
        else if (isFalling) {
            isJumping = false;
            player.InputHandler.UseJumpStopInput();
        }
    }

    private void CheckCoyoteTime() {
        if (coyoteTime && Time.time > startTime + playerData.coyoteTime) {
            coyoteTime = false;
            player.JumpState.DecreaseAmountOfJumpsLeft();
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

    public void StartCoyoteTime() => coyoteTime = true;
    public void StartWallJumpCoyoteTime() {
        wallJumpCoyoteTime = true;
        cumulatedWallJumpCoyoteTime = 0f;
        startWallJumpCoyoteTime = 0f;
    }
    public void StopWallJumpCoyoteTime() => wallJumpCoyoteTime = false;
    public void SetIsJumping() => isJumping = true;
}
