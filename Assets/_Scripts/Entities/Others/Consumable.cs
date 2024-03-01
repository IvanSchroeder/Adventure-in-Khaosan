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
    [field: SerializeField] public SpriteRenderer SpriteRenderer { get; set; }

    public float pickupOffset;
    public float pickupSpeed;
    public float pickupDelaySeconds;
    public AnimationCurve pickupAnimationCurve;
    protected WaitForSeconds pickupDelay;
    protected Coroutine itemPickupRoutine;

    protected virtual void Awake() {
        if (InteractTrigger.IsNull()) InteractTrigger = this.GetComponentInHierarchy<BoxCollider2D>();
        if (InteractableSystem.IsNull()) InteractableSystem = this.GetComponentInHierarchy<InteractableSystem>();
        if (SpriteManager.IsNull()) SpriteManager = this.GetComponentInHierarchy<SpriteManager>();
        if (ConsumableAnimator.IsNull()) ConsumableAnimator = this.GetComponentInHierarchy<Animator>();
        if (SpriteRenderer.IsNull()) SpriteRenderer = this.GetComponentInHierarchy<SpriteRenderer>();
    }

    protected virtual void Start() {
        pickupDelay = new WaitForSeconds(pickupDelaySeconds);
        SpriteRenderer.enabled = true;
    }

    protected virtual void OnEnable() {
        InteractableSystem.OnInteracted += Pickup;
    }

    protected virtual void OnDisable() {
        InteractableSystem.OnInteracted -= Pickup;
    }

    public virtual void Pickup(object sender, OnEntityInteractedEventArgs entityInteracted) {}

    public virtual void PlaySound(string sound) {
        AudioManager.instance.PlaySFX(sound);
    }

    public virtual void DestroyObject() {
        this.gameObject.SetActive(false);
    }

    protected virtual IEnumerator ItemPickupRoutine() {
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

        yield return null;
    }
}
