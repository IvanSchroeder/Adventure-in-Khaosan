using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

[CreateAssetMenu(fileName = "NPC Interacted", menuName = "NPC Logic/Interacted Logic/Still Interacted")]
public class NPCInteractedSOBase : NPCStateSOBase {
    public override void DoEnterLogic() {
        npc.CheckFacingDirection(npc.FacingDirection);
    }

    public override void DoExitLogic() {
    }

    public override void DoUpdateLogic() {
        npc.CheckFacingDirection((npc.transform.position.x - playerTransform.position.x).Sign());

        // if not detecting player go back to idle
    }

    public override void DoFixedUpdateLogic() {
        npc.SetVelocityX(0f, npcData.runDecceleration, npcData.lerpVelocity);

        npc.SetVelocityY(npc.CurrentVelocity.y);
    }
}
