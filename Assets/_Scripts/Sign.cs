using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;
using TMPro;
using UnityEngine.InputSystem;

public class Sign : MonoBehaviour, IInteractable {
    [field: SerializeField] public BoxCollider2D InteractTrigger { get; set; }
    [field: SerializeField] public InteractableSystem InteractableSystem { get; set; }
    [field: SerializeField] public SpriteManager SpriteManager { get; set; }
    [field: SerializeField] public IInteractor CurrentInteractor { get; set; }

    [field: SerializeField] public Canvas SignCanvas { get; set; }
    [field: SerializeField] public Transform TextOrigin { get; set; }
    [field: SerializeField] public Transform TextTarget { get; set; }
    [field: SerializeField] public TextMeshProUGUI SignText { get; set; }
    [field: SerializeField] public float TextTransitionTime { get; set; }
    public AnimationCurve TextEnablingCurve;

    public SpriteRenderer SpriteRenderer { get; private set; }

    public bool isActive = false;

    private Coroutine textEnablingCoroutine;

    public EventHandler<OnEntityInteractedEventArgs> OnInteracted;

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
        if (InteractTrigger == null) InteractTrigger = GetComponentInChildren<BoxCollider2D>();
        if (SpriteRenderer == null) SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (SpriteManager == null) SpriteManager = SpriteRenderer.GetComponent<SpriteManager>();

        SignText.transform.position = TextOrigin.position;
        SignText.color = SignText.color.SetA(0f);
        SignText.enabled = false;
        isActive = false;

        // index = 0;
        // SignText.text = string.Empty;
        // StartDialogue();
    }

    // void StartDialogue() {
    //     StartCoroutine(TypeLineRoutine());
    // }

    public void EnableSign(object sender, OnEntityInteractedEventArgs entityInteracted) {
        if (textEnablingCoroutine != null) {
            StopCoroutine(textEnablingCoroutine);
            textEnablingCoroutine = null;
        }

        isActive = true;

        textEnablingCoroutine = StartCoroutine(TextEnablingRoutine(isActive));
        Debug.Log($"Interacted with {this.transform.name}");
    }

    public void DisableSign(object sender, OnEntityInteractedEventArgs entityInteracted) {
        if (textEnablingCoroutine != null) {
            StopCoroutine(textEnablingCoroutine);
            textEnablingCoroutine = null;
        }

        isActive = false;

        textEnablingCoroutine = StartCoroutine(TextEnablingRoutine(isActive));
    }

    public IEnumerator TextEnablingRoutine(bool enable) {
        SignText.enabled = true;

        float elapsedTime = 0f;
        float percentage = elapsedTime / TextTransitionTime;
        float alpha = SignText.color.a;
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
            SignText.transform.position = Vector2.Lerp(SignText.transform.position, targetPosition, TextEnablingCurve.Evaluate(percentage));
            alpha = Mathf.Lerp(alpha, targetAlpha, TextEnablingCurve.Evaluate(percentage));
            SignText.color = SignText.color.SetA(alpha);

            elapsedTime += Time.deltaTime;
            percentage = elapsedTime / TextTransitionTime;

            yield return null;
        }

        SignText.transform.position = targetPosition;
        SignText.color = SignText.color.SetA(targetAlpha);
        SignText.enabled = enable;

        yield return null;
    }

    public string[] lines;
    public float textSpeed = 0.1f;
    private int index;

    void StartDialog() {
        index = 0;
    }

    // IEnumerator TypeLineRoutine() {
    //     foreach (char c in lines[index].ToCharArray()) {
    //         SignText.text += c;
    //         yield return new WaitForSeconds(textSpeed);
    //     }
    // }

    // void NextLine() {
    //     if (index < lines.Length - 1) {
    //         index++;
    //         textComponent.text = string.Empty;
    //         StartCoroutine(TypeLineRoutine());
    //     }
    //     else {
    //         gameObject.SetActive(false);
    //     }
    // }

    // void Update() {
    //     if (Mouse.current.leftButton.wasPressedThisFrame) {
    //         if (SignText.text == lines[index]) {
    //             NextLine();
    //         }
    //     }
    //     else {
    //         StopAllCoroutines();
    //         SignText.text = lines[index];
    //     }
    // }
}
