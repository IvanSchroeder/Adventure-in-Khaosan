using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using Cinemachine;

public class CameraManager : MonoBehaviour {
    [field: SerializeField] public Camera MainCamera { get; private set; }
    [field: SerializeField] public CinemachineVirtualCamera VirtualCamera { get; private set; }

    private void OnEnable() {
        LevelManager.OnPlayerSpawn += SetVirtualCameraTarget;
    }

    private void OnDisable() {
        LevelManager.OnPlayerSpawn -= SetVirtualCameraTarget;
    }

    private void Start() {
        if (MainCamera == null) MainCamera = this.GetMainCamera();
        if (VirtualCamera == null) VirtualCamera = MainCamera.transform.GetComponentInChildren<CinemachineVirtualCamera>();
    }

    private void SetVirtualCameraTarget(Player player) {
        VirtualCamera.m_LookAt = player.transform;
        VirtualCamera.m_Follow = player.transform;
    }
}
