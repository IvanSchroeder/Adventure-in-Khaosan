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

    public Vector2 parallaxFactor;
    [Range(1, 1000)] public int shaderFactor = 1;

    Vector2 startPosition;
    float startZ;
    Vector2 travel => (Vector2)cam.transform.position - startPosition;
    Vector2 modifiedPosition => (Vector2)startPosition + travel * parallaxFactor;

    private void OnDestroy() {
        if (parallaxMaterial != null) {
            Destroy(parallaxMaterial);
        }
    }

    public void Start() {
        texture = this.GetComponentInHierarchy<RawImage>();
        parallaxMaterial = Instantiate(texture.material);
        texture.material = parallaxMaterial;
        startPosition = transform.position;
        startZ = transform.position.z;
    }

    public void Update() {
        parallaxMaterial.SetVector("_travelPosition", modifiedPosition / shaderFactor);
    }
}
