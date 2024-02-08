using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolState : EnemyState {
    public EnemyPatrolState(Enemy enemy, StateMachine stateMachine, EnemyData enemyData, string animBoolName) : base(enemy, stateMachine, enemyData, animBoolName) {
    }

    public override void Enter() {
        Debug.Log($"{enemy.name} entered Patrol State");

        base.Enter();
        isPatroling = true;
        enemy.EnemyPatrolSOBaseInstance.DoEnterLogic();
    }

    public override void Exit() {
        Debug.Log($"{enemy.name} exited Patrol State");

        base.Exit();
        isPatroling = false;
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
