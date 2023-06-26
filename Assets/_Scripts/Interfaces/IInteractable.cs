using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public interface IInteractable {
    BoxCollider2D InteractTrigger { get; set; }
    Material InteractableOutlineMaterial { get; set; }
    SpriteFlashConfiguration InteractedFlash { get; set; }
    bool RequiresInput { get; set; }
    bool IsInteractable { get; set; }

    void Interact();
    void SetInteraction(bool enable);
    // void OnTriggerEnter2D(Collider2D collision);
    // void OnTriggerStay2D(Collider2D collision);
    // void OnTriggerExit2D(Collider2D collision);
}
