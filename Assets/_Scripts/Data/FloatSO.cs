using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Float Data")]
public class FloatSO : ValueSO<float> {
    public override void OnEnable() {
        if (resetsOnEnable) Value = Default;
    }

    public override void OnDisable() {
        if (resetsOnDisable) Value = Default;
    }
}
