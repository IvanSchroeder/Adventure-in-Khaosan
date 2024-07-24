using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

[CreateAssetMenu(fileName = "NPC Move", menuName = "NPC Logic/Move Logic/Wander to Point")]
public class NPCMoveSOBase : NPCStateSOBase {
    private Vector2 nextWanderPosition;
    private float distanceToSpawnPoint;

    protected float acumulatedTime = 0f;

    protected int lastPatrolDirection = 1;

    public override void DoEnterLogic() {
        acumulatedTime = 0f;

        npc.CheckFacingDirection(lastPatrolDirection);
    }

    public override void DoExitLogic() {
        lastPatrolDirection = npc.FacingDirection;
    }

    public override void DoUpdateLogic() {
        npc.CheckFacingDirection(npc.FacingDirection);

        distanceToSpawnPoint = (npc.transform.position.x - npc.spawnPosition.x).AbsoluteValue();

        if (npc.isGrounded && (npc.isTouchingWall || !npc.isTouchingLedgeWithFoot)) {
            npc.Flip();
            npc.SetVelocityX(npc.FacingDirection * npcData.moveSpeed);
        }

        if (npc.isGrounded && npcData.hasLimitWander && distanceToSpawnPoint >= npcData.wanderLimitRadius) {
            npc.Flip();
            npc.SetVelocityX(npc.FacingDirection * npcData.moveSpeed);
            npc.transform.position.SetX(npc.spawnPosition.x + ((npcData.wanderLimitRadius - 0.1f) * distanceToSpawnPoint.Sign()));
        }

        if (npc.isNearPlayer) {
            npc.StateMachine.ChangeState(npc.IdleState);
        }
        else if (npcData.doesIdle && !npcData.infinitePatrol) {
            acumulatedTime += Time.deltaTime;

            if (acumulatedTime >= npcData.wanderTimeRange) {
                npc.StateMachine.ChangeState(npc.IdleState);
            }
        }
    }

    public override void DoFixedUpdateLogic() {
        npc.SetVelocityX(npc.FacingDirection * npcData.moveSpeed, npcData.runAcceleration, npcData.lerpVelocity);

        npc.SetVelocityY(npc.CurrentVelocity.y);
    }
}
