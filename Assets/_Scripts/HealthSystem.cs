using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public class HealthSystem : MonoBehaviour, IDamageable {
    public Player player;
    public PlayerData playerData;
    public LayerMask damagedBy;

    [field: SerializeField] public BoxCollider2D HitboxCollider { get; private set; }
    [field: SerializeField] public HealthType HealthType { get; set; }
    [field: SerializeField] public float MaxHealth { get; set; }
    [field: SerializeField] public float CurrentHealth { get; set; }
    [field: SerializeField] public int MaxHearts { get; set; }
    [field: SerializeField] public int CurrentHearts { get; set; }
    [field: SerializeField] public bool IsDead { get; set; }
    [field: SerializeField] public bool IsInvulnerable { get; set; }
    [field: SerializeField] public float InvulnerabilitySeconds { get; set; }

    public event Action<Vector2, DamageDealer> OnDamaged;
    public event Action OnInvulnerabilityStart;
    public event Action OnInvulnerabilityEnd;

    private Coroutine invulnerabilityCoroutine;
    private WaitForSeconds invulnerabilityDelay;

    private void OnEnable() {
        player.OnKnockbackEnd += SetInvulnerability;
    }

    private void OnDisable() {
        player.OnKnockbackEnd -= SetInvulnerability;
    }

    private void Awake() {
        if (player == null) player = this.GetComponentInHierarchy<Player>();
        if (HitboxCollider == null) HitboxCollider = GetComponent<BoxCollider2D>();
    }

    private void Start() {
        IsDead = false;
        IsInvulnerable = false;
        InvulnerabilitySeconds = playerData.invulnerabilitySeconds;
        invulnerabilityDelay = new WaitForSeconds(InvulnerabilitySeconds);

        InitializeHealth();
    }

    public void InitializeHealth() {
        HealthType = playerData.healthType;

        switch (HealthType) {
            case HealthType.Numerical:
                MaxHealth = playerData.maxHealth;
                CurrentHealth = MaxHealth;
                playerData.currentHealth = CurrentHealth;
            break;
            case HealthType.Hearts:
                MaxHearts = playerData.maxHearts;
                CurrentHearts = MaxHearts;
                playerData.currentHearts = CurrentHearts;
                // spawn hearts in UI slots, maybe with a list/array and an event?
            break;
        }
    }

    public bool IsDamagedBy(int layer) {
        if (damagedBy.HasLayer(layer)) {
            return true;
        }

        return false;
    }

    public void Damage(float amount, Vector2 point, DamageDealer source) {
        IsInvulnerable = true;

        switch (HealthType) {
            case HealthType.Numerical:
                CurrentHealth -= amount;

                if (CurrentHealth <= 0f) {
                    CurrentHealth = 0f;
                    IsDead = true;
                }

                playerData.currentHealth = CurrentHealth;
            break;
            case HealthType.Hearts:
                CurrentHearts -= 1;

                if (CurrentHearts <= 0) {
                    CurrentHearts = 0;
                    IsDead = true;
                }

                playerData.currentHearts = CurrentHearts;
                // spawn hearts in UI slots, maybe with a list/array and an event?
            break;
        }

        if(IsDead) Debug.Log($"Player died");
        else Debug.Log($"Player damaged by {amount}");

        OnDamaged?.Invoke(point, source);
    }

    public void SetInvulnerability() {
        invulnerabilityCoroutine = StartCoroutine(InvulnerabilityFramesRoutine());
    }

    private IEnumerator InvulnerabilityFramesRoutine() {
        IsInvulnerable = true;
        HitboxCollider.enabled = false;
        OnInvulnerabilityStart?.Invoke();
        // set sprite flash or flickering
        yield return invulnerabilityDelay;
        IsInvulnerable = false;
        HitboxCollider.enabled = true;
        OnInvulnerabilityEnd?.Invoke();
        yield return null;
    }
}
