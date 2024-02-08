using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerCrouchIdleState : PlayerGroundedState {
    protected float cameraOffsetDelay = 1f;

    public PlayerCrouchIdleState(Player player, StateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        isCrouching = true;
        isIdle = true;
        standUp = false;
        player.SetColliderParameters(player.MovementCollider, playerData.crouchColliderConfig, true);
        player.SetColliderParameters(player.HitboxTrigger, playerData.crouchColliderConfig);

        player.CameraTarget.SetTargetPosition(Vector3.down, 3f, true);
    }

    public override void Exit() {
        base.Exit();

        isCrouching = false;
        isIdle = false;
        standUp = false;

        player.CameraTarget.SetTargetPosition(Vector3.zero, 0f, true);
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        if (xInput != 0) {
            player.CheckFacingDirection(xInput);
            
            if(playerData.CanMove.Value)
                stateMachine.ChangeState(player.CrouchMoveState);
        }
        else if (standUp) {
            player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig, true);
            stateMachine.ChangeState(player.IdleState);
        }
        // else if (isTouchingCeiling) {
        //     if (yInput == -1) {
        //         player.CameraTarget.SetTargetPosition(Vector3.down, 3f, true);
        //     }
        //     else if (yInput == 0f) {
        //         player.CameraTarget.SetTargetPosition(Vector3.zero, 0f, true);
        //     }
        // }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        if (!crouchInputHold && player.CheckForSpace(player.GroundPoint.position.ToVector2() + Vector2.up * 0.015f, Vector2.up, 1.1f) /*|| !isTouchingCeiling*/) {
            standUp = true;
        }

        player.SetVelocityX(0f, playerData.crouchDecceleration, playerData.lerpVelocity);
        player.SetVelocityY(player.CurrentVelocity.y);
    }
}
