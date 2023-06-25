using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Bool Data")]
public class BoolSO : ValueSO<bool> {
    public override void OnEnable() {
        if (resetsOnEnable) Value = Default;
    }

    public override void OnDisable() {
        if (resetsOnDisable) Value = Default;
    }
}
