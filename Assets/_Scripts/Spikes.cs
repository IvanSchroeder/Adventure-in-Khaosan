using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public class Spikes : MonoBehaviour, IDamageDealer {
    [field: SerializeField] public Collider2D DamageHitbox { get; set; }
    [field: SerializeField] public LayerMask DamageablesLayers { get; set; }
    [field: SerializeField] public int DamageDealerLayer { get; set; }
    [field: SerializeField] public float DamageAmount { get; set; }

    private void Awake() {
        if (DamageHitbox.IsNull()) DamageHitbox = this.GetComponentInHierarchy<Collider2D>();

        DamageDealerLayer = this.gameObject.layer;
    }

    public void OnTriggerEnter2D(Collider2D collision) {
        IDamageable damagedEntity = collision.GetComponentInHierarchy<IDamageable>();

        if (damagedEntity.IsNull()) return;

        HealthSystem entityHealthSystem = damagedEntity.HealthSystem;

        OnEntityDamagedEventArgs entityArgs = new OnEntityDamagedEventArgs(this, DamageAmount, collision.ClosestPoint(this.transform.position), entityHealthSystem.DamagedFlash);
        damagedEntity.Damage(this, entityArgs);
    }
}
