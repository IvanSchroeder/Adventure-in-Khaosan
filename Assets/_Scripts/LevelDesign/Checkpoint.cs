using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

[Serializable]
public class Checkpoint : MonoBehaviour, IInteractable {
    public BoxCollider2D InteractTrigger { get; set; }
    public Transform CheckpointTransform { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }
    public Animator Anim { get; private set; }

    public int checkpointOrderID;

    [field: SerializeField] public bool RequiresInput { get; set; }
    [field: SerializeField] public bool IsInteractable { get; set; }
    [field: SerializeField] public Material InteractableOutlineMaterial { get; set; }
    [field: SerializeField] public Material DefaultMaterial { get; set; }

    public bool isActive = false;
    public bool isStartingCheckpoint = false;
    public bool isFinalCheckpoint = false;

    public event Action<Checkpoint> OnCheckpointActive;

    private void Awake() {
        if (CheckpointTransform == null) CheckpointTransform = this.transform;
        if (InteractTrigger == null) InteractTrigger = GetComponentInChildren<BoxCollider2D>();
        if (SpriteRenderer == null) SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (Anim == null) Anim = this.GetComponentInHierarchy<Animator>();
        if (DefaultMaterial == null) DefaultMaterial = SpriteRenderer.material;

        isActive = false;
    }

    public void Start() {
        if (isStartingCheckpoint) Interact();
    }

    public void SetActive() {
        isActive = true;
        Anim.SetBool("isActive", isActive);
        InteractTrigger.enabled = !isActive;
        OnCheckpointActive?.Invoke(this);
    }

    public void Interact() {
        if (!IsInteractable || isActive) return;
        SetActive();
        SetInteraction(false);
        Debug.Log($"Interacted with {this.transform.name}");
    }

    public void SetInteraction(bool enable) {
        if (InteractableOutlineMaterial == null || DefaultMaterial == null) Debug.Log($"Materials not set");
        if (enable && IsInteractable) SpriteRenderer.material = InteractableOutlineMaterial;
        else SpriteRenderer.material = DefaultMaterial;
    }
}
