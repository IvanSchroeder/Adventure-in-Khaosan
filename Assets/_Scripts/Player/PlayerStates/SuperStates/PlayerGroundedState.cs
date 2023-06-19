using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerState {
    public PlayerGroundedState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void DoChecks() {
        base.DoChecks();
    }

    public override void Enter() {
        base.Enter();

        player.JumpState.ResetAmountOfJumpsLeft();
    }

    public override void Exit() {
        base.Exit();
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (jumpInput && player.JumpState.CanJump() && !isTouchingCeiling && !isIgnoringPlatforms && yInput != -1) {
            coyoteTime = false;
            stateMachine.ChangeState(player.JumpState);
        }
        else if (!isGrounded) {
            player.AirborneState.StartCoyoteTime();
            stateMachine.ChangeState(player.AirborneState);
        }
        else if (isTouchingWall && ((playerData.autoWallGrab && xInput == player.FacingDirection) || (!playerData.autoWallGrab && grabInput)) && isTouchingLedge) {
            stateMachine.ChangeState(player.WallGrabState);
        }
    }
}
