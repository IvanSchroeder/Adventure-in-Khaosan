using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;
using System.Linq;

public class SpriteManager : MonoBehaviour {
    [field: SerializeField] public Entity Entity { get; private set; }
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
    private SpriteRenderer spriteRenderer;

    private Coroutine spriteFlashCoroutine;
    private WaitForSeconds colorChangeDelay;

    private void OnEnable() {
        if (HealthSystem.IsNotNull()) {
            this.HealthSystem.OnEntityDamaged += SetCurrentDamageableFlash;
            this.HealthSystem.OnEntityDeath += SetCurrentDamageableFlash;
            this.HealthSystem.OnInvulnerabilityStart += SetCurrentDamageableFlash;
            this.HealthSystem.OnInvulnerabilityEnd += DisableIsFlashing;
        }

        if (InteractableSystem.IsNotNull()) {
            this.InteractableSystem.OnInteractionState += SetInteractedOutline;
            this.InteractableSystem.OnInteracted += SetCurrentInteractableFlash;
        }
    }

    private void OnDisable() {
        if (HealthSystem.IsNotNull()) {
            this.HealthSystem.OnEntityDamaged -= SetCurrentDamageableFlash;
            this.HealthSystem.OnEntityDeath -= SetCurrentDamageableFlash;
            this.HealthSystem.OnInvulnerabilityStart -= SetCurrentDamageableFlash;
            this.HealthSystem.OnInvulnerabilityEnd -= DisableIsFlashing;
        }

        if (InteractableSystem.IsNotNull()) {
            this.InteractableSystem.OnInteractionState -= SetInteractedOutline;
            this.InteractableSystem.OnInteracted -= SetCurrentInteractableFlash;
        }
    }

    private void Awake() {
        if (Entity.IsNull() && this.HasComponentInHierarchy<Entity>()) Entity = this.GetComponentInHierarchy<Entity>();
        if (Damageable.IsNull()) Damageable = this.GetComponentInHierarchy<IDamageable>();
        if (Interactable.IsNull()) Interactable = this.GetComponentInHierarchy<IInteractable>();

        if (HealthSystem.IsNull() && Damageable.IsNotNull()) {
            HealthSystem = Damageable.HealthSystem;
        }
        if (InteractableSystem.IsNull() && Interactable.IsNotNull()) {
            InteractableSystem = Interactable.InteractableSystem;
        }

        if (spriteRenderer == null) spriteRenderer = this.GetComponentInHierarchy<SpriteRenderer>();
        _materials = new Material[1];
        _materials[0] = spriteRenderer.material;


        // for (int i = 0; i < _spriteRenderers.Length; i++) {
        //     _materials[i] = _spriteRenderers[i].material;
        // }
    }

    private void Start() {
        SetDefaultValues();
    }

    private void Init() {
        CurrentAmountOfFlashes = 0;
        CurrentColorIndex = 0;
        CurrentAlphaIndex = 0;

        if (CurrentFlashConfiguration.SelectedColorsList.Count > 0) CurrentFlashColor = CurrentFlashConfiguration.SelectedColorsList.ElementAt(CurrentColorIndex);
        else CurrentFlashColor = DefaultFlashColor;
        if (CurrentFlashConfiguration.SelectedAlphasList.Count > 0) CurrentAlphaAmount = CurrentFlashConfiguration.SelectedAlphasList.ElementAt(CurrentAlphaIndex);
        else CurrentAlphaAmount = DefaultAlphaAmount;

        SetFlashColor(CurrentFlashColor);
        SetFlashAmount(CurrentFlashConfiguration.MaxFlashAmount);
        SetAlphaAmount(CurrentAlphaAmount);
    }

    private void SetDefaultValues() {
        SetFlashColor(DefaultFlashColor);
        SetFlashAmount(0f);
        SetAlphaAmount(DefaultAlphaAmount);
        // CurrentFlashConfiguration = null;
    }

    private void DisableIsFlashing(object sender, EventArgs args) {
        IsFlashing = false;
    }

    // public void StartDamageFlash(object sender, OnEntityDamagedEventArgs eventArgs) {
    //     CurrentFlashConfiguration = eventArgs.CurrentFlash;
    //     StartFlash(CurrentFlashConfiguration);
    // }

    // public void StartInvulnerabilityFlash(object sender, OnEntityDamagedEventArgs eventArgs) {
    //     CurrentFlashConfiguration = eventArgs.CurrentFlash;
    //     StartFlash(CurrentFlashConfiguration);
    // }

    // public void StartInteractedFlash(object sender, OnEntityInteractedEventArgs eventArgs) {
    //     CurrentFlashConfiguration = eventArgs.CurrentFlash;
    //     StartFlash(CurrentFlashConfiguration);
    // }

    public void SetCurrentDamageableFlash(object sender, OnEntityDamagedEventArgs eventArgs) {
        if (eventArgs.CurrentFlash.IsNull()) return;

        CurrentFlashConfiguration = eventArgs.CurrentFlash;
        InitializeFlashConfiguration(CurrentFlashConfiguration);

        StartFlash(CurrentFlashConfiguration);
    }

    public void SetCurrentInteractableFlash(object sender, OnEntityInteractedEventArgs eventArgs) {
        if (eventArgs.CurrentFlash.IsNull()) return;

        CurrentFlashConfiguration = eventArgs.CurrentFlash;
        InitializeFlashConfiguration(CurrentFlashConfiguration);

        StartFlash(CurrentFlashConfiguration);
    }

    private void InitializeFlashConfiguration(SpriteFlashConfiguration config) {
        config.Init();
        Init();
        colorChangeDelay = new WaitForSeconds(config.SecondsBetweenFlashes);
    }

    public void SetInteractedOutline(object sender, bool enable) {
        for (int i = 0; i < _materials.Length; i++) {
            _materials[i].SetInt(_enableOutlineID, enable ? 1 : 0);
        }
    }

    private void StartFlash(SpriteFlashConfiguration configuration) {
        // if (IsFlashing) IsFlashing = false;

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

            yield return colorChangeDelay;
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

    private void SetFlashColor(Color color) {
        for (int i = 0; i < _materials.Length; i++) {
            // if (_materials[i].HasColor(_flashColorID))
            _materials[i].SetColor(_flashColorID, color);
        }
    }

    private void SetFlashAmount(float amount) {
        for (int i = 0; i < _materials.Length; i++) {
            // if (_materials[i].HasFloat(_flashAmountID))
            _materials[i].SetFloat(_flashAmountID, amount);
        }
    }

    private void SetAlphaAmount(float amount) {
        for (int i = 0; i < _materials.Length; i++) {
            // if (_materials[i].HasFloat(_alphaAmountID))
            _materials[i].SetFloat(_alphaAmountID, amount);
        }
    }
}
