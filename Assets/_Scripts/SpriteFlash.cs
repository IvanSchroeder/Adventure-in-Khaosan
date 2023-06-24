using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System.Linq;

public class SpriteFlash : MonoBehaviour {
    private Player player;
    private HealthSystem healthSystem;
    // private Interactor interactor;
    private IDamageable damageableBehavior;
    private IInteractable interactableBehavior;
    // [field: SerializeField] public SpriteFlashConfiguration DamageFlash { get; private set; }
    // [field: SerializeField] public SpriteFlashConfiguration InvulnerabilityFlash { get; private set; }
    // [field: SerializeField] public SpriteFlashConfiguration InteractedFlash { get; private set; }
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

    private static readonly int _flashColorID = Shader.PropertyToID("_FlashColor");
    private static readonly int _flashAmountID = Shader.PropertyToID("_FlashAmount");
    private static readonly int _alphaAmountID = Shader.PropertyToID("_AlphaAmount");

    private Material[] _materials;
    private SpriteRenderer[] _spriteRenderers;

    private Coroutine spriteFlashCoroutine;
    private WaitForSeconds colorChangeDelay;

    private void OnEnable() {
        //player.HealthSystem.OnDamaged += StartDamageFlash;
        // player.OnKnockbackEnd += StopFlash;
        // player.HealthSystem.OnInvulnerabilityStart += StartInvulnerabilityFlash;
        // player.HealthSystem.OnInvulnerabilityEnd += StopFlash;
        
        healthSystem.OnDamaged += StartFlash;
        //interactableBehavior.OnInteracted += StartFlash;
    }

    private void OnDisable() {
        //player.HealthSystem.OnDamaged -= StartDamageFlash;
        // player.OnKnockbackEnd -= StopFlash;
        // player.HealthSystem.OnInvulnerabilityStart -= StartInvulnerabilityFlash;
        // player.HealthSystem.OnInvulnerabilityEnd -= StopFlash;

        healthSystem.OnDamaged -= StartFlash;
        healthSystem.OnInvulnerabilityStart -= StartFlash;
        healthSystem.OnInvulnerabilityEnd -= StopFlash;
        //interactableBehavior.OnInteracted -= StartFlash;
    }

    private void OnValidate() {
        // Init();
    }

    private void Awake() {
        if (player == null) player = this.GetComponentInHierarchy<Player>();
        if (damageableBehavior == null) damageableBehavior = this.GetComponentInHierarchy<IDamageable>();
        if (interactableBehavior == null) interactableBehavior = this.GetComponentInHierarchy<IInteractable>();

        _spriteRenderers = player.GetComponentsInChildren<SpriteRenderer>();
        _materials = new Material[1];

        for (int i = 0; i < _spriteRenderers.Length; i++) {
            _materials[i] = _spriteRenderers[i].material;
        }
    }

    private void Start() {
        SetDefaultValues();
    }

    private void SetSpriteFlashConfiguration(SpriteFlashConfiguration config) {
        CurrentFlashConfiguration = config;
        CurrentFlashConfiguration.Init();
        Init();
        colorChangeDelay = new WaitForSeconds(CurrentFlashConfiguration.SecondsBetweenFlashes);
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

    // public void StartDamageFlash() {
    //     StartFlash(DamageFlash);
    // }

    // public void StartInvulnerabilityFlash() {
    //     StartFlash(InvulnerabilityFlash);
    // }

    // public void StartInteractedFlash() {
    //     StartFlash(InteractedFlash);
    // }

    private void StartFlash(DamageInfo damageInfo) {
        if (spriteFlashCoroutine != null) {
            IsFlashing = false;
            StopCoroutine(spriteFlashCoroutine);
            spriteFlashCoroutine = null;
        }

        IsFlashing = true;

        spriteFlashCoroutine = StartCoroutine(SpriteFlashRoutine(damageInfo.CurrentFlash));
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
                StopFlash();
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
        CurrentFlashConfiguration = null;
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
