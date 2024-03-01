using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerCrouchMoveState : PlayerGroundedState {
    public PlayerCrouchMoveState(Player player, StateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        isCrouching = true;
        player.isMoving = true;
        standUp = false;
        player.SetColliderParameters(player.MovementCollider, playerData.crouchColliderConfig, true);
        player.SetColliderParameters(player.HitboxTrigger, playerData.crouchColliderConfig);
    }

    public override void Exit() {
        base.Exit();

        isCrouching = false;
        player.isMoving = false;
        standUp = false;
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        player.CheckFacingDirection(xInput);

        if (isExitingState) return;

        if (xInput == 0 && player.CurrentVelocity.x < player.CurrentVelocity.x.Sign()) {
            stateMachine.ChangeState(player.CrouchIdleState);
        }
        else if (standUp) {
            player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig, true);
            stateMachine.ChangeState(player.MoveState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (!crouchInputHold && player.CheckForSpace(player.GroundPoint.position.ToVector2() + Vector2.up * 0.015f, Vector2.up, 1.1f) /*|| !isTouchingCeiling*/) {
            standUp = true;
        }

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
