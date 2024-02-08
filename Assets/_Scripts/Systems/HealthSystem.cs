using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public class HealthSystem : MonoBehaviour {
    [field: SerializeField] public Entity Entity { get; private set; }
    [field: SerializeField] public EntityData EntityData { get; private set; }
    [field: SerializeField] public BoxCollider2D HitboxTrigger { get; set; }
    [field: SerializeField] public SpriteManager SpriteManager { get; set; }

    [field: SerializeField] public LayerMask DamagedBy { get; set; }
    [field: SerializeField] public HealthType HealthType { get; set; }
    [field: SerializeField] public SpriteFlashConfiguration DamagedFlash { get; set; }
    [field: SerializeField] public SpriteFlashConfiguration InvulnerabilityFlash { get; set; }
    [field: SerializeField] public bool IsRespawneable { get; set; }
    [field: SerializeField] public bool CanRespawn { get; set; }
    [field: SerializeField] public bool HasInfiniteLives { get; set; }
    [field: SerializeField] public int CurrentLives { get; set; }
    [field: SerializeField] public int MaxLives { get; set; }
    [field: SerializeField] public float MaxHealth { get; set; }
    [field: SerializeField] public float CurrentHealth { get; set; }
    [field: SerializeField] public int MaxHearts { get; set; }
    [field: SerializeField] public int CurrentHearts { get; set; }
    [field: SerializeField] public bool IsDead { get; set; }
    [field: SerializeField] public bool HasInvulnerabilityFrames { get; set; }
    [field: SerializeField] public bool IsInvulnerable { get; set; }

    public EventHandler<OnEntityDamagedEventArgs> OnEntityDamaged;
    public EventHandler<OnEntityDamagedEventArgs> OnEntityDeath;
    public EventHandler<OnEntityDamagedEventArgs> OnInvulnerabilityStart;
    public EventHandler OnInvulnerabilityEnd;
    public EventHandler<OnEntityDamagedEventArgs> OnLivesDepleted;

    private Coroutine invulnerabilityCoroutine;
    private WaitForSeconds invulnerabilityDelay;
    private WaitForSeconds invulnerabilityStartDelay;

    private void Awake() {
        if (Entity == null) Entity = this.GetComponentInHierarchy<Entity>();
        if (HitboxTrigger == null) HitboxTrigger = GetComponent<BoxCollider2D>();
        if (SpriteManager == null) SpriteManager = this.GetComponentInHierarchy<SpriteManager>();
    }

    private void Start() {
        Init();
    }

    public void Init() {
        if (EntityData == null) EntityData = Entity.entityData;

        DamagedBy = EntityData.damagedBy;

        MaxLives = EntityData.maxLives;
        CurrentLives = MaxLives;
        if (CurrentLives > 1) CanRespawn = true;
        EntityData.currentLives = CurrentLives;

        invulnerabilityDelay = new WaitForSeconds(EntityData.invulnerabilitySeconds);
        invulnerabilityStartDelay = new WaitForSeconds(0.1f);

        InitializeHealth();
    }

    public void InitializeHealth() {
        IsDead = false;
        IsInvulnerable = false;
        IsRespawneable = true;

        HealthType = EntityData.healthType;

        switch (HealthType) {
            case HealthType.Numerical:
                MaxHealth = EntityData.maxHealth;
                CurrentHealth = MaxHealth;
                EntityData.currentHealth = CurrentHealth;
            break;
            case HealthType.Hearts:
                MaxHearts = EntityData.maxHearts;
                CurrentHearts = EntityData.startingHearts;
                EntityData.currentHearts = CurrentHearts;
                // spawn hearts in UI slots, maybe with a list/array and an event?
            break;
        }
    }

    public bool IsDamagedBy(int layer) {
        if (DamagedBy.HasLayer(layer)) {
            return true;
        }

        return false;
    }

    public void AddHealth(float amount) {

    }

    public void ReduceHealth(object sender, OnEntityDamagedEventArgs entityDamagedEventArgs) {
        if (IsInvulnerable || IsDead || !IsDamagedBy(entityDamagedEventArgs.DamageDealerSource.DamageDealerLayer)) return;

        switch (HealthType) {
            case HealthType.Numerical:
                if (entityDamagedEventArgs.DamageAmount == 0f) return;

                CurrentHealth -= entityDamagedEventArgs.DamageAmount;

                if (CurrentHealth <= 0f) {
                    CurrentHealth = 0f;
                    IsDead = true;
                }

                EntityData.currentHealth = CurrentHealth;
            break;
            case HealthType.Hearts:
                if (entityDamagedEventArgs.DamageInHearts.Value == 0) return;

                CurrentHearts -= entityDamagedEventArgs.DamageInHearts.Value;

                if (CurrentHearts <= 0) {
                    CurrentHearts = 0;
                    IsDead = true;
                }

                EntityData.currentHearts = CurrentHearts;
                // spawn hearts in UI slots, maybe with a list/array and an event?
            break;
        }

        IsInvulnerable = true;
        HitboxTrigger.enabled = false;

        if (IsDead) {
            ReduceLives(entityDamagedEventArgs);
            OnEntityDeath?.Invoke(sender, entityDamagedEventArgs);
        }
        else {
            OnEntityDamaged?.Invoke(sender, entityDamagedEventArgs);
        }
    }

    public void ReduceLives(OnEntityDamagedEventArgs entityArgs) {
        if (HasInfiniteLives) return;
        
        CurrentLives--;

        if (CurrentLives <= 0) {
            CurrentLives = 0;
            CanRespawn = false;
            OnLivesDepleted?.Invoke(this, entityArgs);
        }
        
        EntityData.currentLives = CurrentLives;
    }

    public void SetInvulnerability() {
        if (HasInvulnerabilityFrames) invulnerabilityCoroutine = StartCoroutine(InvulnerabilityFramesRoutine());
    }

    private IEnumerator InvulnerabilityFramesRoutine() {
        IsInvulnerable = true;
        HitboxTrigger.enabled = false;
        OnEntityDamagedEventArgs args = new OnEntityDamagedEventArgs();
        args.CurrentFlash = InvulnerabilityFlash;
        yield return invulnerabilityStartDelay;
        OnInvulnerabilityStart?.Invoke(null, args);
        yield return invulnerabilityDelay;
        IsInvulnerable = false;
        HitboxTrigger.enabled = true;
        OnInvulnerabilityEnd?.Invoke(this, null);
        yield return null;
    }
}
