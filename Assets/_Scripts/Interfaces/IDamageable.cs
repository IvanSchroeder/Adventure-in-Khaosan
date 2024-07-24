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
    public IntSO DamageInHearts { get; set; }
    public Vector2 ContactPoint { get; set; }
    public SpriteFlashConfiguration CurrentFlash { get; set; }

    public OnEntityDamagedEventArgs(IDamageDealer ds = null, float da = 0f, IntSO dh = null, Vector2 cp = default, SpriteFlashConfiguration cf = null) {
        DamageDealerSource = ds;
        DamageAmount = da;
        DamageInHearts = dh;
        ContactPoint = cp;
        CurrentFlash = cf;
    }
}

public interface IDamageable {
    BoxCollider2D HitboxTrigger { get; set; }
    HealthSystem HealthSystem { get; set; }
    SpriteManager SpriteManager { get; set; }

    void Damage(object sender, OnEntityDamagedEventArgs entityDamagedArgs);
}
