using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class DamageDealer : MonoBehaviour {
    public LayerMask damageableLayers;
    public int damageDealerLayer;
    public float damageAmount;

    private void Awake() {
        damageDealerLayer = this.gameObject.layer;
    }

    // private void OnTriggerEnter2D(Collider2D collision) {
    //     IDamageable damagedEntity = collision.GetComponentInHierarchy<IDamageable>();
    //     Debug.Log($"Detected damageable {collision.transform.name}");    

    //     if (damagedEntity == null) return;

    //     if (damagedEntity.IsDamagedBy(this.gameObject.layer)) {
    //         damagedEntity.Damage(damageAmount, collision.ClosestPoint(collision.transform.position), this);
    //     }

    //     //damagedEntity.Damage(damageAmount, collision.ClosestPoint(collision.transform.position), this);
    // }
}
