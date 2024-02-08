using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : EnemyState {
    public EnemyIdleState(Enemy enemy, StateMachine stateMachine, EnemyData enemyData, string animBoolName) : base(enemy, stateMachine, enemyData, animBoolName) {
    }

    public override void Enter() {
        Debug.Log($"{enemy.name} entered Idle State");

        base.Enter();
        isIdle = true;
        enemy.EnemyIdleSOBaseInstance.DoEnterLogic();
    }

    public override void Exit() {
        Debug.Log($"{enemy.name} exited Idle State");

        base.Exit();
        isIdle = false;
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
