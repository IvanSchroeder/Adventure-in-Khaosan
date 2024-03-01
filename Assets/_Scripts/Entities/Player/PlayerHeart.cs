using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum HeartState {
    Idle,
    Broken,
    Restored
}

public class PlayerHeart : MonoBehaviour {

    public Animator anim;
    public Image heartSprite;
    public HeartState heartState;

    public void UpdateAnimator() {
        heartSprite.enabled = true;

        switch (heartState) {
            case HeartState.Idle:
                anim.SetTrigger("idle");
            break;
            case HeartState.Broken:
                anim.SetTrigger("broken");
            break;
            case HeartState.Restored:
                anim.SetTrigger("restored");
            break;
        }
    }

    public void SetHeartState(HeartState state) {
        heartState = state;

        UpdateAnimator();
    }

    public void DisableSprite() {
        heartSprite.enabled = false;
    }
}
