using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ExtensionMethods;

public class Consumable : MonoBehaviour, IInteractable {
    [field: SerializeField] public BoxCollider2D InteractTrigger { get; set; }
    [field: SerializeField] public InteractableSystem InteractableSystem { get; set; }
    [field: SerializeField] public SpriteManager SpriteManager { get; set; }

    [field: SerializeField] public Animator ConsumableAnimator { get; set; }
    [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }

    public virtual void OnEnable() {
        InteractableSystem.OnInteracted += Pickup;
    }

    public virtual void OnDisable() {
        InteractableSystem.OnInteracted -= Pickup;
    }

    public virtual void Pickup(object sender, OnEntityInteractedEventArgs entityInteracted) {
        //ConsumableAnimator.SetTrigger("picked");
    }
}
