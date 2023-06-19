using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerGroundedState {
    public PlayerIdleState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void DoChecks() {
        base.DoChecks();
    }

    public override void Enter() {
        base.Enter();
    }

    public override void Exit() {
        base.Exit();
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        player.CheckFacingDirection(xInput);

        if (isExitingState) return;

        if (yInput == -1)
            stateMachine.ChangeState(player.CrouchIdleState);
        else if (xInput != 0) {
            if (!isTouchingWall)
                stateMachine.ChangeState(player.MoveState);
            else if (isTouchingWall && playerData.autoWallGrab && xInput == player.FacingDirection && isOnPlatform && !isOnSolidGround) {
                if (yInput == 0) 
                    stateMachine.ChangeState(player.WallGrabState);
                else
                    stateMachine.ChangeState(player.WallClimbState);
            }
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        player.SetVelocityX(xInput * playerData.runSpeed, playerData.runDecceleration, playerData.lerpVelocity);
        player.SetVelocityY(player.CurrentVelocity.y);
    }
}
