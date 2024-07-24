using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class FadeBlock : MonoBehaviour {
    public enum FadeBlockState {
        Normal,
        Fading,
        Cooldown,
        Restoring
    }

    public SerializedDictionary<FadeBlockState, string> AnimationParameter = new SerializedDictionary<FadeBlockState, string>();

    public Animator anim;
    public BoxCollider2D solidCollider;
    public BoxCollider2D detectionTrigger;
    public FadeBlockState currentState;

    public float restorationTime;

    private bool isSolid;

    private Coroutine fadeRestoreCoroutine;
    private WaitForSeconds fadeRestoreSeconds;

    private void Start() {
        fadeRestoreSeconds = new WaitForSeconds(restorationTime);

        AnimationParameter.Add(FadeBlockState.Normal, "normal");
        AnimationParameter.Add(FadeBlockState.Fading, "fading");
        AnimationParameter.Add(FadeBlockState.Cooldown, "cooldown");
        AnimationParameter.Add(FadeBlockState.Restoring, "restoring");

        ChangeState(FadeBlockState.Normal);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.transform.GetComponentInHierarchy<Player>().IsNull()) return;

        ChangeState(FadeBlockState.Fading);
    }

    public void FadeTile() {
        SetSolidCollider(false);
    }

    public void RestoreTile() {
        SetSolidCollider(true);
    }

    public void ChangeState(FadeBlockState state) {
        if (currentState.IsNotNull()) anim.SetBool(AnimationParameter[currentState], false);
        currentState = state;
        anim.SetBool(AnimationParameter[currentState], true);
        
        UpdateState(currentState);
    }

    public void UpdateState(FadeBlockState state) {
        switch (currentState) {
            case FadeBlockState.Normal:
                RestoreTile();
                detectionTrigger.enabled = true;
            break;
            case FadeBlockState.Fading:
                detectionTrigger.enabled = false;
            break;
            case FadeBlockState.Cooldown:
                if (fadeRestoreCoroutine.IsNotNull()) {
                    StopCoroutine(fadeRestoreCoroutine);
                    fadeRestoreCoroutine = null;
                }

                fadeRestoreCoroutine = StartCoroutine(FadeCooldown());
            break;
            case FadeBlockState.Restoring:
            break;
        }
    }

    public void SetSolidCollider(bool active) {
        isSolid = active;
        solidCollider.enabled = active;
    }

    private IEnumerator FadeCooldown() {
        yield return fadeRestoreSeconds;

        ChangeState(FadeBlockState.Restoring);

        yield return null;
    }

    public void PlaySound(string sound) {
        AudioManager.instance.PlaySFX(sound);
    }
}
