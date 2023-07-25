using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Sign : MonoBehaviour, IInteractable {
    [field: SerializeField] public BoxCollider2D InteractTrigger { get; set; }
    [field: SerializeField] public InteractableSystem InteractableSystem { get; set; }
    [field: SerializeField] public SpriteManager SpriteManager { get; set; }

    [field: SerializeField] public Canvas SignCanvas { get; set; }
    [field: SerializeField] public Transform TextOrigin { get; set; }
    [field: SerializeField] public Transform TextTarget { get; set; }
    [field: SerializeField] public TypewriterEffect TypewriterSystem { get; set; }
    [field: SerializeField] public RawImage SignPanel { get; set; }
    [field: SerializeField] public TextMeshProUGUI SignText { get; set; }
    [field: SerializeField] public float TextTransitionTime { get; set; }
    [field: SerializeField] public AnimationCurve TextEnablingCurve { get; set; }
    [field: SerializeField] public bool UnlocksAbility { get; set; }
    [field: SerializeField] public BoolSO AbilityToUnlock { get; set; }

    public SpriteRenderer SpriteRenderer { get; private set; }

    public bool isActive = false;

    private Coroutine textEnablingCoroutine;

    private void OnEnable() {
        InteractableSystem.OnInteracted += EnableSign;
        InteractableSystem.OnInteractionStop += DisableSign;

        SignCanvas.worldCamera = this.GetMainCamera();
    }

    private void OnDisable() {
        InteractableSystem.OnInteracted -= EnableSign;
        InteractableSystem.OnInteractionStop -= DisableSign;
    }

    private void Awake() {
        if (InteractTrigger.IsNull()) InteractTrigger = GetComponentInChildren<BoxCollider2D>();
        if (SpriteRenderer.IsNull()) SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (SpriteManager.IsNull()) SpriteManager = SpriteRenderer.GetComponent<SpriteManager>();
        if (TypewriterSystem.IsNull()) TypewriterSystem = SignText.GetComponent<TypewriterEffect>();

        SignPanel.transform.position = TextOrigin.position;
        SignPanel.color = SignPanel.color.SetA(0f);

        SignText.color = SignText.color.SetA(0f);

        isActive = false;
    }

    public void EnableSign(object sender, OnEntityInteractedEventArgs entityInteracted) {
        if (textEnablingCoroutine != null) {
            StopCoroutine(textEnablingCoroutine);
            textEnablingCoroutine = null;
        }

        isActive = true;

        if (UnlocksAbility) EnableAbility();

        TypewriterSystem.StartTypewrite();
        textEnablingCoroutine = StartCoroutine(TextEnablingRoutine(isActive));
    }

    public void DisableSign(object sender, OnEntityInteractedEventArgs entityInteracted) {
        if (textEnablingCoroutine != null) {
            StopCoroutine(textEnablingCoroutine);
            textEnablingCoroutine = null;
        }

        isActive = false;

        textEnablingCoroutine = StartCoroutine(TextEnablingRoutine(isActive));
    }

    public void EnableAbility() {
        if (AbilityToUnlock.IsNotNull() && !AbilityToUnlock.Value) AbilityToUnlock.Value = true;
    }

    public IEnumerator TextEnablingRoutine(bool enable) {
        float elapsedTime = 0f;
        float percentage = elapsedTime / TextTransitionTime;
        float textAlpha = SignText.color.a;
        float panelAlpha = SignPanel.color.a;
        float targetAlpha = 0f;
        Vector2 targetPosition = Vector2.zero;

        if (enable) {
            targetPosition = TextTarget.position;
            targetAlpha = 1f;
        }
        else {
            targetPosition = TextOrigin.position;
            targetAlpha = 0f;
        }


        while (elapsedTime < TextTransitionTime) {
            textAlpha = Mathf.Lerp(textAlpha, targetAlpha, TextEnablingCurve.Evaluate(percentage));
            SignText.color = SignText.color.SetA(textAlpha);
            
            SignPanel.transform.position = Vector2.Lerp(SignPanel.transform.position, targetPosition, TextEnablingCurve.Evaluate(percentage));
            panelAlpha = Mathf.Lerp(panelAlpha, targetAlpha * 0.5f, TextEnablingCurve.Evaluate(percentage));
            SignPanel.color = SignPanel.color.SetA(panelAlpha);

            elapsedTime += Time.deltaTime;
            percentage = elapsedTime / TextTransitionTime;

            yield return null;
        }

        SignText.color = SignText.color.SetA(targetAlpha);

        SignPanel.transform.position = targetPosition;
        SignPanel.color = SignPanel.color.SetA(targetAlpha * 0.5f);

        if (!enable) {
            TypewriterSystem.StopTypewrite();
            TypewriterSystem.ResetText();
        }

        yield return null;
    }

    public string[] lines;
    public float textSpeed = 0.1f;
    private int index;

    void StartDialog() {
        index = 0;
    }
}
