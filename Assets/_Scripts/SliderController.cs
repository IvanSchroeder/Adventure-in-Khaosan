using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExtensionMethods;

public class SliderController : MonoBehaviour {
    public Slider Slider;
    public IntSO SliderInt;
    public TMP_Text SliderText;

    private void Awake() {
        if (Slider.IsNull()) Slider = this.GetComponentInHierarchy<Slider>();
    }

    private void OnValidate() {
        if (Slider.IsNotNull() && Slider.value != SliderInt.Value) UpdateSlider();
    }

    private void Start() {
        UpdateSlider();
    }

    public void OnSliderChanged(float value) {
        SliderText.text = $"{value.ToInt()}";
        SliderInt.Value = value.ToInt();
    }

    private void UpdateSlider() {
        SliderText.text = $"{SliderInt.Value}";
        Slider.value = SliderInt.Value;
    }
}
