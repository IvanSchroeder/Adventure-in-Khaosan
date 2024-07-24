using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMoveState : NPCState {
    public NPCMoveState(NPC npc, StateMachine stateMachine, NPCData npcData, string animBoolName) : base(npc, stateMachine, npcData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();
        npc.isMoving = true;
        npc.NPCMoveSOBaseInstance.DoEnterLogic();
    }

    public override void Exit() {
        base.Exit();
        npc.isMoving = false;
        npc.NPCMoveSOBaseInstance.DoExitLogic();
    }

    public override void LogicUpdate() {
        base.LogicUpdate();
        npc.NPCMoveSOBaseInstance.DoUpdateLogic();
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();
        npc.NPCMoveSOBaseInstance.DoFixedUpdateLogic();
    }

    public override void AnimationTrigger() {
        base.AnimationTrigger();
        npc.NPCMoveSOBaseInstance.DoAnimationTriggerLogic();
    } 
        
    public override void AnimationFinishTrigger() {
        base.AnimationFinishTrigger();
        npc.NPCMoveSOBaseInstance.DoEnterLogic();
    }
}
