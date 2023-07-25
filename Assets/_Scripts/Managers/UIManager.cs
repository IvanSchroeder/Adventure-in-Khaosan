using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour {
    public static UIManager instance;
    public PlayerData PlayerData;
    public List<PlayerHeart> PlayerHeartsList;
    public IntSO CoinCounter;
    public FloatSO LevelTimer;

    public GameObject HeartSlotPrefab;
    public GameObject MainMenuUI;
    public GameObject GameplayUI;
    public GameObject PauseMenuUI;
    public CanvasGroup MainMenuGroup;
    public CanvasGroup GameplayGUIGroup;
    public CanvasGroup PauseMenuGroup;
    public RectTransform PauseMenuPanelRect;
    public GameObject HeartsGroup;
    public TMP_Text CoinsText;
    public TMP_Text LevelTimerText;
    public TMP_Text VersionText;
    public string CurrentVersion;

    public float panelFadeSpeed;
    public float PausePanelYOffset;
    public static Action<bool> OnPause;
    public static Action<bool> OnPauseAnimationCompleted;

    private Coroutine gamePausedCoroutine;

    private void OnValidate() {
        UpdateTimerText(LevelTimer);

        UpdateVersionText(CurrentVersion);
    }

    private void OnEnable() {
        CoinCounter.OnValueChange += UpdateCoinText;
        LevelTimer.OnValueChange += UpdateTimerText;

        // LevelManager.OnGameStateChanged += SetUIState;
        LevelManager.OnLevelLoaded += EnablePlayerResources;
        LevelManager.OnPlayerSpawn += RestoreAllHearts;

        Player.OnPlayerDamaged += ReduceHearts;
    }

    private void OnDisable() {
        CoinCounter.OnValueChange -= UpdateCoinText;
        LevelTimer.OnValueChange -= UpdateTimerText;

        // LevelManager.OnGameStateChanged -= SetUIState;
        LevelManager.OnLevelLoaded -= EnablePlayerResources;
        LevelManager.OnPlayerSpawn -= RestoreAllHearts;

        Player.OnPlayerDamaged -= ReduceHearts;
    }

    private void Start() {
        SetPanelMenuUI(MainMenuGroup, false);
        SetPanelMenuUI(GameplayGUIGroup, true, true);
        SetPanelMenuUI(PauseMenuGroup, false, false, true, PausePanelYOffset);

        // SetCanvasGroupAlpha(MainMenuGroup, 0f);
        // SetCanvasGroupAlpha(GameplayGUIGroup, 1f);
        // SetCanvasGroupAlpha(PauseMenuGroup, 0f);
        // PauseMenuPanelRect.DOLocalMoveY(PausePanelYOffset, 0f).SetUpdate(true);
    }

    private void SetPanelMenuUI(CanvasGroup group, bool enabled, bool instantFade = false, bool displace = false, float displacement = 0f, bool instantDisplacement = false) {
        group.DOFade(enabled == true ? 1f : 0f, instantFade == true ? 0f : panelFadeSpeed).SetUpdate(true);
        if (displace) group.GetComponentInHierarchy<RectTransform>().DOLocalMoveY(displacement, instantDisplacement == true ? 0f : panelFadeSpeed).SetUpdate(true);

        group.interactable = enabled;
    }

    private void SetCanvasGroupAlpha(CanvasGroup canvasGroup, float targetAlpha) {
        canvasGroup.alpha = targetAlpha;
    }

    public void SetUIState(GameState state) {
        switch (state) {
            case GameState.MainMenu:
                MainMenuUI.SetActive(true);

                GameplayUI.SetActive(false);
                PauseMenuUI.SetActive(false);
            break;
            case GameState.Gameplay:
                GameplayUI.SetActive(true);

                MainMenuUI.SetActive(false);
                PauseMenuUI.SetActive(false);
            break;
            case GameState.Paused:
                PauseMenuUI.SetActive(true);

                MainMenuUI.SetActive(false);
                GameplayUI.SetActive(false);
            break;
        }
    }

    public void SetPausedState(bool pause) {
        if (gamePausedCoroutine.IsNotNull()) {
            StopCoroutine(gamePausedCoroutine);
            gamePausedCoroutine = null;
        }

        gamePausedCoroutine = StartCoroutine(GamePauseRoutine(pause));
    }

    private IEnumerator GamePauseRoutine(bool pause) {
        float gameplayAlpha = GameplayGUIGroup.alpha;
        float pauseAlpha = PauseMenuGroup.alpha;

        float elapsedTime = 0f;
        float percentage = elapsedTime / panelFadeSpeed;

        if (pause) {
            OnPause?.Invoke(true);

            SetPanelMenuUI(GameplayGUIGroup, false, true);
            SetPanelMenuUI(PauseMenuGroup, true, false, true, 0f);

            // GameplayGUIGroup.DOFade(0f, panelFadeSpeed).SetUpdate(true);
            // PauseMenuGroup.DOFade(1f, panelFadeSpeed).SetUpdate(true);
            // PauseMenuPanelRect.DOLocalMoveY(0f, panelFadeSpeed).SetUpdate(true);

            // while (elapsedTime < PauseSpeed) {
            //     // gameplayAlpha = Mathf.Lerp(gameplayAlpha, 0f, percentage);
            //     // pauseAlpha = Mathf.Lerp(pauseAlpha, 1f, percentage);

            //     gameplayAlpha = gameplayAlpha.LerpTo(0f, percentage);
            //     pauseAlpha = gameplayAlpha.LerpTo(1f, percentage);

            //     elapsedTime += Time.unscaledDeltaTime;
            //     percentage = elapsedTime / PauseSpeed;

            //     SetCanvasGroupAlpha(GameplayGUIGroup, gameplayAlpha);
            //     SetCanvasGroupAlpha(PauseMenuGroup, pauseAlpha);
            //     yield return null;
            // }

            // SetCanvasGroupAlpha(GameplayGUIGroup, 0f);
            // SetCanvasGroupAlpha(PauseMenuGroup, 1f);
        }
        else {
            SetPanelMenuUI(GameplayGUIGroup, true, true);
            SetPanelMenuUI(PauseMenuGroup, false, false, true, PausePanelYOffset);

            // GameplayGUIGroup.DOFade(1f, panelFadeSpeed).SetUpdate(true);
            // PauseMenuGroup.DOFade(0f, panelFadeSpeed).SetUpdate(true);
            // PauseMenuPanelRect.DOLocalMoveY(PausePanelYOffset, panelFadeSpeed).SetUpdate(true);

            // while (elapsedTime < PauseSpeed) {
            //     // gameplayAlpha = Mathf.Lerp(gameplayAlpha, 1f, percentage);
            //     // pauseAlpha = Mathf.Lerp(pauseAlpha, 0f, percentage);

            //     gameplayAlpha = gameplayAlpha.LerpTo(1f, percentage);
            //     pauseAlpha = gameplayAlpha.LerpTo(0f, percentage);

            //     elapsedTime += Time.unscaledDeltaTime;
            //     percentage = elapsedTime / PauseSpeed;

            //     SetCanvasGroupAlpha(GameplayGUIGroup, gameplayAlpha);
            //     SetCanvasGroupAlpha(PauseMenuGroup, pauseAlpha);
            //     yield return null;
            // }

            // SetCanvasGroupAlpha(GameplayGUIGroup, 1f);
            // SetCanvasGroupAlpha(PauseMenuGroup, 0f);

            OnPauseAnimationCompleted?.Invoke(false);
        }

        yield return null;
    }

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }

        UpdateVersionText(CurrentVersion);
    }

    public void EnablePlayerResources() {
        InitializeHearts();
    }

    public void UpdateVersionText(string version) {
        CurrentVersion = Application.version;
        VersionText.text = $"v." + version;
    }

    public void UpdateCoinText(ValueSO<int> amount) {
        CoinsText.text = $"{amount.Value}";
    }

    public void UpdateTimerText(ValueSO<float> amount) {
        float minutes = Mathf.FloorToInt(amount.Value / 60);
        float seconds = Mathf.FloorToInt(amount.Value % 60);
        float miliseconds = Mathf.FloorToInt((amount.Value * 100f) % 100);

        LevelTimerText.text = $"TIEMPO: {string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, miliseconds)}";
    }

    public void InitializeHearts() {
        for (int i = 0; i < PlayerData.maxHearts; i++) {
            GameObject slot = Instantiate(HeartSlotPrefab, HeartsGroup.transform.position, Quaternion.identity);
            slot.name = $"HeartSlot_0{i}";
            slot.transform.SetParent(HeartsGroup.transform);
            slot.transform.localScale = new Vector3(1, 1, 1);

            PlayerHeart heart = slot.GetComponentInChildren<PlayerHeart>();
            heart.transform.name = $"Heart_0{i}";
            PlayerHeartsList.Add(heart);

            heart.DisableSprite();
        }
    }

    public void RestoreAllHearts() {
        StartCoroutine(RestoreHeartsRoutine());
    }

    private IEnumerator RestoreHeartsRoutine() {
        foreach (PlayerHeart heart in PlayerHeartsList) {
            heart.SetHeartState(PlayerHeart.HeartState.Restored);
            yield return new WaitForSecondsRealtime(0.2f);
        }

        yield return null;
    }

    public void ReduceHearts() {
        PlayerHeartsList[PlayerData.currentHearts].SetHeartState(PlayerHeart.HeartState.Broken);
    }

    public void EnablePlayerUI() {
        // GameplayUI = Instantiate(GameplayUIPrefab);
    }
}
