using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public class InteractableSystem : MonoBehaviour {
    public IInteractable Interactable;
    
    [field: SerializeField] public SpriteFlashConfiguration InteractedFlash { get; set; }
    [field: SerializeField] public bool RequiresInput { get; set; }
    [field: SerializeField] public bool OneTimeIntectarion { get; set; }
    [field: SerializeField] public bool IsInteractable { get; set; }
    [field: SerializeField] public bool WasInteracted { get; set; }

    public EventHandler<OnEntityInteractedEventArgs> OnInteracted;
    public EventHandler<bool> OnInteractionState;

    private void Awake() {
        if (Interactable == null) Interactable = this.GetComponentInHierarchy<IInteractable>();

        WasInteracted = false;
    }

    public void Interact() {
        if (!IsInteractable) return;

        if (OneTimeIntectarion) IsInteractable = false;
        WasInteracted = true;

        OnEntityInteractedEventArgs entityInteracted = new OnEntityInteractedEventArgs();
        entityInteracted.IsInteracted = IsInteractable;
        entityInteracted.CurrentFlash = InteractedFlash;

        SetInteractionState(IsInteractable);

        OnInteracted?.Invoke(this, new OnEntityInteractedEventArgs());
    }

    public void SetInteractionState(bool enable) {
        OnInteractionState?.Invoke(this, enable);
    }
}
