using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;
using System.Linq;

public class SpriteManager : MonoBehaviour {
    [field: SerializeField] public Entity Entity { get; private set; }
    [field: SerializeField] public HealthSystem HealthSystem { get; private set; }
    [field: SerializeField] public IDamageable DamageableBehavior { get; private set; }
    [field: SerializeField] public IInteractable InteractableBehavior { get; private set; }
    // [field: SerializeField] public InteractorSystem InteractorSystem { get; private set; }
    [field: SerializeField] public SpriteFlashConfiguration CurrentFlashConfiguration { get; private set; }
    public SpriteFlashConfiguration DamagedFlash { get; private set; }
    public SpriteFlashConfiguration InvulnerableFlash { get; private set; }
    public SpriteFlashConfiguration InteractedFlash { get; private set; }

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
    private static readonly int _outlineAmountID = Shader.PropertyToID("_OutlineAmount");

    public Material[] _materials;
    // private SpriteRenderer[] _spriteRenderers;
    public SpriteRenderer spriteRenderer;

    private Coroutine spriteFlashCoroutine;
    private WaitForSeconds colorChangeDelay;

    private void OnEnable() {
        if (HealthSystem.IsNotNull()) {
            this.HealthSystem.OnDamaged += StartDamageFlash;
            this.HealthSystem.OnEntityDead += StartDamageFlash;
            this.HealthSystem.OnInvulnerabilityStart += StartInvulnerabilityFlash;
            this.HealthSystem.OnInvulnerabilityEnd += DisableIsFlashing;
        }

        // this.InteractorSystem.OnInteracted += StartInteractedFlash;
    }

    private void OnDisable() {
        if (HealthSystem.IsNotNull()) {
            this.HealthSystem.OnDamaged -= StartDamageFlash;
            this.HealthSystem.OnEntityDead -= StartDamageFlash;
            this.HealthSystem.OnInvulnerabilityStart -= StartInvulnerabilityFlash;
            this.HealthSystem.OnInvulnerabilityEnd -= DisableIsFlashing;
        }

        // this.InteractorSystem.OnInteracted -= StartInteractedFlash;
    }

    private void Awake() {
        if (Entity == null && this.HasComponentInHierarchy<Entity>()) Entity = this.GetComponentInHierarchy<Entity>();
        if (HealthSystem == null) HealthSystem = this.GetComponentInHierarchy<HealthSystem>();
        if (DamageableBehavior == null) DamageableBehavior = this.GetComponentInHierarchy<IDamageable>();
        if (InteractableBehavior == null) InteractableBehavior = this.GetComponentInHierarchy<IInteractable>();

        if (spriteRenderer == null) spriteRenderer = this.GetComponentInHierarchy<SpriteRenderer>();
        _materials = new Material[1];
        _materials[0] = spriteRenderer.material;


        // for (int i = 0; i < _spriteRenderers.Length; i++) {
        //     _materials[i] = _spriteRenderers[i].material;
        // }
    }

    private void Start() {
        if (HealthSystem != null) {
            if (HealthSystem.DamagedFlash != null) DamagedFlash = HealthSystem.DamagedFlash;
            if (HealthSystem.InvulnerableFlash != null) InvulnerableFlash = HealthSystem.InvulnerableFlash;
        }

        // if (InteractorSystem != null) {
        //     if (InteractorSystem.InteractedFlash != null) InteractedFlash = InteractorSystem.InteractedFlash;
        // }
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

    private void SetSpriteFlashConfiguration(SpriteFlashConfiguration config) {
        if (config == null) return;
        CurrentFlashConfiguration = config;
        CurrentFlashConfiguration.Init();
        Init();
        colorChangeDelay = new WaitForSeconds(CurrentFlashConfiguration.SecondsBetweenFlashes);
    }

    public void StartDamageFlash(object sender, OnEntityDamagedEventArgs eventArgs) {
        // StartFlash(HealthSystem.entityData.damageFlash);
        // Material mat = Instantiate(eventArgs.CurrentMaterial);
        // spriteRenderer.material = mat;
        StartFlash(DamagedFlash);
    }

    public void StartInvulnerabilityFlash(object sender, OnEntityDamagedEventArgs eventArgs) {
        // StartFlash(HealthSystem.entityData.invulnerabilityFlash);
        // Material mat = Instantiate(eventArgs.CurrentMaterial);
        // spriteRenderer.material = mat;
        StartFlash(InvulnerableFlash);
    }

    public void StartInteractedFlash(object sender, OnEntityDamagedEventArgs eventArgs) {
        StartFlash(InteractedFlash);
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
        SetSpriteFlashConfiguration(config);

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
