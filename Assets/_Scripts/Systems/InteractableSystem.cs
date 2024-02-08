using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public class InteractableSystem : MonoBehaviour {
    public IInteractable Interactable;
    
    [field: SerializeField] public SpriteFlashConfiguration InteractedFlash { get; set; }
    [field: SerializeField] public bool RequiresInput { get; set; }
    [field: SerializeField] public bool FlashesOnInteract { get; set; }
    [field: SerializeField] public bool FlashesOnInteractStop { get; set; }
    [field: SerializeField] public bool OneTimeIntectarion { get; set; }
    [field: SerializeField] public bool IsInteractable { get; set; }
    [field: SerializeField] public bool WasInteracted { get; set; }
    [field: SerializeField] public bool IsToggable { get; set; }
    [field: SerializeField] public bool HasDelay { get; set; }
    [field: SerializeField] public float InteractionStopDelay { get; set; }

    public EventHandler<OnEntityInteractedEventArgs> OnInteracted;
    public EventHandler<OnEntityInteractedEventArgs> OnInteractionStop;
    public EventHandler<OnEntityInteractedEventArgs> OnInteractionState;

    private Coroutine interactionStopCoroutine;

    private void Awake() {
        if (Interactable == null) Interactable = this.GetComponentInHierarchy<IInteractable>();

        WasInteracted = false;
        IsInteractable = true;
    }

    public void Interact(OnEntityInteractedEventArgs entityInteracted) {
        if (!IsInteractable) return;

        if (OneTimeIntectarion) IsInteractable = false;
        WasInteracted = true;

        entityInteracted.IsInteracted = IsInteractable;
        entityInteracted.ShouldFlash = FlashesOnInteract;
        entityInteracted.CurrentFlash = InteractedFlash;

        SetInteractionState(entityInteracted, IsInteractable);

        OnInteracted?.Invoke(this, entityInteracted);
    }

    public void InteractionStop() {
        if (!IsInteractable) return;

        interactionStopCoroutine = StartCoroutine(InteractionStopRoutine());
    }

    public IEnumerator InteractionStopRoutine() {
        if (HasDelay) yield return new WaitForSeconds(InteractionStopDelay);

        if (WasInteracted) WasInteracted = false;

        OnEntityInteractedEventArgs entityInteracted = new OnEntityInteractedEventArgs();
        entityInteracted.IsInteracted = IsInteractable;
        entityInteracted.ShouldFlash = FlashesOnInteractStop;
        entityInteracted.CurrentFlash = InteractedFlash;

        OnInteractionStop?.Invoke(this, entityInteracted);

        yield return null;
    }

    public void SetInteractionState(OnEntityInteractedEventArgs entityInteracted, bool enable) {
        entityInteracted.ActiveOutline = enable;

        OnInteractionState?.Invoke(this, entityInteracted);
    }
}
