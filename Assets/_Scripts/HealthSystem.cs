using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public class HealthSystem : MonoBehaviour, IDamageable {
    public Player player;

    [field: SerializeField] public HealthType HealthType { get; set; }
    [field: SerializeField] public float MaxHealth { get; set; }
    [field: SerializeField] public float CurrentHealth { get; set; }
    [field: SerializeField] public int MaxHearts { get; set; }
    [field: SerializeField] public int CurrentHearts { get; set; }

    public event Action OnDamaged;

    private void Start() {
        if (player == null) player = this.GetComponentInHierarchy<Player>();

        HealthType = HealthType.Slot;

        switch (HealthType) {
            case HealthType.Numerical:

            break;
            case HealthType.Slot:
                MaxHearts = 3;
                CurrentHearts = MaxHearts;
                // spawn hearts in UI slots, maybe with a list/array and an event?
            break;
        }
    }
}
