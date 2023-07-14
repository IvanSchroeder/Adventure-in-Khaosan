using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public class InteractorSystem : MonoBehaviour {
    public IInteractor Interactor;

    [field: SerializeField] public bool CanInteract { get; set; } = true;
    public List<InteractableSystem> InteractablesList = new List<InteractableSystem>();

    private void Awake() {
        if (Interactor.IsNull()) Interactor = this.GetComponentInHierarchy<IInteractor>();

        CanInteract = true;
    } 

    public void OnTriggerEnter2D(Collider2D collision) {
        if (!CanInteract || !collision.HasComponentInHierarchy<IInteractable>()) return;

        InteractableSystem interactableSystem = collision.GetComponentInHierarchy<InteractableSystem>();
        IInteractable interactableEntity = interactableSystem.Interactable;
        interactableEntity.CurrentInteractor = Interactor;

        InteractablesList.Add(interactableSystem);

        if (!interactableSystem.RequiresInput) {
            interactableSystem.Interact();
        }
        else interactableSystem.SetInteractionState(true);
    }

    public void OnTriggerStay2D(Collider2D collision) {
        if (!CanInteract || !collision.HasComponentInHierarchy<IInteractable>()) return;

        if (InteractablesList.Count > 0) {
            foreach (InteractableSystem interactable in InteractablesList) {
                if (interactable.RequiresInput && CanInteract && Interactor.InputHandler.InteractInput) {
                    interactable.Interact();
                    interactable.WasInteracted = true;
                }
            }

            for (int i = 0; i < InteractablesList.Count;) {
                if (InteractablesList[i].WasInteracted) {
                    InteractablesList.Remove(InteractablesList[i]);
                }
                else {
                    i++;
                }
            }
        }

        Interactor.InputHandler.UseInteractInput();
    }

    public void OnTriggerExit2D(Collider2D collision) {
        if (!CanInteract || !collision.HasComponentInHierarchy<IInteractable>()) return;

        InteractableSystem interactableSystem = collision.GetComponentInHierarchy<InteractableSystem>();
        IInteractable interactableEntity = interactableSystem.Interactable;

        if (interactableSystem.WasInteracted) interactableSystem.InteractionStop();
        
        interactableSystem.SetInteractionState(false);
        interactableEntity.CurrentInteractor = null;
        Interactor.InputHandler.UseInteractInput();
    }

    // public void OnTriggerEnter2D(Collider2D collision) {
    //     InteractableSystem interactableSystem = collision.GetComponentInHierarchy<InteractableSystem>();

    //     if (!CanInteract || interactableSystem.IsNull()) return;

    //     IInteractable interactableEntity = interactableSystem.Interactable;

    //     Interactor.CurrentInteractable = interactableEntity;
    //     interactableEntity.CurrentInteractor = Interactor;

    //     if (!interactableSystem.RequiresInput) Interactor.CurrentInteractable.InteractableSystem.Interact();
    //     else interactableSystem.SetInteractionState(true);
    // }

    // public void OnTriggerStay2D(Collider2D collision) {
    //     if (!CanInteract || !collision.HasComponentInHierarchy<IInteractable>()) return;

    //     if (Interactor.CurrentInteractable.InteractableSystem.RequiresInput && CanInteract && Interactor.InputHandler.InteractInput) {
    //         Interactor.InputHandler.UseInteractInput();
    //         Interactor.CurrentInteractable.InteractableSystem.Interact();
    //     }
    // }

    // public void OnTriggerExit2D(Collider2D collision) {
    //     InteractableSystem interactableSystem = collision.GetComponentInHierarchy<InteractableSystem>();

    //     if (!CanInteract || interactableSystem.IsNull()) return;

    //     IInteractable interactableEntity = interactableSystem.Interactable;

    //     if (Interactor.CurrentInteractable.InteractableSystem.WasInteracted) Interactor.CurrentInteractable.InteractableSystem.InteractionStop();
    //     Interactor.CurrentInteractable.InteractableSystem.SetInteractionState(false);
    //     Interactor.CurrentInteractable = null;
    //     interactableEntity.CurrentInteractor = null;
    //     Interactor.InputHandler.UseInteractInput();
    // }
}
