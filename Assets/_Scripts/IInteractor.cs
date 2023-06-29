using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public interface IInteractor {
    BoxCollider2D InteractorTrigger { get; set; }
    InteractorSystem InteractorSystem { get; set; }
    PlayerInputHandler InputHandler { get; set; }
    IInteractable CurrentInteractable { get; set; }
}
