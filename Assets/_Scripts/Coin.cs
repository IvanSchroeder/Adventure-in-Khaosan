using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ExtensionMethods;

public class Coin : Consumable {
    public float pickupOffset;
    public float pickupSpeed;
    public float pickupDelaySeconds;
    public AnimationCurve pickupAnimationCurve;

    public static Action<Coin> OnCoinCreated;
    public static Action<Coin> OnCoinPickup;

    private WaitForSeconds pickupDelay;
    private Coroutine coinPickupCoroutine;

    private void Start() {
        pickupDelay = new WaitForSeconds(pickupDelaySeconds);
        SpriteRenderer.enabled = true;
        OnCoinCreated?.Invoke(this);
    }

    public override void Pickup(object sender, OnEntityInteractedEventArgs entityInteracted) {
        base.Pickup(sender, entityInteracted);

        OnCoinPickup?.Invoke(this);

        coinPickupCoroutine = StartCoroutine(CoinPickupRoutine());
    }

    private IEnumerator CoinPickupRoutine() {
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
        SpriteRenderer.enabled = false;
        // create particles

        yield return null;
    }
}
