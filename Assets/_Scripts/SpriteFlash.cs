using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class SpriteFlash : MonoBehaviour {
    private Player player;

    [field: SerializeField, ColorUsage(true, true)] public Color PrimaryFlashColor { get; private set; } = Color.black;
    [field: SerializeField, ColorUsage(true, true)] public Color SecondaryFlashColor { get; private set; } = Color.white;
    [ColorUsage(true, true)] private Color currentFlashColor;
    [field: SerializeField] public float ColorChangeSeconds { get; private set; } = 0.1f;
    [field: SerializeField] public float FlashTime { get; private set; } = 0.25f;
    [field: SerializeField] public bool LoopFlash { get; private set; } = false;
    [field: SerializeField] public bool IsFlashing { get; private set; } = false;
    [field: SerializeField] public AnimationCurve FlashSpeedCurve { get; private set; }
    [field: SerializeField] public int CurrentAmountOfFlashes { get; private set; }
    [field: SerializeField] public int MaxAmountOfFlashes { get; private set; } = 2;

    private static readonly int _flashColorID = Shader.PropertyToID("_FlashColor");
    private static readonly int _flashAmountID = Shader.PropertyToID("_FlashAmount");

    private Material[] _materials;
    private SpriteRenderer[] _spriteRenderers;

    private Coroutine spriteFlashCoroutine;
    private WaitForSeconds colorChangeDelay;

    private void OnEnable() {
        player.HealthSystem.OnDamaged += StartSpriteFlash;
        player.OnKnockbackEnd += StopSpriteFlash;
    }

    private void OnDisable() {
        player.HealthSystem.OnDamaged -= StartSpriteFlash;
        player.OnKnockbackEnd -= StopSpriteFlash;
    }

    private void Awake() {
        if (player == null) player = this.GetComponentInHierarchy<Player>();

        _spriteRenderers = player.GetComponentsInChildren<SpriteRenderer>();

        Init();
    }

    private void Init() {
        _materials = new Material[1];

        for (int i = 0; i < _spriteRenderers.Length; i++) {
            _materials[i] = _spriteRenderers[i].material;
        }

        colorChangeDelay = new WaitForSeconds(ColorChangeSeconds);
        CurrentAmountOfFlashes = 0;
    }

    public void StartSpriteFlash(Vector2 point, DamageDealer source) {
        CurrentAmountOfFlashes = 0;
        IsFlashing = true;
        spriteFlashCoroutine = StartCoroutine(SpriteFlashRoutine());
    }

    public void StopSpriteFlash() {
        IsFlashing = false;
        SetFlashAmount(0f);

        if (spriteFlashCoroutine != null) {
            StopCoroutine(spriteFlashCoroutine);
            spriteFlashCoroutine = null;
        }
        
    }

    private IEnumerator SpriteFlashRoutine() {
        SetFlashAmount(1f);
        currentFlashColor = SecondaryFlashColor;
        SetFlashColor(currentFlashColor);

        while ((LoopFlash && IsFlashing) || CurrentAmountOfFlashes < MaxAmountOfFlashes) {
            if (currentFlashColor == PrimaryFlashColor) {
                currentFlashColor = SecondaryFlashColor;
            }
            else if (currentFlashColor == SecondaryFlashColor) {
                currentFlashColor = PrimaryFlashColor;
            }

            SetFlashColor(currentFlashColor);

            CurrentAmountOfFlashes++;
            yield return colorChangeDelay;
            
            //elapsedTime += Time.deltaTime;

            // currentFlashAmount = FlashSpeedCurve.Evaluate(elapsedTime / FlashTime);
            // SetFlashAmount(currentFlashAmount);
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
}
