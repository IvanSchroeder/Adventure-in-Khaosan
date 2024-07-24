using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolState : EnemyState {
    public EnemyPatrolState(Enemy enemy, StateMachine stateMachine, EnemyData enemyData, string animBoolName) : base(enemy, stateMachine, enemyData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();
        enemy.isPatroling = true;
        enemy.EnemyPatrolSOBaseInstance.DoEnterLogic();
    }

    public override void Exit() {
        base.Exit();
        enemy.isPatroling = false;
        enemy.EnemyPatrolSOBaseInstance.DoExitLogic();
    }

    public override void LogicUpdate() {
        base.LogicUpdate();
        enemy.EnemyPatrolSOBaseInstance.DoUpdateLogic();
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();
        enemy.EnemyPatrolSOBaseInstance.DoFixedUpdateLogic();
    }

    public override void AnimationTrigger() {
        base.AnimationTrigger();
        enemy.EnemyPatrolSOBaseInstance.DoAnimationTriggerLogic();
    } 
        
    public override void AnimationFinishTrigger() {
        base.AnimationFinishTrigger();
        enemy.EnemyPatrolSOBaseInstance.DoEnterLogic();
    }
}
