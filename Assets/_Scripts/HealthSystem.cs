using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public class OnEntityDamagedEventArgs : EventArgs {
    public DamageDealer DamageDealerSource { get; set; }
    public float DamageAmount { get; set; }
    public Vector2 ContactPoint { get; set; }
    public SpriteFlashConfiguration CurrentFlash { get; set; }

    public OnEntityDamagedEventArgs(DamageDealer ds = null, float da = 0f, Vector2 cp = default, SpriteFlashConfiguration cf = null) {
        DamageDealerSource = ds;
        DamageAmount = da;
        ContactPoint = cp;
        CurrentFlash = cf;
    }
}

public class HealthSystem : MonoBehaviour, IDamageable {
    public Entity entity;
    public EntityData entityData;
    public LayerMask damagedBy;
    [field: SerializeField] public BoxCollider2D HitboxTrigger { get; set; }

    [field: SerializeField] public HealthType HealthType { get; set; }
    [field: SerializeField] public SpriteFlashConfiguration DamagedFlash { get; set; }
    [field: SerializeField] public SpriteFlashConfiguration InvulnerableFlash { get; set; }
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

    public EventHandler<OnEntityDamagedEventArgs> OnDamaged;
    public EventHandler<OnEntityDamagedEventArgs> OnInvulnerabilityStart;
    public EventHandler OnInvulnerabilityEnd;
    public EventHandler<OnEntityDamagedEventArgs> OnEntityDead;
    public EventHandler<OnEntityDamagedEventArgs> OnLivesDepleted;

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

        OnEntityDamagedEventArgs entityArgs = new OnEntityDamagedEventArgs(damageDealerSource, damageDealerSource.damageAmount, collision.ClosestPoint(this.transform.position), entityData.damageFlash);
        Damage(entityArgs);
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
    }

    public bool IsDamagedBy(int layer) {
        if (damagedBy.HasLayer(layer)) {
            return true;
        }

        return false;
    }

    public void Damage(OnEntityDamagedEventArgs entityDamagedEventArgs) {
        if (IsInvulnerable || IsDead) return;
        
        if (!IsDamagedBy(entityDamagedEventArgs.DamageDealerSource.damageDealerLayer)) return;

        IsInvulnerable = true;

        switch (HealthType) {
            case HealthType.Numerical:
                CurrentHealth -= entityDamagedEventArgs.DamageAmount;

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

        if(IsDead) {
            Death(entityDamagedEventArgs);
        }
        else {
            OnDamaged?.Invoke(this, entityDamagedEventArgs);
        }
    }

    public void Death(OnEntityDamagedEventArgs entityArgs) {
        CurrentLives--;

        if (CurrentLives <= 0) {
            CurrentLives = 0;
            CanRespawn = false;
            OnLivesDepleted?.Invoke(this, entityArgs);
        }
        
        entityData.currentLives = CurrentLives;

        OnEntityDead?.Invoke(this, entityArgs);
    }

    public void Invulnerable(object sender, OnEntityDamagedEventArgs entityArgs) {
        IsInvulnerable = true;
        HitboxTrigger.enabled = false;
        OnInvulnerabilityStart?.Invoke(this, entityArgs);
    }

    public void SetInvulnerability(object sender, OnEntityDamagedEventArgs entityDamagedEventArgs) {
        invulnerabilityCoroutine = StartCoroutine(InvulnerabilityFramesRoutine(entityDamagedEventArgs));
    }

    private IEnumerator InvulnerabilityFramesRoutine(OnEntityDamagedEventArgs entityDamagedEventArgs) {
        IsInvulnerable = true;
        HitboxTrigger.enabled = false;
        entityDamagedEventArgs.CurrentFlash = entityData.invulnerabilityFlash;
        OnInvulnerabilityStart?.Invoke(this, entityDamagedEventArgs);
        yield return invulnerabilityDelay;
        IsInvulnerable = false;
        HitboxTrigger.enabled = true;
        OnInvulnerabilityEnd?.Invoke(this, entityDamagedEventArgs);
        yield return null;
    }
}
