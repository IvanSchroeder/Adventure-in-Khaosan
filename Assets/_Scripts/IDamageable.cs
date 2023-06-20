using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HealthType {
    Numerical,
    Slot
}

public interface IDamageable {
    float MaxHealth { get; set; }
    float CurrentHealth { get; set; }
    int MaxHearts { get; set; }
    int CurrentHearts { get; set; }
    HealthType HealthType { get; set; }

    void InitializeHealth() {}
}
