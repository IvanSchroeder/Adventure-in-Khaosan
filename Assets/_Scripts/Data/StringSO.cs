using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/String Data")]
public class StringSO : ValueSO<string> {
    public override void OnEnable() {
        if (resetsOnEnable) Value = Default;
    }

    public override void OnDisable() {
        if (resetsOnDisable) Value = Default;
    }
}
