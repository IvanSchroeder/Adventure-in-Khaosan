using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHeart : MonoBehaviour {
    public enum HeartState {
        Idle,
        Broken,
        Restored
    }

    public Animator anim;
    public Image heartSprite;
    public HeartState heartState;

    public void UpdateAnimator() {
        heartSprite.enabled = true;

        switch (heartState) {
            case HeartState.Idle:
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
