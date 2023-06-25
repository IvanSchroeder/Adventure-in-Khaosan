using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum HealthType {
    Numerical,
    Hearts
}

[Serializable]
public struct DamageInfo {
    public DamageDealer DamageDealerSource { get; set; }
    public float DamageAmount { get; set; }
    public Vector2 ContactPoint { get; set; }
    public SpriteFlashConfiguration CurrentFlash { get; set; }

    public DamageInfo(DamageDealer damageSource, float damageAmount, Vector2 contactPoint, SpriteFlashConfiguration currentFlash) {
        DamageDealerSource = damageSource;
        DamageAmount = damageAmount;
        ContactPoint = contactPoint;
        CurrentFlash = currentFlash;
    }
}

public interface IDamageable {
    BoxCollider2D HitboxTrigger { get; set; }
    Material SpriteFlashMaterial { get; set; }
    HealthType HealthType { get; set; }
    DamageInfo LastDamageInfo { get; set; }
    bool IsRespawneable { get; set; }
    bool CanRespawn { get; set; }
    int CurrentLives { get; set; }
    int MaxLives { get; set; }
    float MaxHealth { get; set; }
    float CurrentHealth { get; set; }
    int MaxHearts { get; set; }
    int CurrentHearts { get; set; }
    bool IsDead { get; set; }
    bool IsInvulnerable { get; set; }
    float InvulnerabilitySeconds { get; set; }

    void InitializeHealth();
    bool IsDamagedBy(int layer);
    void Damage(DamageInfo damageInfo);
    void SetInvulnerability(DamageInfo damageInfo);
    // void Heal(float amount);
}
