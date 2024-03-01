using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public class OnEntityInteractedEventArgs : EventArgs {
    // public IInteractor InteractorSource { get; set; }
    public InteractorSystem InteractorSystemSource { get; set; }
    public InteractableSystem InteractableSystemSource { get; set; }
    public bool IsInteracted { get; set; }
    public bool ActiveOutline { get; set; }
    public bool ShouldFlash { get; set; }
    public Vector2 ContactPoint { get; set; }
    public SpriteFlashConfiguration CurrentFlash { get; set; }

    public OnEntityInteractedEventArgs(InteractorSystem iss = null, InteractableSystem iss2 = null, bool isInteracted = false, bool activeOutline = true, bool shouldFlash = true, Vector2 cp = default, SpriteFlashConfiguration cf = null) {
        InteractorSystemSource = iss;
        InteractableSystemSource = iss2;
        IsInteracted = isInteracted;
        ActiveOutline = activeOutline;
        ShouldFlash = shouldFlash;
        ContactPoint = cp;
        CurrentFlash = cf;
    }
}

public interface IInteractable {
    BoxCollider2D InteractTrigger { get; set; }
    InteractableSystem InteractableSystem { get; set; }
    SpriteManager SpriteManager { get; set; }
}
