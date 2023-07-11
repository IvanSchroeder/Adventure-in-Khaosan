using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public class InteractorSystem : MonoBehaviour {
    public IInteractor Interactor;

    [field: SerializeField] public bool CanInteract { get; set; } = true;

    private void Awake() {
        if (Interactor.IsNull()) Interactor = this.GetComponentInHierarchy<IInteractor>();

        CanInteract = true;
    } 

    public void OnTriggerEnter2D(Collider2D collision) {
        InteractableSystem interactableSystem = collision.GetComponentInHierarchy<InteractableSystem>();

        if (!CanInteract || interactableSystem.IsNull()) return;

        IInteractable interactableEntity = interactableSystem.Interactable;

        Interactor.CurrentInteractable = interactableEntity;
        interactableEntity.CurrentInteractor = Interactor;

        if (!interactableSystem.RequiresInput) Interactor.CurrentInteractable.InteractableSystem.Interact();
        else interactableSystem.SetInteractionState(true);
    }

    public void OnTriggerStay2D(Collider2D collision) {
        if (!CanInteract || !collision.HasComponentInHierarchy<IInteractable>()) return;

        if (Interactor.CurrentInteractable.InteractableSystem.RequiresInput && CanInteract && Interactor.InputHandler.InteractInput) {
            Interactor.InputHandler.UseInteractInput();
            Interactor.CurrentInteractable.InteractableSystem.Interact();
        }
    }

    public void OnTriggerExit2D(Collider2D collision) {
        InteractableSystem interactableSystem = collision.GetComponentInHierarchy<InteractableSystem>();

        if (!CanInteract || interactableSystem.IsNull()) return;

        IInteractable interactableEntity = interactableSystem.Interactable;

        if (Interactor.CurrentInteractable.InteractableSystem.WasInteracted) Interactor.CurrentInteractable.InteractableSystem.InteractionStop();
        Interactor.CurrentInteractable.InteractableSystem.SetInteractionState(false);
        Interactor.CurrentInteractable = null;
        interactableEntity.CurrentInteractor = null;
        Interactor.InputHandler.UseInteractInput();
    }
}
