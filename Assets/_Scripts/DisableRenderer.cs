using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class DisableRenderer : MonoBehaviour {
    private Renderer rend;
    public bool shouldDisable = false;
    public bool disableOnStart = false;
    public bool isDisabled = false;

    private void OnEnable() {
        WorldMapManager.OnWorldLoaded += Disable;
    }

    private void OnDisable() {
        WorldMapManager.OnWorldLoaded -= Disable;
    }

    private void Awake() {
        if (rend == null) rend = this.GetComponent<Renderer>();
    }
    
    private void Start() {
        if (shouldDisable && disableOnStart) Disable();
    }

    private void Disable() {
        if (!shouldDisable || isDisabled) return;

        isDisabled = true;
        rend.enabled = false;
        rend = null;
    }
}
