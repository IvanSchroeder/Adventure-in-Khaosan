using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour {
    public Vector2 RawMovementInput { get; private set; }
    public int LastXInput { get; private set; }
    public int NormInputX { get; private set; }
    public int NormInputY { get; private set; }
    public bool JumpInput { get; private set; }
    public bool JumpInputStop { get; private set; }
    public bool JumpInputHold { get; private set; }
    public bool GrabInput { get; private set; }
    public bool AttackInput { get; private set; }
    public bool AttackInputHold { get; private set; }

    [field: SerializeField] public bool LockInputs { get; private set; }
    [field: SerializeField] public float InputHoldTime { get; private set; } = 0.2f;
    private float jumpInputStartTime;

    private void OnEnable() {
        LevelManager.OnLevelStarted += UnlockGameplayInputs;
        LevelManager.OnLevelFinished += LockGameplayInputs;
        LevelManager.OnPlayerSpawn += UnlockGameplayInputs;
    }

    private void OnDisable() {
        LevelManager.OnLevelStarted -= UnlockGameplayInputs;
        LevelManager.OnLevelFinished -= LockGameplayInputs;
        LevelManager.OnPlayerSpawn -= UnlockGameplayInputs;
    }

    private void Start() {
        LockInputs = false;
    }

    private void Update() {
        CheckJumpInputHoldTime();
    }

    public void OnMoveInput(InputAction.CallbackContext context) {
        if (LockInputs) return;

        RawMovementInput = context.ReadValue<Vector2>();
        
        NormInputX = Mathf.RoundToInt(RawMovementInput.x);
        NormInputY = Mathf.RoundToInt(RawMovementInput.y);

        if (context.started) {
            LastXInput = NormInputX;
        }

        if (NormInputX != 0) 
            LastXInput = NormInputX;
    }

    public void OnJumpImput(InputAction.CallbackContext context) {
        if (LockInputs) return;

        if (context.started) {
            JumpInput = true;
            JumpInputStop = false;
            JumpInputHold = true;
            jumpInputStartTime = Time.time;
        }
        
        if (context.canceled) {
            JumpInputStop = true;
            JumpInputHold = false;
        }
    }

    public void OnGrabInput(InputAction.CallbackContext context) {
        if (LockInputs) return;

        if (context.started) GrabInput = true;
        if (context.canceled) GrabInput = false;
    }

    public void UseJumpInput() => JumpInput = false;

    private void CheckJumpInputHoldTime() {
        if (Time.time >= jumpInputStartTime + InputHoldTime) {
            JumpInput = false;
        }
    }

    public void UnlockGameplayInputs() {
        LockInputs = false;
    }

    public void LockGameplayInputs() {
        LockInputs = true;
    }
}
