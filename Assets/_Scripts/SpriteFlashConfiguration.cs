using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

[CreateAssetMenu(fileName = "NewSpriteFlashData", menuName = "Assets/Data/Sprite Flash Data")]
public class SpriteFlashConfiguration : ScriptableObject {
    public enum ColorsListType {
        Set,
        Lerped
    }

    public enum AlphasListType {
        Set,
        Lerped
    }
    
    [field: SerializeField, Header("--- General Parameters ---"), Space(5f)] public float SecondsBetweenFlashes { get; private set; } = 0.1f;
    [field: SerializeField] public bool LoopFlash { get; private set; } = false;
    [field: SerializeField] public int MaxAmountOfFlashes { get; private set; } = 2;

    [field: SerializeField, Space(20f), Header("--- Colors Parameters ---"), Space(5f)] public bool ChangeColor { get; private set; } = true;
    [field: SerializeField] public ColorsListType ColorsType { get; private set; } = ColorsListType.Set;
    [field: SerializeField, ColorUsage(true, true)] public List<Color> SetColorsList { get; private set; }
    [field: SerializeField, ColorUsage(true, true)] public List<Color> LerpedColorsList { get; private set; }
    [HideInInspector] public List<Color> SelectedColorsList { get; private set; }
    [field: SerializeField] public bool InvertColors { get; private set; } = false;
    [field: SerializeField, ColorUsage(true, true)] public Color StartColor { get; private set; } = Color.white;
    [field: SerializeField, ColorUsage(true, true)] public Color EndColor { get; private set; } = Color.black;
    [field: SerializeField] public int ColorsSteps { get; private set; } = 2;
    [field: SerializeField] public int FlashesPerColor { get; private set; } = 1;
    [field: SerializeField] public int TotalColors { get; private set; }

    [field: SerializeField, Range(0f, 1f), Space(20f), Header("--- Flash Amount Parameters ---"), Space(5f)] public float MinFlashAmount { get; private set; } = 0;
    [field: SerializeField, Range(0f, 1f)] public float MaxFlashAmount { get; private set; } = 1;

    [field: SerializeField, Space(20f), Header("--- Alphas Parameters ---"), Space(5f)] public bool ChangeAlpha { get; private set; } = false;
    [field: SerializeField] public AlphasListType AlphasType { get; private set; } = AlphasListType.Set;
    [field: SerializeField] public List<float> SetAlphasList { get; private set; }
    [field: SerializeField] public List<float> LerpedAlphasList { get; private set; }
    [HideInInspector] public List<float> SelectedAlphasList { get; private set; }
    [field: SerializeField] public bool InvertAlphas { get; private set; } = false;
    [field: SerializeField] public int FlashesPerAlpha { get; private set; } = 1;
    [field: SerializeField] public int AlphasSteps { get; private set; } = 2;
    [field: SerializeField] public int TotalAlphas { get; private set; }
    [field: SerializeField, Range(0f, 1f)] public float MinAlphaAmount { get; private set; } = 0;
    [field: SerializeField, Range(0f, 1f)] public float MaxAlphaAmount { get; private set; } = 1;


    private void OnValidate() {
        Init();
    }

    public void Init() {
        GetColorsLists();
        GetAlphasLists();

        MaxAmountOfFlashes = Mathf.Max(FlashesPerColor * TotalColors, FlashesPerAlpha * TotalAlphas);
    }

    private void GetColorsLists() {
        if (ChangeColor) {
            switch (ColorsType) {
                case ColorsListType.Set:
                    TotalColors = SetColorsList.Count;

                    if (InvertColors) {
                        InvertColors = false;
                        SetColorsList = (List<Color>)SetColorsList.Flip();
                    }

                    SelectedColorsList = new List<Color>(SetColorsList);
                break;
                case ColorsListType.Lerped:
                    LerpedColorsList = new List<Color>();

                    float diff = 1f - 0f;
                    float div = ColorsSteps - 1;
                    float increment = diff / div;
                    float currentIncrement = 0;

                    Color newColor = Color.white;

                    // if (InvertColors) {
                    //     InvertColors = false;
                    //     newColor = StartColor;
                    //     StartColor = EndColor;
                    //     EndColor = newColor;
                    // }

                    newColor = StartColor;

                    for (int index = 0; index < ColorsSteps; index++) {
                        if (index == 0) {
                            newColor = StartColor;
                        }
                        else if (index < ColorsSteps - 1) {
                            newColor = Color.Lerp(StartColor, EndColor, currentIncrement);
                        }
                        else {
                            newColor = EndColor;
                        }

                        currentIncrement += increment;
                        LerpedColorsList.Add(newColor);
                    }

                    TotalColors = LerpedColorsList.Count;

                    if (InvertColors) {
                        var tempList = LerpedColorsList.Flip();
                        LerpedColorsList = new List<Color>(tempList);
                    }

                    SelectedColorsList = new List<Color>(LerpedColorsList);
                break;
            }
        }
        else LerpedColorsList = new List<Color>();
    }

    private void GetAlphasLists() {
        float tempFloat = 0f;

        if (MinAlphaAmount > MaxAlphaAmount) {
            tempFloat = MinAlphaAmount;
            MinAlphaAmount = MaxAlphaAmount;
            MaxAlphaAmount = tempFloat;
        }

        if (ChangeAlpha) {
            switch (AlphasType) {
                case AlphasListType.Set:
                    SetAlphasList = new List<float>();
                    SetAlphasList.Add(MinAlphaAmount);
                    SetAlphasList.Add(MaxAlphaAmount);

                    if (InvertAlphas) {
                        InvertAlphas = false;
                        SetAlphasList = (List<float>)SetAlphasList.Flip();
                    }

                    TotalAlphas = SetAlphasList.Count;
                    SelectedAlphasList = new List<float>(SetAlphasList);
                break;
                case AlphasListType.Lerped:
                    LerpedAlphasList = new List<float>();

                    float diff = MaxAlphaAmount - MinAlphaAmount;
                    float div = AlphasSteps - 1;
                    float increment = diff / div;
                    float alphaAmount = MinAlphaAmount;

                    for (int index = 0; index < AlphasSteps; index++) {
                        if (index == 0) {
                            alphaAmount = MinAlphaAmount;
                        }
                        else if (index < AlphasSteps - 1) {
                            alphaAmount += increment;
                        }
                        else {
                            alphaAmount = MaxAlphaAmount;
                        }

                        LerpedAlphasList.Add(alphaAmount);
                    }

                    TotalAlphas = LerpedAlphasList.Count;

                    if (InvertAlphas) {
                        InvertAlphas = false;
                        LerpedAlphasList = (List<float>)LerpedAlphasList.Flip();
                    }

                    SelectedAlphasList = new List<float>(LerpedAlphasList);
                break;
            }
        }
        else {
            SetAlphasList = new List<float>();
            LerpedAlphasList = new List<float>();
        }
    }
}
