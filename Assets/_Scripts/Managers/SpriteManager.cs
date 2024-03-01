using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;
using System.Linq;
using DG.Tweening;

public class SpriteManager : MonoBehaviour {
    [field: SerializeField] public HealthSystem HealthSystem { get; private set; }
    [field: SerializeField] public IDamageable Damageable { get; private set; }
    [field: SerializeField] public IInteractable Interactable { get; private set; }
    [field: SerializeField] public InteractableSystem InteractableSystem { get; private set; }
    [field: SerializeField] public SpriteFlashConfiguration CurrentFlashConfiguration { get; private set; }

    [field: SerializeField, ColorUsage(true, true)] public Color DefaultFlashColor { get; private set; } = Color.white;
    [field: SerializeField, ColorUsage(true, true)] public Color CurrentFlashColor { get; private set; } = Color.white;
    [field: SerializeField] public float CurrentAlphaAmount { get; private set; } = 1;
    [field: SerializeField] public float DefaultAlphaAmount { get; private set; } = 1;
    [field: SerializeField] public bool IsFlashing { get; private set; } = false;
    [field: SerializeField] public int CurrentColorIndex { get; private set; }
    [field: SerializeField] public int CurrentAmountIndex { get; private set; }
    [field: SerializeField] public int CurrentAlphaIndex { get; private set; }
    [field: SerializeField] public int CurrentAmountOfFlashes { get; private set; }

    [Space(5f)]
    [Header("After Image Properties")]
    [Space(3f)]
    public bool canCreateAfterImage;
    public bool enableAfterImage;
    public float afterImageInterval;
    public float afterImageDecayDelay;
    public float afterImageDecaySpeed;
    public float afterImageStartingAlpha;
    public float afterImageColor;
    [Space(5f)]

    private static readonly int _enableFlashID = Shader.PropertyToID("_EnableFlash");
    private static readonly int _flashColorID = Shader.PropertyToID("_FlashColor");
    private static readonly int _flashAmountID = Shader.PropertyToID("_FlashAmount");
    private static readonly int _alphaAmountID = Shader.PropertyToID("_AlphaAmount");
    private static readonly int _enableOutlineID = Shader.PropertyToID("_EnableOutline");
    private static readonly int _eightDirectionsID = Shader.PropertyToID("_EightDirections");
    private static readonly int _outlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int _outlineAmountID = Shader.PropertyToID("_OutlineAmount");

    private Material[] _materials;
    private SpriteRenderer[] _spriteRenderers;
    private Renderer spriteRenderer;

    private Coroutine spriteFlashCoroutine;
    private WaitForSeconds flashStartDelay;

    private void OnEnable() {
        if (HealthSystem.IsNotNull()) {
            this.HealthSystem.OnEntityDamaged += SetCurrentDamageableFlash;
            this.HealthSystem.OnEntityDeath += SetCurrentDamageableFlash;
            this.HealthSystem.OnInvulnerabilityStart += SetCurrentInvulnerabilityFlash;
            this.HealthSystem.OnInvulnerabilityEnd += DisableIsFlashing;
        }

        if (InteractableSystem.IsNotNull()) {
            this.InteractableSystem.OnInteracted += SetCurrentInteractableFlash;
            this.InteractableSystem.OnInteractionStop += SetCurrentInteractableFlash;
            this.InteractableSystem.OnInteractionState += SetInteractedOutline;
        }
    }

    private void OnDisable() {
        if (HealthSystem.IsNotNull()) {
            this.HealthSystem.OnEntityDamaged -= SetCurrentDamageableFlash;
            this.HealthSystem.OnEntityDeath -= SetCurrentDamageableFlash;
            this.HealthSystem.OnInvulnerabilityStart -= SetCurrentInvulnerabilityFlash;
            this.HealthSystem.OnInvulnerabilityEnd -= DisableIsFlashing;
        }

        if (InteractableSystem.IsNotNull()) {
            this.InteractableSystem.OnInteracted -= SetCurrentInteractableFlash;
            this.InteractableSystem.OnInteractionStop -= SetCurrentInteractableFlash;
            this.InteractableSystem.OnInteractionState -= SetInteractedOutline;
        }
    }

    private void Start() {
        // if (RelatedEntity.IsNull() && this.HasComponentInHierarchy<Entity>()) RelatedEntity = this.GetComponentInHierarchy<Entity>();
        if (spriteRenderer == null) spriteRenderer = this.GetComponentInHierarchy<Renderer>();

        if (Damageable.IsNull()) Damageable = this.GetComponentInHierarchy<IDamageable>();
        if (Interactable.IsNull()) Interactable = this.GetComponentInHierarchy<IInteractable>();

        if (HealthSystem.IsNull() && Damageable.IsNotNull()) {
            HealthSystem = Damageable.HealthSystem;
        }
        if (InteractableSystem.IsNull() && Interactable.IsNotNull()) {
            InteractableSystem = Interactable.InteractableSystem;
        }

        _materials = new Material[1];
        _materials[0] = spriteRenderer.material;

        flashStartDelay = new WaitForSeconds(0.1f);

        SetDefaultValues();
    }

    private void Init() {
        CurrentAmountOfFlashes = 0;
        CurrentColorIndex = 0;
        CurrentAlphaIndex = 0;

        CurrentFlashColor = DefaultFlashColor;
        CurrentAlphaAmount = DefaultAlphaAmount;

        SetFlashAmount(CurrentFlashConfiguration.MaxFlashAmount);
        SetAlphaAmount(CurrentAlphaAmount);
    }

