using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrouchIdleState : PlayerGroundedState {
    public PlayerCrouchIdleState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        isCrouching = true;
        //player.SetColliderHeight(playerData.crouchColliderHeight);
    }

    public override void Exit() {
        base.Exit();

        isCrouching = false;
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (isExitingState) return;

        player.SetColliderHeight(playerData.crouchColliderHeight);

        if (xInput != 0) {
            player.CheckFacingDirection(xInput);
            if(!isTouchingWall) stateMachine.ChangeState(player.CrouchMoveState);
            else if (yInput != -1 && !isTouchingCeiling) {
                player.SetColliderHeight(playerData.standColliderHeight);
                stateMachine.ChangeState(player.IdleState);
            }
        }
        else if (yInput != -1 && !isTouchingCeiling) {
            player.SetColliderHeight(playerData.standColliderHeight);
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        player.SetVelocityX(xInput * playerData.crouchWalkSpeed, playerData.crouchDecceleration, playerData.lerpVelocity);
        player.SetVelocityY(player.CurrentVelocity.y);
    }
}
