using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerCrouchIdleState : PlayerGroundedState {
    protected float cameraOffsetDelay = 1f;

    public PlayerCrouchIdleState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        isCrouching = true;
        player.SetColliderParameters(player.MovementCollider, playerData.crouchColliderConfig);

        player.CameraTarget.SetTargetPosition(Vector3.down, 3f, true);
    }

    public override void Exit() {
        base.Exit();

        isCrouching = false;

        player.CameraTarget.SetTargetPosition(Vector3.zero, 0f, true);
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        player.SetColliderParameters(player.MovementCollider, playerData.crouchColliderConfig);

        if (isExitingState) return;

        if (xInput != 0) {
            player.CheckFacingDirection(xInput);

            if(playerData.canMove && !isTouchingWall) stateMachine.ChangeState(player.CrouchMoveState);
            else if (yInput != -1 && !isTouchingCeiling) {
                player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig);
                stateMachine.ChangeState(player.IdleState);
            }
        }
        else if (yInput != -1 && !isTouchingCeiling) {
            player.SetColliderParameters(player.MovementCollider, playerData.standingColliderConfig);
            stateMachine.ChangeState(player.IdleState);
        }
        else if (isTouchingCeiling) {
            if (yInput == -1) {
                player.CameraTarget.SetTargetPosition(Vector3.down, 3f, true);
            }
            else if (yInput == 0f) {
                player.CameraTarget.SetTargetPosition(Vector3.zero, 0f, true);
            }
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        player.SetVelocityX(0f, playerData.crouchDecceleration, playerData.lerpVelocity);
        player.SetVelocityY(player.CurrentVelocity.y);
    }
}
