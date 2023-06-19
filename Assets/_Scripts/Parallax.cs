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
                    currentPosition = Vector3.SmoothDamp(currentPosition, targetPosition, ref velocity, smoothTime * Time.unscaledDeltaTime);
                break;
            }
        }

        currentPosition.SetZ(zPosition);
        transform.position = currentPosition;
    }
}
