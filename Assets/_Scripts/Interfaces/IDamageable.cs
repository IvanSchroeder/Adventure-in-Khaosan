using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum HealthType {
    Numerical,
    Hearts
}

public class OnEntityDamagedEventArgs : EventArgs {
    public IDamageDealer DamageDealerSource { get; set; }
    public float DamageAmount { get; set; }
    public Vector2 ContactPoint { get; set; }
    public SpriteFlashConfiguration CurrentFlash { get; set; }

    public OnEntityDamagedEventArgs(IDamageDealer ds = null, float da = 0f, Vector2 cp = default, SpriteFlashConfiguration cf = null) {
        DamageDealerSource = ds;
        DamageAmount = da;
        ContactPoint = cp;
        CurrentFlash = cf;
    }
}

public interface IDamageable {
    BoxCollider2D HitboxTrigger { get; set; }
    HealthSystem HealthSystem { get; set; }
    SpriteManager SpriteManager { get; set; }

    void Damage(object sender, OnEntityDamagedEventArgs entityDamagedArgs);

    // LayerMask DamagedBy { get; set; }
    // SpriteFlashConfiguration DamagedFlash { get; set; }
    // SpriteFlashConfiguration InvulnerableFlash { get; set; }
    // HealthType HealthType { get; set; }
    // bool IsRespawneable { get; set; }
    // bool CanRespawn { get; set; }
    // int CurrentLives { get; set; }
    // int MaxLives { get; set; }
    // float MaxHealth { get; set; }
    // float CurrentHealth { get; set; }
    // int MaxHearts { get; set; }
    // int CurrentHearts { get; set; }
    // bool IsDead { get; set; }
    // bool IsInvulnerable { get; set; }
    // float InvulnerabilitySeconds { get; set; }

    // void InitializeHealth();
    // bool IsDamagedBy(int layer);
    // void Damage(OnEntityDamagedEventArgs eventArgs);
    // void SetInvulnerability(object sender, OnEntityDamagedEventArgs eventArgs);
    // void Heal(float amount);
}
