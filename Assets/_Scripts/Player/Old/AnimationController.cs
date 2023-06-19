using UnityEngine;

public class AnimationController : MonoBehaviour {
    // private Animator anim;
    // private SpriteRenderer spriteRenderer;
    // public PlayerData playerData;

    // public void OnEnable() {
    //     PlayerController.StateChangeEvent += UpdateAnimations;
    //     PlayerController.TurnEvent += FlipSprite;
    // }

    // public void OnDisable() {
    //     PlayerController.StateChangeEvent -= UpdateAnimations;
    //     PlayerController.TurnEvent -= FlipSprite;
    // }

    // private void Start() {
    //     if (anim == null) anim = GetComponent<Animator>();
    //     if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    // }

    // public void Update() {
    //     UpdateAnimator();
    // }

    // private void UpdateAnimator() {
    //     //anim.SetFloat("yVelocity", playerData.yVelocity);
    // }

    // private void FlipSprite(bool flip) {
    //     spriteRenderer.flipX = flip;
    // }

    // private void UpdateAnimations(int state) {
    //     if (state == playerData.currentAnimationState) return;

    //     anim.Play(state);
    //     playerData.currentAnimationState = state;
    // }
}
