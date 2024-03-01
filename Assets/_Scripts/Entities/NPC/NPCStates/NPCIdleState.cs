using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCIdleState : NPCState {
    public NPCIdleState(NPC npc, StateMachine stateMachine, NPCData npcData, string animBoolName) : base(npc, stateMachine, npcData, animBoolName) {
    }

    public override void Enter() {
        base.Enter();
        npc.isIdle = true;
        npc.NPCIdleSOBaseInstance.DoEnterLogic();
    }

    public override void Exit() {
        base.Exit();
        npc.isIdle = false;
        npc.NPCIdleSOBaseInstance.DoExitLogic();
    }

    public override void LogicUpdate() {
        base.LogicUpdate();
        npc.NPCIdleSOBaseInstance.DoUpdateLogic();
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();
        npc.NPCIdleSOBaseInstance.DoFixedUpdateLogic();
    }

    public override void AnimationTrigger() {
        base.AnimationTrigger();
        npc.NPCIdleSOBaseInstance.DoAnimationTriggerLogic();
    } 
        
    public override void AnimationFinishTrigger() {
        base.AnimationFinishTrigger();
        npc.NPCIdleSOBaseInstance.DoEnterLogic();
    }
}
