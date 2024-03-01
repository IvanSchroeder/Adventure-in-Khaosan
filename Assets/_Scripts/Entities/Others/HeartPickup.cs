using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ExtensionMethods;

public class HeartPickup : Consumable {
    public IntSO heartsToRestore;
    public static Action<int> OnHeartPickup;

    public override void Pickup(object sender, OnEntityInteractedEventArgs entityInteracted) {
        if (LevelManager.instance.PlayerInstance.entityData.isMaxHealth) {
            entityInteracted.IsInteracted = false;
            entityInteracted.InteractableSystemSource.WasInteracted = false;
            entityInteracted.InteractableSystemSource.IsInteractable = true;
            return;
        }

        OnHeartPickup?.Invoke(heartsToRestore.Value);

        itemPickupRoutine = StartCoroutine(ItemPickupRoutine());
    }
}
