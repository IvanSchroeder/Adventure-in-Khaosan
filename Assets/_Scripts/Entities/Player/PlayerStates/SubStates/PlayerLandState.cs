using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PlayerLandState : PlayerGroundedState {
    protected Vector2 targetLandPosition;

    public PlayerLandState(Player player, StateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();

        AudioManager.instance.PlaySFX("PlayerLand");

        // RaycastHit2D groundHit = player.GetGroundHit();
        // targetLandPosition = groundHit.point;
    }

    public override void Exit() {
        base.Exit();
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (playerData.CanMove.Value && xInput != 0)
            stateMachine.ChangeState(player.MoveState);
        else if (player.isAnimationFinished) {
            if (playerData.CanCrouch.Value && yInput == -1)
                stateMachine.ChangeState(player.CrouchIdleState);
            else
                stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        player.SetVelocityX(0f, playerData.runDecceleration, playerData.lerpVelocity);
        player.SetVelocityY(player.CurrentVelocity.y);
    }
}