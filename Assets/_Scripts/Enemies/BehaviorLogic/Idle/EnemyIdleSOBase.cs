using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

[CreateAssetMenu(fileName = "Enemy Idle", menuName = "Enemy Logic/Idle Logic/Still Idle")]
public class EnemyIdleSOBase : EnemyStateSOBase {
    protected float acumulatedTime = 0f;
    protected float randomIdleTimer;
    [SerializeField] protected float idleTimerRange = 1f;
    protected float randomFaceDirectionTimer;
    [SerializeField] protected float randomFaceDirectionRange = 0.5f;

    protected int lastIdleDirection;

    public override void DoEnterLogic() {
        acumulatedTime = 0f;
        randomIdleTimer = 0f;
        randomFaceDirectionTimer = 0f;

        enemy.CheckFacingDirection(enemy.FacingDirection);
        
        lastIdleDirection = enemy.FacingDirection;
    }

    public override void DoExitLogic() {
    }

    public override void DoUpdateLogic() {
        randomFaceDirectionTimer += Time.deltaTime;

        if (randomFaceDirectionTimer >= randomFaceDirectionRange) {  
            enemy.CheckFacingDirection(Utils.CoinFlip(-1, 1));
            randomFaceDirectionTimer = 0f;
        }

        if (enemyData.doesPatrol && !enemyData.infiniteIdle) {
            acumulatedTime += Time.deltaTime;

            if (acumulatedTime >= enemyData.idleTimeRange) {
                randomIdleTimer += Time.deltaTime;
                enemy.CheckFacingDirection(lastIdleDirection);

                if (randomIdleTimer >= idleTimerRange) {
                    enemy.StateMachine.ChangeState(enemy.PatrolState);
                }
            }
        }
    }

    public override void DoFixedUpdateLogic() {
        enemy.SetVelocityY(enemy.CurrentVelocity.y);

        if (enemyData.enableFriction) {
            enemy.SetVelocityX(0f, enemyData.runDecceleration * enemyData.frictionAmount, enemyData.lerpVelocity);
        }
        else {
            enemy.SetVelocityX(0f, enemyData.runDecceleration, enemyData.lerpVelocity);
        }
    }
}
