using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerCrouchMoveState : PlayerGroundedState {
    public PlayerCrouchMoveState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        isCrouching = true;
        isMoving = true;
        player.SetColliderParameters(player.MovementCollider, playerData.crouchColliderConfig);
        player.SetColliderParameters(player.HitboxTrigger, playerData.crouchColliderConfig);
    }

    public override void Exit() {
        base.Exit();

        isCrouching = false;
        isMoving = false;
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        player.CheckFacingDirection(xInput);

        player.SetColliderParameters(player.MovementCollider, playerData.crouchColliderConfig);
        player.SetColliderParameters(player.HitboxTrigger, playerData.crouchColliderConfig);

        if (isExitingState) return;

        if ((xInput == 0 && player.CurrentVelocity.x == 0f) || (player.CurrentVelocity.x != 0f && isTouchingWall)) {
            stateMachine.ChangeState(player.CrouchIdleState);
        }
        else if (yInput != -1 && !isTouchingCeiling) {
            player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig);
            player.SetColliderParameters(player.HitboxTrigger, playerData.standingColliderConfig);
            stateMachine.ChangeState(player.MoveState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (xInput == 0) {
            player.SetVelocityX(0f, playerData.crouchDecceleration, playerData.lerpVelocity);
        }
        else {
            player.SetVelocityX(xInput * playerData.crouchWalkSpeed, playerData.crouchAcceleration, playerData.lerpVelocity);
        }

        player.SetVelocityY(player.CurrentVelocity.y);

        // if (!isOnSlope) {
        //     player.SetVelocityX(lastXInput * playerData.crouchWalkSpeed, playerData.crouchAcceleration, playerData.lerpVelocity);
        //     player.SetVelocityY(player.CurrentVelocity.y);
        // }
        // else if (isOnSlope) {
        //     player.SetVelocityX(-lastXInput * playerData.crouchWalkSpeed * slopeNormalPerpendicular.x, playerData.crouchAcceleration, playerData.lerpVelocity);
        //     player.SetVelocityYOnGround(-lastXInput * playerData.crouchWalkSpeed * slopeNormalPerpendicular.y);
        // }
    }
}
