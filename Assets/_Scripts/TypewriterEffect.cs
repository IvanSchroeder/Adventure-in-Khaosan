using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using ExtensionMethods;

[RequireComponent(typeof(TMP_Text))]
public class TypewriterEffect : MonoBehaviour {
    private TMP_Text textBox;

    private int currentVisibleCharacterIndex;

    private Coroutine typewriterCoroutine;
    private WaitForSeconds typewriterStartDelay;
    private WaitForSeconds simpleDelay;
    private WaitForSeconds interpunctuationDelay;

    [Header("Typewriter Settings")]
    [SerializeField] private float typewriterStartDelaySeconds = 1f;
    [SerializeField] private float charactersPerSecond = 20;
    [SerializeField] private float interpunctuationDelaySeconds = 0.5f;

    public bool CurrentlySkipping { get; private set; }
    private WaitForSeconds skipDelay;

    [Header("Skip Settings")]
    [SerializeField] private bool quickSkip;
    [SerializeField][Min(1)] private int skipSpeedup = 5;

    private void Awake() {
        if (textBox.IsNull()) textBox = GetComponent<TMP_Text>();

        typewriterStartDelay = new WaitForSeconds(typewriterStartDelaySeconds);
        simpleDelay = new WaitForSeconds(1 / charactersPerSecond);
        interpunctuationDelay = new WaitForSeconds(interpunctuationDelaySeconds);

        skipDelay = new WaitForSeconds(1 / (charactersPerSecond * skipSpeedup));
    }

    public void StartTypewrite() {
        if (typewriterCoroutine != null) {
            StopCoroutine(typewriterCoroutine);
        }

        ResetText();

        typewriterCoroutine = StartCoroutine(TypewriterRoutine());
    }

    public void StopTypewrite() {
        if (typewriterCoroutine != null) {
            StopCoroutine(typewriterCoroutine);
        }
    }

    public void ResetText() {
        textBox.maxVisibleCharacters = 0;
        currentVisibleCharacterIndex = 0;
    }

    private void Skip() {
        if (CurrentlySkipping) return;

        CurrentlySkipping = true;

        if (!quickSkip) {
            StartCoroutine(SkipSpeedupResetRoutine());
            return;
        }

        StopCoroutine(typewriterCoroutine);
        textBox.maxVisibleCharacters = textBox.textInfo.characterCount;
    }

    IEnumerator TypewriterRoutine() {
        TMP_TextInfo textInfo = textBox.textInfo;

        yield return typewriterStartDelay;

        while (currentVisibleCharacterIndex < textInfo.characterCount) {
            char character = textInfo.characterInfo[currentVisibleCharacterIndex].character;

            textBox.maxVisibleCharacters++;

            if (!CurrentlySkipping && (character == '?' || character == '.' || character == ',' || character == ':' || character == ';' || character == '!' || character == '-')) {
                yield return interpunctuationDelay;
            }
            else {
                yield return CurrentlySkipping ? skipDelay : simpleDelay;
            }

            currentVisibleCharacterIndex++;
        }

        yield return null;
    }

    IEnumerator SkipSpeedupResetRoutine() {
        yield return new WaitUntil(() => textBox.maxVisibleCharacters == textBox.textInfo.characterCount - 1);
        CurrentlySkipping = false;
    }
}
