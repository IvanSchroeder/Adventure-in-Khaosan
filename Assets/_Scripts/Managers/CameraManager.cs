using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using Cinemachine;

public class CameraManager : MonoBehaviour {
    public static CameraManager instance;
    [field: SerializeField] public Camera MainCamera { get; private set; }
    [field: SerializeField] public CinemachineVirtualCamera VirtualCamera { get; private set; }
    [field: SerializeField] public Transform CameraTarget { get; private set; }
    [field: SerializeField] public bool DisableCameraTargetOnDeath { get; private set; } = true;

    [Header("Snap Parameters")]
    [SerializeField] private bool snapToPixelGrid = true;
    [SerializeField] private IntSO pixelsPerUnit;
    [SerializeField] public Vector3 snappedCurrentPosition;

    private void OnEnable() {
        Player.OnPlayerSpawned += SetVirtualCameraTarget;
        Player.OnPlayerDeath += DisableCameraTarget;
    }

    private void OnDisable() {
        Player.OnPlayerSpawned -= SetVirtualCameraTarget;
        Player.OnPlayerDeath -= DisableCameraTarget;
    }

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    private void Start() {
        if (MainCamera == null) MainCamera = this.GetMainCamera();
        if (VirtualCamera == null) VirtualCamera = MainCamera.transform.GetComponentInChildren<CinemachineVirtualCamera>();
    }

    private void LateUpdate() {
        if (!snapToPixelGrid) return;

        Vector3 snappedTargetPosition = MainCamera.transform.position;
        snappedTargetPosition = GetSnappedPosition(MainCamera.transform.position, pixelsPerUnit.Value);

        // if (transform.parent == null) {
        //     snappedTargetPosition = GetSnappedPosition(MainCamera.transform.position, pixelsPerUnit.Value);
        // }
        // else if (transform.parent != null) {
        //     snappedTargetPosition = GetSnappedPosition(transform.parent.position, pixelsPerUnit.Value);
        // }

        MainCamera.transform.position = snappedTargetPosition;
        snappedCurrentPosition = MainCamera.transform.position;
    }

    private void SetVirtualCameraTarget(Player player) {
        if (CameraTarget == null) CameraTarget = player.CameraTarget.transform;
        VirtualCamera.m_LookAt = CameraTarget;
        VirtualCamera.m_Follow = CameraTarget;
    }

    private void DisableCameraTarget(Player player) {
        if (DisableCameraTargetOnDeath) CameraTarget.transform.SetParent(null);
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
