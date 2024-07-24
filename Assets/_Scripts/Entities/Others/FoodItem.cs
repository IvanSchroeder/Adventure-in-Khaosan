using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ExtensionMethods;

public class FoodItem : Consumable {
    public FoodItemData FoodItemData;

    public static Action<FoodItem> OnItemCreated;
    public static Action<FoodItem> OnItemPickup;

    protected override void Start() {
        base.Start();
        OnItemCreated?.Invoke(this);
    }

    public override void Pickup(object sender, OnEntityInteractedEventArgs entityInteracted) {
        OnItemPickup?.Invoke(this);

        itemPickupRoutine = StartCoroutine(ItemPickupRoutine());
    }

    public void DestroyItem() {
        this.gameObject.SetActive(false);
    }

    protected override IEnumerator ItemPickupRoutine() {
        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = currentPosition + (Vector2.up * pickupOffset);
        float elapsedTime = 0f;
        float percentage = elapsedTime / pickupSpeed;

        while (currentPosition != targetPosition) {
            currentPosition = Vector2.MoveTowards(currentPosition, targetPosition, pickupAnimationCurve.Evaluate(percentage));
            transform.position = currentPosition;

            elapsedTime += Time.deltaTime;
            percentage = elapsedTime / pickupSpeed;
            yield return null;
        }

        transform.position = targetPosition;

        yield return pickupDelay;

        ConsumableAnimator.SetBool("picked", true);
        
        OnItemPickup?.Invoke(this);

        yield return null;
    }
}
