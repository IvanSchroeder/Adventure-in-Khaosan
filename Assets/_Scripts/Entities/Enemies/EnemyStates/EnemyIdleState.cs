using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : EnemyState {
    public EnemyIdleState(Enemy enemy, StateMachine stateMachine, EnemyData enemyData, string animBoolName) : base(enemy, stateMachine, enemyData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();
        enemy.isIdle = true;
        enemy.EnemyIdleSOBaseInstance.DoEnterLogic();
    }

    public override void Exit() {
        base.Exit();
        enemy.isIdle = false;
        enemy.EnemyIdleSOBaseInstance.DoExitLogic();
    }

    public override void LogicUpdate() {
        base.LogicUpdate();
        enemy.EnemyIdleSOBaseInstance.DoUpdateLogic();
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();
        enemy.EnemyIdleSOBaseInstance.DoFixedUpdateLogic();
    }

    public override void AnimationTrigger() {
        base.AnimationTrigger();
        enemy.EnemyIdleSOBaseInstance.DoAnimationTriggerLogic();
    } 
        
    public override void AnimationFinishTrigger() {
        base.AnimationFinishTrigger();
        enemy.EnemyIdleSOBaseInstance.DoEnterLogic();
    }
}
