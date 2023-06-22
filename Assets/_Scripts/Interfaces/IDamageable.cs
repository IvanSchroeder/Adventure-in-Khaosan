using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HealthType {
    Numerical,
    Hearts
}

public interface IDamageable {
    HealthType HealthType { get; set; }
    float MaxHealth { get; set; }
    float CurrentHealth { get; set; }
    int MaxHearts { get; set; }
    int CurrentHearts { get; set; }
    bool IsDead { get; set; }
    bool IsInvulnerable { get; set; }
    float InvulnerabilitySeconds { get; set; }

    void InitializeHealth();
    bool IsDamagedBy(int layer);
    void Damage(float amount, Vector2 contactPoint, DamageDealer damageSource);
    void SetInvulnerability();
    // void Heal(float amount);
}
