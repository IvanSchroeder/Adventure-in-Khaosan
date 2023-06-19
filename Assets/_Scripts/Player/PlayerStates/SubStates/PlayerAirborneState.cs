using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirborneState : PlayerState {
    public PlayerAirborneState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void DoChecks() {
        base.DoChecks();

        // hasTouchedWall = isTouchingWall;
        // hasTouchedWallBack = isTouchingBackWall;

        // if (isTouchingWall && !isTouchingLedge && !isGrounded) {
        //     player.LedgeClimbState.SetDetectedPosition(player.transform.position);
        // }

        // if (!wallJumpCoyoteTime && !isTouchingWall && !isTouchingBackWall && (hasTouchedWall || hasTouchedWallBack)) {
        //     StartWallJumpCoyoteTime();
        // }
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

        player.SetColliderHeight(playerData.standColliderHeight);
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
            else if (isFalling) {
                isJumping = false;
            }
        }
        
        if (jumpInput && (isTouchingWall || isTouchingBackWall || wallJumpCoyoteTime || hasTouchedWall) && yInput != -1) {
            StopWallJumpCoyoteTime();
            isTouchingWall = player.CheckWall();
            isTouchingBackWall = player.CheckBackWall();
            player.WallJumpState.GetWallJumpDirection(isTouchingWall || hasTouchedWall);
            stateMachine.ChangeState(player.WallJumpState);
        }
        else if (jumpInput && player.JumpState.CanJump() && yInput != -1 && !isIgnoringPlatforms) {
            stateMachine.ChangeState(player.JumpState);
        }
        else if (isTouchingWall && !isTouchingLedge && !isGrounded && yInput == 0) {
            isTouchingWall = player.CheckWall();
            isTouchingLedge = player.CheckLedge();
            if (isTouchingWall && !isTouchingLedge && !isGrounded) {
                player.LedgeClimbState.SetDetectedPosition(player.transform.position);
                stateMachine.ChangeState(player.LedgeClimbState);
            }
            else return;
        }
        else if (isOnSolidGround && !isJumping) {
            if (xInput == 0 || isTouchingWall)
                stateMachine.ChangeState(player.LandState);
            else if (xInput != 0 && !isTouchingWall)
                stateMachine.ChangeState(player.MoveState);
        }
        else if (isOnPlatform && isIgnoringPlatforms && !isJumping) {
            return;
        }
        else if (isOnPlatform && !isIgnoringPlatforms && !isJumping) {
            RaycastHit2D detectedPlatform = player.GetPlatformHit();
            if (detectedPlatform)
                player.transform.position = detectedPlatform.point + new Vector2(0f, 0.05f);

            if (xInput == 0 || isTouchingWall)
                stateMachine.ChangeState(player.LandState);
            else if (xInput != 0 && !isTouchingWall)
                stateMachine.ChangeState(player.MoveState);
        }
        else if (!playerData.autoWallGrab && isTouchingWall && isTouchingLedge) {
            if (grabInput)
                stateMachine.ChangeState(player.WallGrabState);
            else if (!isJumping && (xInput == player.FacingDirection || playerData.autoWallSlide))
                stateMachine.ChangeState(player.WallSlideState);
        }
        else if (playerData.autoWallGrab && isTouchingWall && isTouchingLedge && !isJumping) {
            if (xInput == player.FacingDirection)
                stateMachine.ChangeState(player.WallGrabState);
            else if (xInput == 0)
                stateMachine.ChangeState(player.WallSlideState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (xInput != 0)
            player.SetVelocityX(xInput * playerData.runSpeed, playerData.airAcceleration, playerData.lerpVelocityInAir);
        else
            player.SetVelocityX(xInput * playerData.runSpeed, playerData.airDecceleration, playerData.lerpVelocityInAir);

        if (isFalling) {
            if (yInput == -1) {
                playerData.currentFallSpeed = playerData.fastFallSpeed;
                player.SetVelocityY(player.CurrentVelocity.y, playerData.fallAcceleration / 2f, playerData.lerpVerticalVelocity);
                player.Anim.SetBool("fastFall", true);
            }
            else {
                playerData.currentFallSpeed = playerData.defaultFallSpeed;
                player.SetVelocityY(player.CurrentVelocity.y, playerData.fallAcceleration, playerData.lerpVerticalVelocity);
                player.Anim.SetBool("fastFall", false);
            }
        }
        else if (isJumping || isAscending)
            player.SetVelocityY(player.CurrentVelocity.y);
    }

    private void CheckJumpMultiplier() {
        if (isJumping) {
            if (jumpInputStop) {
                player.SetVelocityY(player.CurrentVelocity.y * playerData.variableJumpHeightMultiplier);
                isJumping = false;
            }
            // else if (isFalling) {
            //     isJumping = false;
            // }
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
