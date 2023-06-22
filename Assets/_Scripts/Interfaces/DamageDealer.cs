using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class DamageDealer : MonoBehaviour {
    public LayerMask damageableLayers;
    public float damageAmount;

    private void OnTriggerEnter2D(Collider2D collision) {
        IDamageable damagedEntity = collision.GetComponentInHierarchy<IDamageable>();

        if (damagedEntity == null) return;

        if (damagedEntity.IsDamagedBy(this.gameObject.layer) && !damagedEntity.IsInvulnerable) {
            damagedEntity.Damage(damageAmount, collision.ClosestPoint(collision.transform.position), this);
        }
    }
}
