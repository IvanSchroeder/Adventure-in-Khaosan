using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class BuildingFading : MonoBehaviour, IInteractable {
    [field: SerializeField] public BoxCollider2D InteractTrigger { get; set; }
    [field: SerializeField] public InteractableSystem InteractableSystem { get; set; }
    public SpriteManager SpriteManager { get; set; }

    public List<SpriteManager> SpriteManagers;
    public float fadeDuration;
    private float lastAlpha = 1f;

    private Coroutine FadeCoroutine;

    private void OnEnable() {
        InteractableSystem.OnInteracted += FadeObjects;
        InteractableSystem.OnInteractionStop += UnfadeObjects;
    }

    private void OnDisable() {
        InteractableSystem.OnInteracted -= FadeObjects;
        InteractableSystem.OnInteractionStop -= UnfadeObjects;
    }

    private void Awake() {
        if (InteractTrigger.IsNull()) InteractTrigger = this.GetComponentInHierarchy<BoxCollider2D>();
        if (InteractableSystem.IsNull()) InteractableSystem = this.GetComponentInHierarchy<InteractableSystem>();
        // if (SpriteManager.IsNull()) SpriteManager = this.GetComponentInHierarchy<SpriteManager>();
    }

    public void FadeObjects(object sender, OnEntityInteractedEventArgs args) {
        if (FadeCoroutine.IsNotNull()) {
            StopCoroutine(FadeCoroutine);
            FadeCoroutine = null;
        }

        FadeCoroutine = StartCoroutine(FadeRoutine(0f));
    }

    public void UnfadeObjects(object sender, OnEntityInteractedEventArgs args) {
        if (FadeCoroutine.IsNotNull()) {
            StopCoroutine(FadeCoroutine);
            FadeCoroutine = null;
        }

        FadeCoroutine = StartCoroutine(FadeRoutine(1f));
    }

    public IEnumerator FadeRoutine(float targetAlpha) {
        float elapsedTime = 0f;
        float currentAlpha = lastAlpha;

        while (elapsedTime < fadeDuration) {
            elapsedTime += Time.unscaledDeltaTime;
            
            currentAlpha = currentAlpha.LerpTo(targetAlpha, elapsedTime);
            lastAlpha = currentAlpha;
            ChangeAlphas(currentAlpha);

            yield return null;
        }

        lastAlpha = targetAlpha;

        yield return null;
    }

    private void ChangeAlphas(float currentAlpha) {
        foreach (SpriteManager manager in SpriteManagers) {
            manager.SetAlphaAmount(currentAlpha);
        }
    }
}
