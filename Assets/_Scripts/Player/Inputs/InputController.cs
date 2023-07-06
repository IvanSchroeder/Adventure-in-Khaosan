using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ExtensionMethods;
using UnityEngine.Events;
using System;

[CreateAssetMenu(fileName = "New Input Controller", menuName = "Assets/Inputs/Player Inputs")]
public class InputController : ScriptableObject, InputMaster.IGameplayActions {
    public InputMaster inputMaster;

    public event UnityAction JumpKeyDownEvent = delegate { };
    public event UnityAction JumpKeyUpEvent = delegate { };

    public readonly int Idle = Animator.StringToHash("Idle");
    public readonly int Running = Animator.StringToHash("Movement");
    public readonly int Jumping = Animator.StringToHash("Jump");
    public readonly int Falling = Animator.StringToHash("Fall");
    public readonly int Rolling = Animator.StringToHash("Roll");
    public readonly int Hurt = Animator.StringToHash("Hurt");
    public readonly int Attack = Animator.StringToHash("Attack");
    public readonly int Death = Animator.StringToHash("Death");

    [HideInInspector] public InputAction move;
    [HideInInspector] public InputAction jump;

    public Vector2 movementInputs;
    public float horizontalInput;
    public float verticalInput;
    public bool pressingJump;

    public void OnEnable() {
        if (inputMaster == null) {
            inputMaster = new InputMaster();
            inputMaster.Gameplay.SetCallbacks(this);
        }

        inputMaster.Enable();
    }

    public void OnDisable() {
        inputMaster.Disable();
    }

    public void GetActions() {
        move = inputMaster.Gameplay.Movement;
        jump = inputMaster.Gameplay.Jump;
    }

    public void EnableGameplayActions() {
        move.Enable();
        jump.Enable();
    }

    public void DisableGameplayActions() {
        move.Disable();
        jump.Disable();
    }

    public void OnMovement(InputAction.CallbackContext context) {
        movementInputs = context.ReadValue<Vector2>().ToVector2Int();
        horizontalInput = movementInputs.x;
        verticalInput = movementInputs.y;
    }

    public void OnJump(InputAction.CallbackContext context) {
        if (context.action.phase == InputActionPhase.Started) {
            JumpKeyDownEvent?.Invoke();
            pressingJump = true;
        }

        if(context.action.phase == InputActionPhase.Canceled) {
            JumpKeyUpEvent?.Invoke();
            pressingJump = false;
        }
    }

    public void OnGrab(InputAction.CallbackContext context) {
    }

    public void OnAttack(InputAction.CallbackContext context) {
    }

    public void OnInteract(InputAction.CallbackContext context) {
    }

    public void OnUnplatform(InputAction.CallbackContext context) {
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }
}
