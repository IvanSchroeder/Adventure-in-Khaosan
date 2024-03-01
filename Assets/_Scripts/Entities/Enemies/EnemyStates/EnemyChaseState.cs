using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseState : EnemyState {
    public EnemyChaseState(Enemy enemy, StateMachine stateMachine, EnemyData enemyData, string animBoolName) : base(enemy, stateMachine, enemyData, animBoolName) {
    }

    public override void Enter() {
        Debug.Log($"{enemy.name} entered Chase State");

        base.Enter();
        enemy.isChasing = true;
        enemy.EnemyChaseSOBaseInstance.DoEnterLogic();
    }

    public override void Exit() {
        Debug.Log($"{enemy.name} exited Chase State");

        base.Exit();
        enemy.isChasing = false;
        enemy.EnemyChaseSOBaseInstance.DoExitLogic();
    }

    public override void LogicUpdate() {
        base.LogicUpdate();
        enemy.EnemyChaseSOBaseInstance.DoUpdateLogic();
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();
        enemy.EnemyChaseSOBaseInstance.DoFixedUpdateLogic();
    }

    public override void AnimationTrigger() {
        base.AnimationTrigger();
        enemy.EnemyChaseSOBaseInstance.DoAnimationTriggerLogic();
    } 
        
    public override void AnimationFinishTrigger() {
        base.AnimationFinishTrigger();
        enemy.EnemyChaseSOBaseInstance.DoEnterLogic();
    }
}