    private void SetDefaultValues() {
        SetFlashColor(DefaultFlashColor);
        SetFlashAmount(0f);
        SetAlphaAmount(DefaultAlphaAmount);
    }

    private void DisableIsFlashing(object sender, EventArgs args) {
        IsFlashing = false;
    }

    public void SetCurrentDamageableFlash(object sender, OnEntityDamagedEventArgs args) {
        if (args.CurrentFlash.IsNotNull()) {
            CurrentFlashConfiguration = args.CurrentFlash;
        }
        else if (HealthSystem.InvulnerabilityFlash.IsNotNull()) {
            CurrentFlashConfiguration = HealthSystem.DamagedFlash;
        }

        StartFlash(CurrentFlashConfiguration);
    }

    public void SetCurrentInvulnerabilityFlash(object sender, OnEntityDamagedEventArgs args) {
        if (args.CurrentFlash.IsNotNull()) {
            CurrentFlashConfiguration = args.CurrentFlash;
        }
        else if (HealthSystem.InvulnerabilityFlash.IsNotNull()) {
            CurrentFlashConfiguration = HealthSystem.InvulnerabilityFlash;
        }

        StartFlash(CurrentFlashConfiguration);
    }

    public void SetCurrentInteractableFlash(object sender, OnEntityInteractedEventArgs args) {
        if (!args.ShouldFlash) return;

        if (args.CurrentFlash.IsNotNull()) {
            CurrentFlashConfiguration = args.CurrentFlash;
        }
        else if (InteractableSystem.InteractedFlash.IsNotNull()) {
            CurrentFlashConfiguration = InteractableSystem.InteractedFlash;
        }

        StartFlash(CurrentFlashConfiguration);
    }

    public void SetInteractedOutline(object sender, OnEntityInteractedEventArgs args) {
        for (int i = 0; i < _materials.Length; i++) {
            _materials[i].SetInt(_enableOutlineID, args.ActiveOutline ? 1 : 0);
        }
    }

    private void StartFlash(SpriteFlashConfiguration configuration) {
        if (IsFlashing) IsFlashing = false;

        configuration.Init();
        Init();

        if (spriteFlashCoroutine != null) {
            StopCoroutine(spriteFlashCoroutine);
            spriteFlashCoroutine = null;
        }

        spriteFlashCoroutine = StartCoroutine(SpriteFlashRoutine(configuration));
    }

    private void StopFlash() {
        IsFlashing = false;

        if (spriteFlashCoroutine != null) {
            StopCoroutine(spriteFlashCoroutine);
            spriteFlashCoroutine = null;
        }

        SetDefaultValues();
    }

    private IEnumerator SpriteFlashRoutine(SpriteFlashConfiguration config) {
        IsFlashing = true;

        yield return flashStartDelay;

        while ((CurrentFlashConfiguration.LoopFlash) || (!CurrentFlashConfiguration.LoopFlash && CurrentAmountOfFlashes < CurrentFlashConfiguration.MaxAmountOfFlashes)) {
            if (CurrentFlashConfiguration.ChangeColor) {
                CurrentFlashColor = CurrentFlashConfiguration.SelectedColorsList.ElementAt(CurrentColorIndex);
                SetFlashColor(CurrentFlashColor);

                CurrentColorIndex++;
                if (CurrentColorIndex >= CurrentFlashConfiguration.TotalColors) CurrentColorIndex = 0;
            }

            if (CurrentFlashConfiguration.ChangeAlpha) {
                CurrentAlphaAmount = CurrentFlashConfiguration.SelectedAlphasList.ElementAt(CurrentAlphaIndex);
                SetAlphaAmount(CurrentAlphaAmount);

                CurrentAlphaIndex++;
                if (CurrentAlphaIndex >= CurrentFlashConfiguration.TotalAlphas) CurrentAlphaIndex = 0;
            }

            CurrentAmountOfFlashes++;

            if ((CurrentFlashConfiguration.LoopFlash && !IsFlashing) || (!CurrentFlashConfiguration.LoopFlash && CurrentAmountOfFlashes >= CurrentFlashConfiguration.MaxAmountOfFlashes)) {
                StopFlash();
                yield break;
            }

            yield return CurrentFlashConfiguration.colorChangeDelay;
        }

        StopFlash();

        yield return null;

        // float currentFlashAmount = 0f;
        // float elapsedTime = 0f;

        // if (LoopFlash) {
        //     SetFlashAmount(1f);

        //     while (IsFlashing) {
        //         SetFlashColor(SecondaryFlashColor);
        //         yield return colorChangeDelay;
        //         SetFlashColor(PrimaryFlashColor);
        //         yield return colorChangeDelay;
        //     }
        // }
        // else {
        //     while (elapsedTime < FlashTime) {
        //         elapsedTime += Time.deltaTime;

        //         currentFlashAmount = Mathf.Lerp(1f, 0f, (elapsedTime / FlashTime));
        //         SetFlashAmount(currentFlashAmount);

        //         yield return null;
        //     }
        // }
    }

    public void SetFlashColor(Color color) {
        for (int i = 0; i < _materials.Length; i++) {
            _materials[i].SetColor(_flashColorID, color);
        }
    }

    public void SetFlashAmount(float amount) {
        for (int i = 0; i < _materials.Length; i++) {
            _materials[i].SetFloat(_flashAmountID, amount);
        }
    }

    public void SetAlphaAmount(float amount) {
        for (int i = 0; i < _materials.Length; i++) {
            _materials[i].SetFloat(_alphaAmountID, amount);
        }
    }
}
