using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeathState : EnemyState {
    public EnemyDeathState(Enemy enemy, StateMachine stateMachine, EnemyData enemyData, string animBoolName) : base(enemy, stateMachine, enemyData, animBoolName) {
    }

    public override void Enter() {
        Debug.Log($"{enemy.name} entered Death State");

        base.Enter();
        enemy.isDead = true;
        enemy.EnemyDeathSOBaseInstance.DoEnterLogic();
    }

    public override void Exit() {
        Debug.Log($"{enemy.name} exited Death State");

        base.Exit();
        enemy.isDead = false;
        enemy.EnemyDeathSOBaseInstance.DoExitLogic();
    }

    public override void LogicUpdate() {
        base.LogicUpdate();
        enemy.EnemyDeathSOBaseInstance.DoUpdateLogic();
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();
        enemy.EnemyDeathSOBaseInstance.DoFixedUpdateLogic();
    }

    public override void AnimationTrigger() {
        base.AnimationTrigger();
        enemy.EnemyDeathSOBaseInstance.DoAnimationTriggerLogic();
    } 
        
    public override void AnimationFinishTrigger() {
        base.AnimationFinishTrigger();
        enemy.EnemyDeathSOBaseInstance.DoEnterLogic();
    }
}
