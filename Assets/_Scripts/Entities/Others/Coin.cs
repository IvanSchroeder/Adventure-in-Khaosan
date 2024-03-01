using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ExtensionMethods;

public class Coin : Consumable {
    public static Action<Coin> OnCoinCreated;
    public static Action<Coin> OnCoinPickup;

    protected override void Start() {
        base.Start();
        OnCoinCreated?.Invoke(this);
    }

    public override void Pickup(object sender, OnEntityInteractedEventArgs entityInteracted) {
        OnCoinPickup?.Invoke(this);

        itemPickupRoutine = StartCoroutine(ItemPickupRoutine());
    }
}
