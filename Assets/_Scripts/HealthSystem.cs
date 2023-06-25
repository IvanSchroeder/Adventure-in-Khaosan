using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public class HealthSystem : MonoBehaviour, IDamageable {
    public Entity entity;
    public EntityData entityData;
    public LayerMask damagedBy;
    [field: SerializeField] public BoxCollider2D HitboxTrigger { get; set; }

    [field: SerializeField] public HealthType HealthType { get; set; }
    [field: SerializeField] public DamageInfo LastDamageInfo { get; set; }
    [field: SerializeField] public Material SpriteFlashMaterial { get; set; }
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

    public Action<DamageInfo> OnDamaged;
    public Action<DamageInfo> OnInvulnerabilityStart;
    public Action<DamageInfo> OnEntityDead;
    public Action OnLivesDepleted;

    private Coroutine invulnerabilityCoroutine;
    private WaitForSeconds invulnerabilityDelay;

    // private void OnEnable() {
    //     // player.OnKnockbackEnd += SetInvulnerability;
    //     Player.OnPlayerDeathEnd += Invulnerable;
    // }

    // private void OnDisable() {
    //     // player.OnKnockbackEnd -= SetInvulnerability;
    //     Player.OnPlayerDeathEnd -= Invulnerable;
    // }

    private void Awake() {
        if (entity == null) entity = this.GetComponentInHierarchy<Entity>();
        if (HitboxTrigger == null) HitboxTrigger = GetComponent<BoxCollider2D>();
    }

    private void Start() {
        if (entityData == null) entityData = entity.entityData;

        MaxLives = entityData.maxLives;
        CurrentLives = MaxLives;
        if (CurrentLives > 1) CanRespawn = true;
        entityData.currentLives = CurrentLives;

        InvulnerabilitySeconds = entityData.invulnerabilitySeconds;
        invulnerabilityDelay = new WaitForSeconds(InvulnerabilitySeconds);

        InitializeHealth();
    }

    public void OnTriggerEnter2D(Collider2D collision) {
        DamageDealer damageDealerSource = collision.GetComponentInHierarchy<DamageDealer>();
        Debug.Log($"Detected hazard {damageDealerSource.transform.name}");

        if (damageDealerSource == null) return;

        // LastDamageInfo = new DamageInfo(damageDealerSource, damageDealerSource.damageAmount, collision.ClosestPoint(this.transform.position), DamageFlash, DamageFlash, InvulnerabilityFlash);
        LastDamageInfo = new DamageInfo(damageDealerSource, damageDealerSource.damageAmount, collision.ClosestPoint(this.transform.position), entityData.damageFlash);
        Damage(LastDamageInfo);
    }

    public void InitializeHealth() {
        IsDead = false;
        IsInvulnerable = false;
        IsRespawneable = true;

        HealthType = entityData.healthType;

        switch (HealthType) {
            case HealthType.Numerical:
                MaxHealth = entityData.maxHealth;
                CurrentHealth = MaxHealth;
                entityData.currentHealth = CurrentHealth;
            break;
            case HealthType.Hearts:
                MaxHearts = entityData.maxHearts;
                CurrentHearts = MaxHearts;
                entityData.currentHearts = CurrentHearts;
                // spawn hearts in UI slots, maybe with a list/array and an event?
            break;
        }

        // LastDamageInfo = new DamageInfo(null, 0f, Vector2.zero, DamageFlash, DamageFlash, InvulnerabilityFlash);
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

                entityData.currentHealth = CurrentHealth;
            break;
            case HealthType.Hearts:
                CurrentHearts -= 1;

                if (CurrentHearts <= 0) {
                    CurrentHearts = 0;
                    IsDead = true;
                }

                entityData.currentHearts = CurrentHearts;
                // spawn hearts in UI slots, maybe with a list/array and an event?
            break;
        }

        OnDamaged?.Invoke(damageInfo);

        if(IsDead) {
            Death();
            OnEntityDead?.Invoke(damageInfo);
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
        
        entityData.currentLives = CurrentLives;
    }

    public void Invulnerable() {
        // DamageInfo damageInfo = new DamageInfo(default, 0f, default, InvulnerabilityFlash, DamageFlash, InvulnerabilityFlash);
        DamageInfo damageInfo = new DamageInfo(default, 0f, default, entityData.invulnerabilityFlash);
        LastDamageInfo = damageInfo;
        IsInvulnerable = true;
        HitboxTrigger.enabled = false;
    }

    public void SetInvulnerability(DamageInfo damageInfo) {
        invulnerabilityCoroutine = StartCoroutine(InvulnerabilityFramesRoutine(damageInfo));
    }

    private IEnumerator InvulnerabilityFramesRoutine(DamageInfo damageInfo) {
        // damageInfo = new DamageInfo(default, 0f, default, InvulnerabilityFlash, DamageFlash, InvulnerabilityFlash);
        damageInfo = new DamageInfo(default, 0f, default, entityData.invulnerabilityFlash);
        LastDamageInfo = damageInfo;
        Invulnerable();
        OnInvulnerabilityStart?.Invoke(LastDamageInfo);
        yield return invulnerabilityDelay;
        IsInvulnerable = false;
        HitboxTrigger.enabled = true;
        yield return null;
    }
}
