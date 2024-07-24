using System.Collections;
using System.Collections.Generic;
using ExtensionMethods;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Int Data")]
public class IntSO : ValueSO<int> {
    public override void OnEnable() {
        if (resetsOnEnable) Value = Default;
    }

    public override void OnDisable() {
        if (resetsOnDisable) Value = Default;
    }
}
