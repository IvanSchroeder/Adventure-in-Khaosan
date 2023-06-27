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

    private void SetVirtualCameraTarget(Player player) {
        if (CameraTarget == null) CameraTarget = player.CameraTarget.transform;
        VirtualCamera.m_LookAt = CameraTarget;
        VirtualCamera.m_Follow = CameraTarget;
    }

    private void DisableCameraTarget(Player player) {
        // CameraTarget.transform.SetParent(null);
    }
}
