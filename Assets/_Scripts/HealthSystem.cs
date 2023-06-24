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
    [field: SerializeField] public DamageInfo LastDamageInfo { get; set; }
    [field: SerializeField] public SpriteFlashConfiguration DamageFlash { get; set; }
    [field: SerializeField] public SpriteFlashConfiguration InvulnerabilityFlash { get; set; }
    [field: SerializeField] public bool IsRespawneable { get; set; }
    [field: SerializeField] public bool CanRespawn { get; set; }
    [field: SerializeField] public int CurrentLives { get; set; }
    [field: SerializeField] public int MaxLives { get; set; }
    [field: SerializeField] public float MaxHealth { get; set; }
    [field: SerializeField] public float CurrentHealth { get; set; }
    [field: SerializeField] public int MaxHearts { get; set; }
    [field: SerializeField] public int CurrentHearts { get; set; }
    [field: SerializeField] public bool IsDead { get; set; }
    [field: SerializeField] public bool IsInvulnerable { get; set; }
    [field: SerializeField] public float InvulnerabilitySeconds { get; set; }

    public event Action<DamageInfo> OnDamaged;
    public event Action<DamageInfo> OnInvulnerabilityStart;
    public event Action<DamageInfo> OnInvulnerabilityEnd;
    public static event Action<DamageInfo> OnPlayerDeath;
    public static event Action OnLivesDepleted;

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
        MaxLives = playerData.maxLives;
        CurrentLives = MaxLives;
        if (CurrentLives > 1) CanRespawn = true;
        playerData.currentLives = CurrentLives;

        InvulnerabilitySeconds = playerData.invulnerabilitySeconds;
        invulnerabilityDelay = new WaitForSeconds(InvulnerabilitySeconds);

        InitializeHealth();
    }

    public void OnTriggerEnter2D(Collider2D collision) {
        DamageDealer damageDealerSource = collision.GetComponentInHierarchy<DamageDealer>();
        Debug.Log($"Detected hazard {damageDealerSource.transform.name}");

        if (damageDealerSource == null) return;

        LastDamageInfo = new DamageInfo(damageDealerSource, damageDealerSource.damageAmount, collision.ClosestPoint(this.transform.position), DamageFlash, DamageFlash, InvulnerabilityFlash);
        Damage(LastDamageInfo);
    }

    public void InitializeHealth() {
        IsDead = false;
        IsInvulnerable = false;
        IsRespawneable = true;

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

        LastDamageInfo = new DamageInfo(null, 0f, Vector2.zero, DamageFlash, DamageFlash, InvulnerabilityFlash);
    }

    public bool IsDamagedBy(int layer) {
        if (damagedBy.HasLayer(layer)) {
            return true;
        }

        return false;
    }

    public void Damage(DamageInfo damageInfo) {
        if (IsInvulnerable || IsDead) return;
        
        if (!IsDamagedBy(damageInfo.DamageDealerSource.damageDealerLayer)) return;

        IsInvulnerable = true;

        switch (HealthType) {
            case HealthType.Numerical:
                CurrentHealth -= damageInfo.DamageAmount;

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

        OnDamaged?.Invoke(damageInfo);

        if(IsDead) {
            Death();
            OnPlayerDeath?.Invoke(damageInfo);
        }
        else {
            OnDamaged?.Invoke(damageInfo);
        }
    }

    public void Death() {
        CurrentLives--;

        if (CurrentLives <= 0) {
            CurrentLives = 0;
            CanRespawn = false;
            OnLivesDepleted?.Invoke();
        }
        
        playerData.currentLives = CurrentLives;
    }

    public void SetInvulnerability(DamageInfo damageInfo) {
        invulnerabilityCoroutine = StartCoroutine(InvulnerabilityFramesRoutine(damageInfo));
    }

    private IEnumerator InvulnerabilityFramesRoutine(DamageInfo damageInfo) {
        IsInvulnerable = true;
        HitboxCollider.enabled = false;
        damageInfo = new DamageInfo(null, 0f, default, InvulnerabilityFlash, DamageFlash, InvulnerabilityFlash);
        OnInvulnerabilityStart?.Invoke(damageInfo);
        yield return invulnerabilityDelay;
        IsInvulnerable = false;
        HitboxCollider.enabled = true;
        OnInvulnerabilityEnd?.Invoke(damageInfo);
        yield return null;
    }
}
