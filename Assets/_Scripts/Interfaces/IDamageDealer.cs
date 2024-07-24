using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public interface IDamageDealer {
    Collider2D DamageHitbox { get; set; }
    LayerMask DamageablesLayers { get; set; }
    int DamageDealerLayer { get; set; }
    float DamageAmount { get; set; }
    IntSO DamageInHearts { get; set; }
}
