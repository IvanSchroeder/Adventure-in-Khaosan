using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HealthType {
    Numerical,
    Hearts
}

public struct DamageInfo {
    public float DamageAmount { get; set; }
    public Vector2 ContactPoint { get; set; }
    public DamageDealer DamageDealerSource { get; set; }
    public SpriteFlashConfiguration CurrentFlash { get; set; }
    public SpriteFlashConfiguration DamageFlash { get; set; }
    public SpriteFlashConfiguration InvulnerabilityFlash { get; set; }

    public DamageInfo(DamageDealer damageSource, float damageAmount, Vector2 contactPoint, SpriteFlashConfiguration damageFlash, SpriteFlashConfiguration invulnerabilityFlash) {
        DamageDealerSource = damageSource;
        DamageAmount = damageAmount;
        ContactPoint = contactPoint;
        CurrentFlash = null;
        DamageFlash = damageFlash;
        InvulnerabilityFlash = invulnerabilityFlash;
    }
}

public interface IDamageable {
    HealthType HealthType { get; set; }
    DamageInfo LastDamageInfo { get; set; }
    // Material SpriteFlashMaterial { get; set; }
    SpriteFlashConfiguration DamageFlash { get; set; }
    SpriteFlashConfiguration InvulnerabilityFlash { get; set; }
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
    void SetInvulnerability();
    // void Heal(float amount);
}
