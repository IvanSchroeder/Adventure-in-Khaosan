using System;
using System.Collections;
using System.Collections.Generic;
using ExtensionMethods;
using UnityEngine;

public abstract class ValueSO<T> : ScriptableObject {
    [SerializeField] private T _value;

    public T Value {
        get => _value;
        set {
            _value = value;
            OnValueChange?.Invoke(_value);
        }
    }

    public event Action<T> OnValueChange;
}
