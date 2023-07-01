using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class Parallax : MonoBehaviour {
    public enum PanMode {
        Lerp,
        MoveTowards,
        SmoothDamp
    }

    [SerializeField] private Camera mainCamera;
    [Header("Camera Parameters")]
    [Space(5)]
    [SerializeField] private PanMode panMode = PanMode.Lerp;
    [SerializeField] private Vector3 offsetToCamera;
    [SerializeField] public Vector3 currentPosition;
    [SerializeField] public Vector3 targetPosition;
    [SerializeField] private bool snapToCamera;
    [SerializeField] private float acceleration;
    [SerializeField] private float zPosition = 10f;
    [SerializeField] private float smoothTime = 0.5f;

    [Space(20)]

    [Header("Snap Parameters")]
    [Space(5)]
    [SerializeField] private bool snapToPixelGrid = true;
    [SerializeField] private IntSO pixelsPerUnit;
    [SerializeField] public Vector3 snappedCurrentPosition;
    private Vector3 snappedTargetPosition;

    private Vector3 velocity;
    
    private void Start() {
        if (mainCamera == null) mainCamera = this.GetMainCamera();

        offsetToCamera.z = zPosition;

        if (snapToCamera) {
            if (transform.parent == null) this.transform.SetParent(mainCamera.transform);
        }
        else {
            if (transform.parent != null) this.transform.SetParent(null);
        }

        targetPosition = mainCamera.transform.position + offsetToCamera;
        targetPosition.SetZ(zPosition);
        transform.position = targetPosition;
        currentPosition = transform.position;
    }

    private void LateUpdate() {
        currentPosition = transform.position;
        targetPosition = mainCamera.transform.position + offsetToCamera;

        if (snapToCamera) {
            if (transform.parent == null) this.transform.SetParent(mainCamera.transform);

            currentPosition = targetPosition;
        }
        else {
            if (transform.parent != null) this.transform.SetParent(null);

            switch (panMode) {
                case PanMode.Lerp:
                    currentPosition = Vector3.Lerp(currentPosition, targetPosition, acceleration * Time.unscaledDeltaTime);
                break;
                case PanMode.MoveTowards:
                    currentPosition = Vector3.MoveTowards(currentPosition, targetPosition, acceleration * Time.unscaledDeltaTime);
                break;
                case PanMode.SmoothDamp:
                    currentPosition = Vector3.SmoothDamp(currentPosition, targetPosition, ref velocity, acceleration * smoothTime * Time.unscaledDeltaTime);
                break;
            }
        }

        currentPosition.SetZ(zPosition);
        transform.position = currentPosition;

        if (!snapToPixelGrid) return;

        snappedTargetPosition = transform.position;

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
