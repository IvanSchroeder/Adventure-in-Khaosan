using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class PixelGridSnap : MonoBehaviour {
    [Header("Snap Parameters")]
    [SerializeField] private bool snapToPixelGrid = true;
    [SerializeField] private IntSO pixelsPerUnit;
    [SerializeField] public Vector3 snappedCurrentPosition;
    
    private void LateUpdate() {
        if (!snapToPixelGrid) return;

        Vector3 snappedTargetPosition = transform.position;

        if (transform.parent == null) {
            snappedTargetPosition = GetSnappedPosition(transform.position, pixelsPerUnit.Value);
        }
        else if (transform.parent != null) {
            snappedTargetPosition = GetSnappedPosition(transform.parent.position, pixelsPerUnit.Value);
        }

        transform.position = snappedTargetPosition;
        snappedCurrentPosition = transform.position;
    }

    private Vector3 GetSnappedPosition(Vector3 position, float snapPPU) {
        float pixelGridSize = 1f / snapPPU;
        // float x = ((position.x * snapValue).Round() / snapValue);
        // float y = ((position.y * snapValue).Round() / snapValue);
        float x = ((position.x / pixelGridSize).Round() * pixelGridSize);
        float y = ((position.y / pixelGridSize).Round() * pixelGridSize);
        return new Vector3(x, y, position.z);
    }
}
