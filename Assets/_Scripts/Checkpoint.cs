using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class Checkpoint : MonoBehaviour {
    public Transform CheckpointTransform { get; private set; }
    public BoxCollider2D Collider { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }
    public Animator Anim { get; private set; }
    public int checkpointOrderID;
    public bool isActive = false;
    public bool isFinalCheckpoint = false;

    private void Awake() {
        if (CheckpointTransform == null) CheckpointTransform = this.transform;
        if (Collider == null) Collider = GetComponentInChildren<BoxCollider2D>();
        if (SpriteRenderer == null) SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void SetActive() {
        isActive = true;
        Anim.SetBool("isActive", isActive);
    }
}
