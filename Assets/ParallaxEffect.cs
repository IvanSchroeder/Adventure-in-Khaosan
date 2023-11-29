using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using UnityEngine.UI;

public class ParallaxEffect : MonoBehaviour {
    public Camera cam;
    public Material parallaxMaterial;
    // public Renderer rend;
    public RawImage texture;

    public Vector2 parallaxSpeed;
    [Range(1, 1000)] public int shaderFactor = 1;

    public bool autoScroll;
    public bool addScrollToParallax;
    public float autoScrollSpeed;
    public Vector2 autoScrollDirection;

    Vector2 startPosition;
    float startZ;
    Vector2 travel => (Vector2)cam.transform.position - startPosition;
    Vector2 modifiedPosition => (Vector2)startPosition + travel * parallaxSpeed;

    private void OnValidate() {
        UpdateShaderValues();
    }

    private void OnDestroy() {
        if (parallaxMaterial != null) {
            Destroy(parallaxMaterial);
        }
    }

    public void Awake() {
        if (cam == null) cam = this.GetMainCamera();
    }

    public void Start() {
        texture = this.GetComponentInHierarchy<RawImage>();
        parallaxMaterial = Instantiate(texture.material);
        texture.material = parallaxMaterial;
        UpdateShaderValues();
        startPosition = transform.position;
        startZ = transform.position.z;
    }

    public void Update() {
        parallaxMaterial.SetVector("_TravelPosition", modifiedPosition);
    }

    private void UpdateShaderValues() {
        if (parallaxMaterial != null) {
            parallaxMaterial.SetInt("_AutoScroll", autoScroll ? 1 : 0);
            parallaxMaterial.SetInt("_AddScrollToParallax", addScrollToParallax ? 1 : 0);
            parallaxMaterial.SetFloat("_AutoScrollSpeed", autoScrollSpeed * parallaxSpeed.x);
            parallaxMaterial.SetVector("_ScrollDirection", autoScrollDirection);
            parallaxMaterial.SetFloat("_ShaderFactor", shaderFactor);
        }
    }
}
