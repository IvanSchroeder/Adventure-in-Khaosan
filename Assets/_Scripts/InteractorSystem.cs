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

        InteractablesList.Add(interactableSystem);

        OnEntityInteractedEventArgs entityInteracted = new OnEntityInteractedEventArgs();
        entityInteracted.ContactPoint = collision.transform.position;

        if (!interactableSystem.RequiresInput) {
            interactableSystem.Interact(entityInteracted);
        }
        else {
            interactableSystem.SetInteractionState(entityInteracted, true);
        }
    }

    public void OnTriggerStay2D(Collider2D collision) {
        if (!CanInteract || !collision.HasComponentInHierarchy<IInteractable>()) return;

        if (InteractablesList.Count > 0) {
            foreach (InteractableSystem interactable in InteractablesList) {
                OnEntityInteractedEventArgs entityInteracted = new OnEntityInteractedEventArgs();
                entityInteracted.ContactPoint = collision.transform.position;

                if (interactable.RequiresInput && CanInteract && Interactor.InputHandler.InteractInput) {
                    interactable.Interact(entityInteracted);
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
        
        OnEntityInteractedEventArgs entityInteracted = new OnEntityInteractedEventArgs();
        interactableSystem.SetInteractionState(entityInteracted, false);
        Interactor.InputHandler.UseInteractInput();
    }
}
