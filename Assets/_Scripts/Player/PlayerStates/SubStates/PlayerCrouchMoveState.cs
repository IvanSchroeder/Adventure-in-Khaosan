using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrouchMoveState : PlayerGroundedState {
    public PlayerCrouchMoveState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        // player.SetVelocityY(0f);

        isCrouching = true;
        player.SetColliderParameters(player.MovementCollider, playerData.crouchColliderConfig);
        // player.CalculateColliderHeight(playerData.crouchColliderHeight);
    }

    public override void Exit() {
        base.Exit();

        isCrouching = false;
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        player.CheckFacingDirection(xInput);

        if (isExitingState) return;

        if (xInput == 0 || (player.Rb.velocity.x != 0f && isTouchingWall)) {
            stateMachine.ChangeState(player.CrouchIdleState);
        }
        else if (yInput != -1 && !isTouchingCeiling) {
            player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig);
            // player.CalculateColliderHeight(playerData.standColliderHeight);
            stateMachine.ChangeState(player.MoveState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (!isOnSlope) {
            player.SetVelocityX(lastXInput * playerData.crouchWalkSpeed, playerData.crouchAcceleration, playerData.lerpVelocity);
            player.SetVelocityY(player.CurrentVelocity.y);
        }
        else if (isOnSlope) {
            player.SetVelocityX(-lastXInput * playerData.crouchWalkSpeed * slopeNormalPerpendicular.x, playerData.crouchAcceleration, playerData.lerpVelocity);
            player.SetVelocityYOnGround(-lastXInput * playerData.crouchWalkSpeed * slopeNormalPerpendicular.y);
        }
    }
}
