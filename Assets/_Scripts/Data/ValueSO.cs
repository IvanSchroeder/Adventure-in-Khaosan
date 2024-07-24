using System;
using System.Collections;
using System.Collections.Generic;
using ExtensionMethods;
using UnityEngine;

public abstract class ValueSO<T> : ScriptableObject {
    [SerializeField] private T _value;
    [SerializeField] private T _defaultValue;
    public bool resetsOnEnable;
    public bool resetsOnDisable;

    public T Value {
        get => _value;
        set {
            _value = value;
            OnValueChange?.Invoke(this);
        }
    }

    public T Default {
        get => _defaultValue;
        set {
            _defaultValue = value;
            OnValueChange?.Invoke(this);
        }
    }

    public event Action<ValueSO<T>> OnValueChange;

    public abstract void OnEnable();

    public abstract void OnDisable();
}
