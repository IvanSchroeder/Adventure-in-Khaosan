using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System.Linq;

public class SpriteManager : MonoBehaviour {
    [field: SerializeField] public Entity Entity { get; private set; }
    [field: SerializeField] public HealthSystem HealthSystem { get; private set; }
    // [field: SerializeField] public InteractorSystem InteractorSystem { get; private set; }
    // private Interactor interactor;
    private IDamageable damageableBehavior;
    private IInteractable interactableBehavior;
    [field: SerializeField] public SpriteFlashConfiguration CurrentFlashConfiguration { get; private set; }
    [field: SerializeField] public SpriteFlashConfiguration DamagedFlash { get; private set; }
    [field: SerializeField] public SpriteFlashConfiguration InvulnerableFlash { get; private set; }
    [field: SerializeField] public SpriteFlashConfiguration InteractedFlash { get; private set; }

    [field: SerializeField, ColorUsage(true, true)] public Color DefaultFlashColor { get; private set; } = Color.white;
    [field: SerializeField, ColorUsage(true, true)] public Color CurrentFlashColor { get; private set; } = Color.white;
    [field: SerializeField] public float CurrentAlphaAmount { get; private set; } = 1;
    [field: SerializeField] public float DefaultAlphaAmount { get; private set; } = 1;
    [field: SerializeField] public bool IsFlashing { get; private set; } = false;
    [field: SerializeField] public int CurrentColorIndex { get; private set; }
    [field: SerializeField] public int CurrentAmountIndex { get; private set; }
    [field: SerializeField] public int CurrentAlphaIndex { get; private set; }
    [field: SerializeField] public int CurrentAmountOfFlashes { get; private set; }

    private static readonly int _flashColorID = Shader.PropertyToID("_FlashColor");
    private static readonly int _flashAmountID = Shader.PropertyToID("_FlashAmount");
    private static readonly int _alphaAmountID = Shader.PropertyToID("_AlphaAmount");

    private Material[] _materials;
    // private SpriteRenderer[] _spriteRenderers;
    private SpriteRenderer spriteRenderer;

    private Coroutine spriteFlashCoroutine;
    private WaitForSeconds colorChangeDelay;

    private void OnEnable() {
        // this.HealthSystem.OnDamaged += StartFlash;
        // this.HealthSystem.OnEntityDead += StartFlash;
        // this.HealthSystem.OnInvulnerabilityStart += StartFlash;

        this.HealthSystem.OnDamaged += StartDamageFlash;
        this.HealthSystem.OnEntityDead += StartDamageFlash;
        this.HealthSystem.OnInvulnerabilityStart += StartInvulnerabilityFlash;

        // this.InteractorSystem.OnInteracted += StartInteractedFlash;
    }

    private void OnDisable() {
        // this.HealthSystem.OnDamaged -= StartFlash;
        // this.HealthSystem.OnEntityDead -= StartFlash;
        // this.HealthSystem.OnInvulnerabilityStart -= StartFlash;

        this.HealthSystem.OnDamaged -= StartDamageFlash;
        this.HealthSystem.OnEntityDead -= StartDamageFlash;
        this.HealthSystem.OnInvulnerabilityStart -= StartInvulnerabilityFlash;

        // this.InteractorSystem.OnInteracted -= StartInteractedFlash;
    }

    private void Awake() {
        if (Entity == null) Entity = this.GetComponentInHierarchy<Entity>();
        if (HealthSystem == null) HealthSystem = this.GetComponentInHierarchy<HealthSystem>();
        if (damageableBehavior == null) damageableBehavior = this.GetComponentInHierarchy<IDamageable>();
        if (interactableBehavior == null) interactableBehavior = this.GetComponentInHierarchy<IInteractable>();

        spriteRenderer = this.GetComponentInHierarchy<SpriteRenderer>();
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

    private void SetSpriteFlashConfiguration(SpriteFlashConfiguration config) {
        if (config == null) return;
        CurrentFlashConfiguration = config;
        CurrentFlashConfiguration.Init();
        Init();
        colorChangeDelay = new WaitForSeconds(CurrentFlashConfiguration.SecondsBetweenFlashes);
    }

    public void StartDamageFlash(DamageInfo damageInfo = default) {
        StartFlash(HealthSystem.entityData.damageFlash);
    }

    public void StartInvulnerabilityFlash(DamageInfo damageInfo = default) {
        StartFlash(HealthSystem.entityData.damageFlash);
    }

    public void StartInteractedFlash(DamageInfo damageInfo = default) {
        StartFlash(InteractedFlash);
    }

    private void StartFlash(SpriteFlashConfiguration configuration) {
        if (spriteFlashCoroutine != null) {
            IsFlashing = false;
            StopCoroutine(spriteFlashCoroutine);
            spriteFlashCoroutine = null;
        }

        IsFlashing = true;

        spriteFlashCoroutine = StartCoroutine(SpriteFlashRoutine(configuration));
    }

    private void StopFlash(DamageInfo damageInfo) {
        IsFlashing = false;

        if (spriteFlashCoroutine != null) {
            StopCoroutine(spriteFlashCoroutine);
            spriteFlashCoroutine = null;
        }

        SetDefaultValues();
    }

    private IEnumerator SpriteFlashRoutine(SpriteFlashConfiguration config) {
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

            yield return colorChangeDelay;

            if ((CurrentFlashConfiguration.LoopFlash && !IsFlashing) || (!CurrentFlashConfiguration.LoopFlash && CurrentAmountOfFlashes >= CurrentFlashConfiguration.MaxAmountOfFlashes)) {
                StopFlash(default);
                yield break;
            }
        }

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

    private void SetDefaultValues() {
        SetFlashColor(DefaultFlashColor);
        SetFlashAmount(0f);
        SetAlphaAmount(DefaultAlphaAmount);
        // CurrentFlashConfiguration = null;
    }

    private void SetFlashColor(Color color) {
        for (int i = 0; i < _materials.Length; i++) {
            _materials[i].SetColor(_flashColorID, color);
        }
    }

    private void SetFlashAmount(float amount) {
        for (int i = 0; i < _materials.Length; i++) {
            _materials[i].SetFloat(_flashAmountID, amount);
        }
    }

    private void SetAlphaAmount(float amount) {
        for (int i = 0; i < _materials.Length; i++) {
            _materials[i].SetFloat(_alphaAmountID, amount);
        }
    }
}
