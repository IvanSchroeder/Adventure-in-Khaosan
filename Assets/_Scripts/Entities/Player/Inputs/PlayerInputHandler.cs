using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour {
    public PlayerInput PlayerInputComponent;
    public InputMaster PlayerInputsMaster;

    public Vector2 RawMovementInput { get; private set; }
    public int LastXInput { get; private set; }
    public int NormInputX { get; private set; }
    public int NormInputY { get; private set; }
    public bool GrabInput { get; private set; }
    public bool JumpInput { get; private set; }
    public bool JumpInputStop { get; private set; }
    public bool JumpInputHold { get; private set; }
    public bool CrouchInput { get; private set; }
    public bool CrouchInputHold { get; private set; }
    public bool CrouchInputStop { get; private set; }
    public bool UnplatformInput { get; private set; }
    public bool InteractInput { get; private set; }
    public bool InteractInputHold { get; private set; }
    public bool InteractInputStop { get; private set; }
    public bool AttackInput { get; private set; }
    public bool AttackInputHold { get; private set; }
    public bool AttackInputStop { get; private set; }

    [field: SerializeField] public bool LockInputs { get; private set; }
    [field: SerializeField] public float InputHoldTime { get; private set; } = 0.2f;
    private float jumpInputStartTime;

    private InputActionMap gameplayMap;
    private InputActionMap uiMap;

    private void OnEnable() {
        //LevelManager.OnLevelStarted += EnableGameplayInputs;
        LevelManager.OnLevelLoaded += ResetInputs;

        LevelManager.OnLevelFinished += DisableGameplayInputs;
        LevelManager.OnLevelFinished += ResetInputs;

        //LevelManager.OnPlayerSpawn += EnableGameplayInputs;
        LevelManager.OnPlayerSpawn += ResetInputs;

        LevelManager.OnGamePaused += DisableGameplayInputs;
        LevelManager.OnGameUnpaused += EnableGameplayInputs;
    }

    private void OnDisable() {
        //LevelManager.OnLevelStarted -= EnableGameplayInputs;
        LevelManager.OnLevelLoaded -= ResetInputs;

        LevelManager.OnLevelFinished -= DisableGameplayInputs;
        LevelManager.OnLevelFinished -= ResetInputs;

        //LevelManager.OnPlayerSpawn -= EnableGameplayInputs;
        LevelManager.OnPlayerSpawn -= ResetInputs;

        LevelManager.OnGamePaused -= DisableGameplayInputs;
        LevelManager.OnGameUnpaused -= EnableGameplayInputs;
    }

    private void Start() {
        PlayerInputsMaster = new InputMaster();

        gameplayMap = PlayerInputComponent.actions.FindActionMap("Gameplay");
        uiMap = PlayerInputComponent.actions.FindActionMap("UI");

        EnableGameplayInputs();
        EnableUIInputs();
        
        ResetInputs();
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

        if (NormInputX != 0) {
            LastXInput = NormInputX;
        }
    }

    public void OnJumpImput(InputAction.CallbackContext context) {
        if (LockInputs) return;

        if (context.started) {
            JumpInput = true;
            JumpInputHold = true;
            JumpInputStop = false;
            jumpInputStartTime = Time.time;
        }
        
        if (context.canceled) {
            JumpInputHold = false;
            JumpInputStop = true;
        }
    }

    public void OnInteractInput(InputAction.CallbackContext context) {
        if (LockInputs) return;

        if (context.started) {
            InteractInput = true;
            InteractInputHold = true;
            InteractInputStop = false;
        }
        
        if (context.canceled) {
            InteractInput = false;
            InteractInputHold = false;
            InteractInputStop = true;
        }
    }

    public void OnUnplatformInput(InputAction.CallbackContext context) {
        if (LockInputs) return;

        if (context.started) {
            UnplatformInput = true;
        }
        
        if (context.canceled) {
            UnplatformInput = false;
        }
    }

    public void OnCrouchInput(InputAction.CallbackContext context) {
        if (LockInputs) return;

        if (context.started) {
            CrouchInput = true;
            CrouchInputHold = true;
            CrouchInputStop = false;
        }
        
        if (context.canceled) {
            CrouchInput = false;
            CrouchInputHold = false;
            CrouchInputStop = true;
        }
    }

    public void OnGrabInput(InputAction.CallbackContext context) {
        if (LockInputs) return;

        if (context.started) GrabInput = true;

        if (context.canceled) GrabInput = false;
    }

    public void OnAttackInput(InputAction.CallbackContext context) {
        if (LockInputs) return;

        if (context.started) {
            AttackInput = true;
            AttackInputHold = true;
            AttackInputStop = false;
        }

        if (context.canceled) {
            AttackInput = false;
            AttackInputHold = false;
            AttackInputStop = true;
        }
    }

    public void UseInteractInput() => InteractInput = false;
    public void UseInteractStopInput() => InteractInputStop = false;
    public void UseCrouchInput() => CrouchInput = false;
    public void UseCrouchStopInput() => CrouchInputStop = false;
    public void UseJumpInput() => JumpInput = false;
    public void UseJumpStopInput() => JumpInputStop = false;
    public void UseAttackInput() => AttackInput = false;
    public void UseAttackStopInput() => AttackInputStop = false;

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

    private void ResetInputs() {
        RawMovementInput = Vector2.zero;
        LastXInput = 0;
        NormInputX = 0;
        NormInputY = 0;
        JumpInput = false;
        JumpInputHold = false;
        JumpInputStop = false;
        GrabInput = false;
        UnplatformInput = false;
        InteractInput = false;
        InteractInputHold = false;
        InteractInputStop = false;
        CrouchInput = false;
        CrouchInputHold = false;
        CrouchInputStop = false;
        AttackInput = false;
        AttackInputStop = false;
        AttackInputHold = false;
    }

    public void EnableGameplayInputs() {
        gameplayMap.Enable();
    }

    public void DisableGameplayInputs() {
        gameplayMap.Disable();
    }

    public void EnableUIInputs() {
        uiMap.Enable();
    }

    public void DisableUIInputs() {
        uiMap.Disable();
    }
}