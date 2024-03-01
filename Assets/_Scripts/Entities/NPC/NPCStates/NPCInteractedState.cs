using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractedState : NPCState {
    public NPCInteractedState(NPC npc, StateMachine stateMachine, NPCData npcData, string animBoolName) : base(npc, stateMachine, npcData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();
        npc.isInteracted = true;
        npc.NPCInteractedSOBaseInstance.DoEnterLogic();
    }

    public override void Exit() {
        base.Exit();
        npc.isInteracted = false;
        npc.NPCInteractedSOBaseInstance.DoExitLogic();
    }

    public override void LogicUpdate() {
        base.LogicUpdate();
        npc.NPCInteractedSOBaseInstance.DoUpdateLogic();
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();
        npc.NPCInteractedSOBaseInstance.DoFixedUpdateLogic();
    }

    public override void AnimationTrigger() {
        base.AnimationTrigger();
        npc.NPCInteractedSOBaseInstance.DoAnimationTriggerLogic();
    } 
        
    public override void AnimationFinishTrigger() {
        base.AnimationFinishTrigger();
        npc.NPCInteractedSOBaseInstance.DoEnterLogic();
    }
}
