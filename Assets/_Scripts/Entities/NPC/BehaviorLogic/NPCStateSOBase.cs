using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class NPCStateSOBase : ScriptableObject {
    protected NPC npc;
    protected NPCData npcData;
    protected Transform transform;
    protected GameObject gameObject;
    protected NPCState npcState;

    protected Transform playerTransform;

    public virtual void Initialize(GameObject gameObject, NPC npc, NPCData npcData, NPCState npcState) {
        this.gameObject = gameObject;
        this.transform = gameObject.transform;
        this.npc = npc;
    
        this.npcData = npcData;
        this.npcState = npcState;
    }

    public virtual void DoEnterLogic() {}

    public virtual void DoExitLogic() { ResetValues(); }

    public virtual void DoUpdateLogic() {}

    public virtual void DoFixedUpdateLogic() {}

    public virtual void DoAnimationTriggerLogic() {}

    public virtual void DoAnimationFinishLogic() {}

    public virtual void ResetValues() {}
}
