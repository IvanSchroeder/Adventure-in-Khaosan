using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerGroundedState {
    public PlayerMoveState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
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

        if (xInput == 0 || (player.Rb.velocity.x != 0f && isTouchingWall)) {
            stateMachine.ChangeState(player.IdleState);
        }
        else if (yInput == -1) {
            stateMachine.ChangeState(player.CrouchMoveState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (!isOnSlope) {
            player.SetVelocityX(xInput * playerData.runSpeed, playerData.runAcceleration, playerData.lerpVelocity);
            player.SetVelocityY(player.CurrentVelocity.y);
        }
        else if (isOnSlope) {
            // player.SetVelocityX(-xInput * playerData.runSpeed * slopeNormalPerpendicular.x, playerData.runAcceleration, playerData.lerpVelocity);
            player.SetVelocityX(-xInput * playerData.runSpeed * slopeNormalPerpendicular.x);
            player.SetVelocityYOnGround(-xInput * playerData.runSpeed * slopeNormalPerpendicular.y);
        }
    }
}
