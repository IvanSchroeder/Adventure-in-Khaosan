using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

[Serializable]
public class Checkpoint : MonoBehaviour, IInteractable {
    [field: SerializeField] public BoxCollider2D InteractTrigger { get; set; }
    [field: SerializeField] public InteractableSystem InteractableSystem { get; set; }
    [field: SerializeField] public SpriteManager SpriteManager { get; set; }
    [field: SerializeField] public IInteractor CurrentInteractor { get; set; }

    public SpriteRenderer SpriteRenderer { get; private set; }
    public Animator Anim { get; private set; }
    [field: SerializeField] public Transform SpawnpointTransform { get; private set; }
    [field: SerializeField] public Transform BasepointTransform { get; private set; }

    public int checkpointOrderID;

    public bool isActive = false;
    public bool isStartingCheckpoint = false;
    public bool isFinalCheckpoint = false;

    // public Action<Checkpoint> OnCheckpointActive;
    public static EventHandler<Checkpoint> OnCheckpointActive;
    public EventHandler<OnEntityInteractedEventArgs> OnInteracted;

    private void OnEnable() {
        InteractableSystem.OnInteracted += SetActiveCheckpoint;
    }

    private void OnDisable() {
        InteractableSystem.OnInteracted -= SetActiveCheckpoint;
    }

    private void Awake() {
        if (InteractTrigger == null) InteractTrigger = GetComponentInChildren<BoxCollider2D>();
        if (SpriteRenderer == null) SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (SpriteManager == null) SpriteManager = SpriteRenderer.GetComponent<SpriteManager>();
        if (Anim == null) Anim = this.GetComponentInHierarchy<Animator>();

        isActive = false;
    }

    public void Start() {
        if (isStartingCheckpoint) {
            InteractableSystem.Interact();
        }
    }

    public void SetActiveCheckpoint(object sender, OnEntityInteractedEventArgs entityInteracted) {
        isActive = true;
        Anim.SetBool("isActive", isActive);
        InteractTrigger.enabled = !isActive;

        Debug.Log($"Interacted with {this.transform.name}");

        OnInteracted?.Invoke(this, entityInteracted);
        OnCheckpointActive?.Invoke(this, this);
    }
}
