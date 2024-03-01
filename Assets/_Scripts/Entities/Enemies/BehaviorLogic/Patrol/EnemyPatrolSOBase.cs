using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Patrol", menuName = "Enemy Logic/Patrol Logic/Horizontal Patrol")]
public class EnemyPatrolSOBase : EnemyStateSOBase {
    protected float acumulatedTime = 0f;

    protected int lastPatrolDirection = 1;

    public override void DoEnterLogic() {
        acumulatedTime = 0f;

        enemy.CheckFacingDirection(lastPatrolDirection);
    }

    public override void DoExitLogic() {
        lastPatrolDirection = enemy.FacingDirection;
    }

    public override void DoUpdateLogic() {
        enemy.CheckFacingDirection(enemy.FacingDirection);

        if (enemyData.isGrounded && enemyData.isTouchingWall) {
            enemy.Flip();
            enemy.SetVelocityX(enemy.FacingDirection * enemyData.moveSpeed);
        }

        if (enemyData.doesIdle && !enemyData.infinitePatrol) {
            acumulatedTime += Time.deltaTime;

            if (acumulatedTime >= enemyData.patrolTimeRange) {
                enemy.StateMachine.ChangeState(enemy.IdleState);
            }
        }
    }

    public override void DoFixedUpdateLogic() {
        enemy.SetVelocityX(enemy.FacingDirection * enemyData.moveSpeed, enemyData.runAcceleration, enemyData.lerpVelocity);

        enemy.SetVelocityY(enemy.CurrentVelocity.y);
    }
}
