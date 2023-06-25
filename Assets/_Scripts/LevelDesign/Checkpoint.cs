using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

[Serializable]
public class Checkpoint : MonoBehaviour, IInteractable {
    public BoxCollider2D InteractTrigger { get; set; }
    [field: SerializeField] public Transform SpawnpointTransform { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }
    public Animator Anim { get; private set; }

    public int checkpointOrderID;

    [field: SerializeField] public bool RequiresInput { get; set; }
    [field: SerializeField] public bool IsInteractable { get; set; } = true;
    [field: SerializeField] public Material DefaultMaterial { get; set; }
    [field: SerializeField] public Material InteractableOutlineMaterial { get; set; }
    public SpriteFlashConfiguration InteractedFlash { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool isActive = false;
    public bool isStartingCheckpoint = false;
    public bool isFinalCheckpoint = false;

    public static event Action<Checkpoint> OnCheckpointActive;

    private void Awake() {
        if (InteractTrigger == null) InteractTrigger = GetComponentInChildren<BoxCollider2D>();
        if (SpriteRenderer == null) SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (Anim == null) Anim = this.GetComponentInHierarchy<Animator>();
        if (DefaultMaterial == null) DefaultMaterial = SpriteRenderer.material;

        isActive = false;
        IsInteractable = true;
    }

    public void Start() {
        if (isStartingCheckpoint) Interact();
    }

    public void Interact() {
        if (!IsInteractable || isActive) return;

        isActive = true;
        Anim.SetBool("isActive", isActive);
        InteractTrigger.enabled = !isActive;
        
        SetInteraction(false);
        SpriteFlash flash = GetComponentInChildren<SpriteFlash>();
        
        Debug.Log($"Interacted with {this.transform.name}");
        OnCheckpointActive?.Invoke(this);
    }

    public void SetInteraction(bool enable) {
        IsInteractable = enable;

        if (enable && IsInteractable && InteractableOutlineMaterial != null) {
            SpriteRenderer.material = InteractableOutlineMaterial;
        }
        else if (DefaultMaterial != null) {
            SpriteRenderer.material = DefaultMaterial;
            
            SpriteFlash flash = GetComponentInChildren<SpriteFlash>();
            // flash.StartInteractedFlash();
        }
    }
}
