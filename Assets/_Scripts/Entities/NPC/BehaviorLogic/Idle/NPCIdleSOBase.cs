using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

[CreateAssetMenu(fileName = "NPC Idle", menuName = "NPC Logic/Idle Logic/Still Idle")]
public class NPCIdleSOBase : NPCStateSOBase {
    protected float acumulatedTime = 0f;
    protected float randomIdleTimer;
    [SerializeField] protected float idleTimerRange = 1f;
    protected float randomFaceDirectionTimer;
    [SerializeField] protected float randomFaceDirectionRange = 0.5f;
    protected float secondaryIdleTimer;
    [SerializeField] protected float secondaryIdleTimerRange = 5f;

    protected int lastIdleDirection;

    public override void DoEnterLogic() {
        acumulatedTime = 0f;
        randomIdleTimer = 0f;
        randomFaceDirectionTimer = 0f;

        if (npcData.hasRandomIdle) secondaryIdleTimer = 0f;

        npc.CheckFacingDirection(npc.FacingDirection);

        // npc.Anim.SetBool("idle2", false);
        
        lastIdleDirection = npc.FacingDirection;
    }

    public override void DoExitLogic() {
        // npc.Anim.SetBool("idle2", false);
        npc.Anim.ResetTrigger("idle2");
    }

    public override void DoUpdateLogic() {
        randomFaceDirectionTimer += Time.deltaTime;

        if (npcData.hasRandomIdle) {
            secondaryIdleTimer += Time.deltaTime;

            if (secondaryIdleTimer >= secondaryIdleTimerRange) {
                secondaryIdleTimer = 0f;
                // npc.Anim.SetBool("idle2", true);
                npc.Anim.SetTrigger("idle2");
            }
        }

        if (npc.isNearPlayer) {
            npc.CheckFacingDirection((LevelManager.instance.PlayerInstance.transform.position.x - npc.transform.position.x).Sign());
            // npc.Anim.SetBool("idle2", false);
            randomFaceDirectionTimer = 0f;
        }
        else if (!npcData.hasRandomIdle && randomFaceDirectionTimer >= randomFaceDirectionRange) {  
            npc.CheckFacingDirection(Utils.CoinFlip(-1, 1));
            randomFaceDirectionTimer = 0f;
        }

        if (npcData.doesWander && !npcData.infiniteIdle) {
            acumulatedTime += Time.deltaTime;

            if (acumulatedTime >= npcData.idleTimeRange) {
                randomIdleTimer += Time.deltaTime;
                npc.CheckFacingDirection(lastIdleDirection);

                if (randomIdleTimer >= idleTimerRange) {
                    npc.StateMachine.ChangeState(npc.MoveState);
                }
            }
        }
    }

    public override void DoFixedUpdateLogic() {
        npc.SetVelocityY(npc.CurrentVelocity.y);

        if (npcData.enableFriction) {
            npc.SetVelocityX(0f, npcData.runDecceleration * npcData.frictionAmount, npcData.lerpVelocity);
        }
        else {
            npc.SetVelocityX(0f, npcData.runDecceleration, npcData.lerpVelocity);
        }
    }
}
