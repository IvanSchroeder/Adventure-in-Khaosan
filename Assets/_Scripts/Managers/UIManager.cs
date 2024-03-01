using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ExtensionMethods;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour {
    [Header("General References")]
    [Space(5)]
    public static UIManager instance;
    public PlayerData PlayerData;
    public List<PlayerHeart> PlayerHeartsList;
    public IntSO CoinCounter;
    public IntSO MusicVolume;
    public IntSO SFXVolume;
    public FloatSO LevelTimer;

    [Space(20)]

    [Header("Background References")]
    [Space(5)]
    public GameObject titleScreenBackground;
    public GameObject backgroundCanvas;
    public GameObject currentLevelBackground;

    [Space(20)]

    [Header("Hearts")]
    [Space(5)]
    public GameObject HeartSlotPrefab;
    public GameObject HeartsGroupContainer;

    [Space(20)]

    [Header("Canvas Groups")]
    [Space(5)]
    public CanvasGroup MainMenuGroup;
    public CanvasGroup InGameGroup;
    [Space(5)]
    public CanvasGroup TitleScreenMenuGroup;
    public CanvasGroup LevelSelectMenuGroup;
    public CanvasGroup GlosaryMenuGroup;
    public CanvasGroup SettingsMenuGroup;
    [Space(5)]
    public CanvasGroup GameplayGUIGroup;
    public CanvasGroup PauseMenuGroup;
    public CanvasGroup GameOverMenuGroup;
    public CanvasGroup LevelCompletedMenuGroup;
    public CanvasGroup SceneTransitionGroup;

    [Space(20)]

    [Header("Texts")]
    [Space(5)]
    public TMP_Text CoinsText;
    public GameObject LevelTimerPanel;
    public TMP_Text LevelTimerText;
    public TMP_Text VersionText;
    public TMP_Text CompletedTimerText;
    public TMP_Text MusicVolumeText;
    public TMP_Text SFXVolumeText;
    private string currentVersion;

    [Space(20)]

    [Header("Other Values")]
    [Space(5)]
    public float panelFadeSpeed;
    public float InGamePanelYOffset;
    public Color allCoinsColor;

    public static Action<bool> OnPause;
    public static Action<bool> OnPauseAnimationCompleted;
    public float heartRestoreDelaySeconds;
    private WaitForSecondsRealtime heartRestoreDelay;
    private Coroutine gamePausedCoroutine;
    private Coroutine heartRestorationCoroutine;

    private void OnValidate() {
        heartRestoreDelay = new WaitForSecondsRealtime(heartRestoreDelaySeconds);

        UpdateTimerText(LevelTimer);
        UpdateVersionText(currentVersion);
    }

    private void OnEnable() {
        CoinCounter.OnValueChange += UpdateCoinText;
        LevelTimer.OnValueChange += UpdateTimerText;

        LevelManager.OnLevelLoaded += EnablePlayerResources;
        LevelManager.OnPlayerSpawn += RestoreStartingHearts;
        LevelManager.OnLevelFinished += ShowLevelFinishUI;
        LevelManager.OnNewTimeRecord += UpdateCompletedTimer;
        LevelManager.OnGameOver += ShowGameOverUI;
        LevelManager.OnAllCoinsCollected += ChangeCoinTextColor;

        LevelManager.OnGameSessionInitialized += InitializeMainMenuUI;
        LevelManager.OnMainMenuLoadEnd += InitializeMainMenuUI;
        LevelManager.OnGameplayScreenLoadEnd += InitializeGameplayUI;

        Player.OnPlayerDamaged += ReduceHearts;
        Player.OnPlayerHealed += RestoreHearts;
    }

    private void OnDisable() {
        CoinCounter.OnValueChange -= UpdateCoinText;
        LevelTimer.OnValueChange -= UpdateTimerText;

        LevelManager.OnLevelLoaded -= EnablePlayerResources;
        LevelManager.OnPlayerSpawn -= RestoreStartingHearts;
        LevelManager.OnLevelFinished -= ShowLevelFinishUI;
        LevelManager.OnNewTimeRecord -= UpdateCompletedTimer;
        LevelManager.OnGameOver -= ShowGameOverUI;
        LevelManager.OnAllCoinsCollected -= ChangeCoinTextColor;

        LevelManager.OnGameSessionInitialized -= InitializeMainMenuUI;
        LevelManager.OnMainMenuLoadEnd -= InitializeMainMenuUI;
        LevelManager.OnGameplayScreenLoadEnd -= InitializeGameplayUI;

        Player.OnPlayerDamaged -= ReduceHearts;
        Player.OnPlayerHealed -= RestoreHearts;
    }

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    private void Start() {
        heartRestoreDelay = new WaitForSecondsRealtime(heartRestoreDelaySeconds);
    }

    private CanvasGroup currentScreen;
    private CanvasGroup lastScreen;

    public void ChangeScreen(CanvasGroup screenToChange) {
        // if (currentScreen == screenToChange) return;

        lastScreen = currentScreen;

        if (lastScreen.IsNotNull()) SetPanelMenuUI(lastScreen, false);

        currentScreen = screenToChange;

        if (currentScreen.IsNotNull()) SetPanelMenuUI(currentScreen, true);
    }

    public void CreateLevelBackground() {
        currentLevelBackground = Instantiate(LevelManager.instance.selectedLevel.levelBackground, Vector3.zero, default, backgroundCanvas.transform);
        currentLevelBackground.SetActive(true);
        currentLevelBackground.transform.LocalPositionReset();
    }

    private void InitializeMainMenuUI() {
        SetPanelMenuUI(MainMenuGroup, true, true);
        SetPanelMenuUI(InGameGroup, false, true);

        titleScreenBackground.SetActive(true);
        currentLevelBackground.Destroy();

        lastScreen = null;

        ChangeScreen(TitleScreenMenuGroup);

        SetPanelMenuUI(LevelSelectMenuGroup, false, true);
        SetPanelMenuUI(GlosaryMenuGroup, false, true);
        SetPanelMenuUI(SettingsMenuGroup, false, true);

        KillHearts();
    }

    private void InitializeGameplayUI() {
        SetPanelMenuUI(MainMenuGroup, false);
        SetPanelMenuUI(InGameGroup, true, true);

        titleScreenBackground.SetActive(false);
        CreateLevelBackground();

        lastScreen = null;

        ChangeScreen(GameplayGUIGroup);

        SetPanelMenuUI(PauseMenuGroup, false, false, true, InGamePanelYOffset, true);
        SetPanelMenuUI(GameOverMenuGroup, false, false, true, InGamePanelYOffset, true);
        SetPanelMenuUI(LevelCompletedMenuGroup, false, false, true, InGamePanelYOffset, true);

        UpdateTimerText(LevelTimer);
        UpdateVersionText(currentVersion);

        LevelTimerPanel.gameObject.SetActive(false);
        CompletedTimerText.gameObject.SetActive(false);
    }

    private void SetPanelMenuUI(CanvasGroup group, bool enabled, bool instantFade = false, bool displace = false, float displacement = 0f, bool instantDisplacement = false) {
        group.DOFade(enabled == true ? 1f : 0f, instantFade == true ? 0f : panelFadeSpeed).SetUpdate(true);
        if (displace)
            group.GetComponentInChildren<RectTransform>().DOLocalMoveY(displacement, instantDisplacement == true ? 0f : panelFadeSpeed).SetUpdate(true);

        group.blocksRaycasts = enabled;
    }

    public void SetPausedState(bool pause) {
        if (gamePausedCoroutine.IsNotNull()) {
            StopCoroutine(gamePausedCoroutine);
            gamePausedCoroutine = null;
        }

        gamePausedCoroutine = StartCoroutine(GamePauseRoutine(pause));
    }

    public void EnablePlayerResources() {
        InitializeHearts();
        ShowTimerPanel(LevelManager.instance.currentLevel.enableLevelTimer);
    }

    public void ShowTimerPanel(bool enableTimer) {
        LevelTimerPanel.gameObject.SetActive(enableTimer);
        CompletedTimerText.gameObject.SetActive(enableTimer);
    }

    private void ShowLevelFinishUI() {
        SetPanelMenuUI(GameplayGUIGroup, false, true);
        SetPanelMenuUI(LevelCompletedMenuGroup, true, false, true, InGamePanelYOffset);
    }

    private void ShowGameOverUI() {
        SetPanelMenuUI(GameplayGUIGroup, false, true);
        SetPanelMenuUI(GameOverMenuGroup, true, false, true, InGamePanelYOffset);
    }

    public void UpdateVersionText(string version) {
        currentVersion = Application.version;
        VersionText.text = $"v." + version;
    }

    public void UpdateCoinText(ValueSO<int> amount) {
        CoinsText.text = $"{amount.Value}";
    }

    public void ChangeCoinTextColor() {
        CoinsText.color = allCoinsColor;
    }

    public void UpdateTimerText(ValueSO<float> amount) {
        float minutes = Mathf.FloorToInt(amount.Value / 60);
        float seconds = Mathf.FloorToInt(amount.Value % 60);
        float miliseconds = Mathf.FloorToInt((amount.Value * 100f) % 100);

        LevelTimerText.text = $"TIEMPO: {string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, miliseconds)}";
    }

    string completedTimerStyle = "Bold_Gold";

    public void UpdateCompletedTimer(bool isRecord) {
        string timer = LevelTimerText.text;

        Debug.Log($"Is record {isRecord}");

        if (isRecord) {
            CompletedTimerText.text = $"{timer}. <style=\"{completedTimerStyle}\">Nuevo Record!</style>";
        }
        else {
            CompletedTimerText.text = $"{timer}";
        }

    }

    public void UpdateMusicVolumeSlider(ValueSO<int> volume) {
        MusicVolumeText.text = $"{volume.Value}";
    }

    public void UpdateSFXVolumeSlider(ValueSO<int> volume) {
        SFXVolumeText.text = $"{volume.Value}";
    }

    public void UpdateMusicVolumeSlider(int volume) {
        MusicVolumeText.text = $"{volume}";
        MusicVolume.Value = volume;
    }

    public void UpdateSFXVolumeSlider(int volume) {
        SFXVolumeText.text = $"{volume}";
        SFXVolume.Value = volume;
    }

    public void InitializeHearts() {
        PlayerHeartsList = new List<PlayerHeart>();

        KillHearts();

        for (int i = 0; i < PlayerData.maxHearts; i++) {
            GameObject slot = Instantiate(HeartSlotPrefab, HeartsGroupContainer.transform.position, Quaternion.identity);
            slot.name = $"HeartSlot_0{i}";
            slot.transform.SetParent(HeartsGroupContainer.transform);
            slot.transform.localScale = new Vector3(1, 1, 1);

            PlayerHeart heart = slot.GetComponentInChildren<PlayerHeart>();
            heart.transform.name = $"Heart_0{i}";
            PlayerHeartsList.Add(heart);
            heart.SetHeartState(HeartState.Broken);

            heart.DisableSprite();
        }
    }

    public void KillHearts() {
        HeartsGroupContainer.transform.DestroyChildren();
    }

    public void RestoreStartingHearts() {
        RestoreHearts(PlayerData.startingHearts);
    }

    public void RestoreHearts(int heartsToRestore) {
        UpdateHearts(true, heartsToRestore);
    }

    public void ReduceHearts(OnEntityDamagedEventArgs args) {
        UpdateHearts(false, args.DamageInHearts.Value);
    }

    public void UpdateHearts(bool heal, int heartsToRestore) {
        if (heartRestorationCoroutine.IsNotNull()) {
            StopCoroutine(heartRestorationCoroutine);
            heartRestorationCoroutine = null;
        }

        heartRestorationCoroutine = StartCoroutine(HeartsRestorationRoutine(heal, heartsToRestore));
    }

    private IEnumerator GamePauseRoutine(bool pause) {
        float gameplayAlpha = GameplayGUIGroup.alpha;
        float pauseAlpha = PauseMenuGroup.alpha;

        float elapsedTime = 0f;
        float percentage = elapsedTime / panelFadeSpeed;

        if (pause) {
            OnPause?.Invoke(true);

            SetPanelMenuUI(GameplayGUIGroup, false, true);
            SetPanelMenuUI(PauseMenuGroup, true, false, true, 0f, false);

            AudioManager.instance.PlaySFX("PauseOn");
            AudioManager.instance.TogglePauseMusic(false);
        }
        else {
            SetPanelMenuUI(GameplayGUIGroup, true, true);
            SetPanelMenuUI(PauseMenuGroup, false, false, true, InGamePanelYOffset, false);

            AudioManager.instance.PlaySFX("PauseOff");

            yield return new WaitForSecondsRealtime(panelFadeSpeed);
            OnPauseAnimationCompleted?.Invoke(false);
            AudioManager.instance.TogglePauseMusic(true);
        }

        yield return null;
    }

    public IEnumerator ScreenFadeRoutine(bool fadeIn, Action actionToDo) {
        

        if (fadeIn) {
            
        }
        else {

        }

        yield return null;
    }
    
    private IEnumerator HeartsRestorationRoutine(bool heal, int heartsAmount) {
        if (heal) {
            for (int i = 0; i < PlayerHeartsList.Count; i++) {
                if (PlayerHeartsList.GetElement(i).heartState == HeartState.Broken) {
                    if (i + heartsAmount > PlayerData.maxHearts) {
                        int diff = (i + heartsAmount) - PlayerData.maxHearts;
                        heartsAmount -= diff;
                    }

                    for (int j = 0; j < heartsAmount; j++) {
                        PlayerHeartsList.GetElement(i + j).SetHeartState(HeartState.Restored);

                        yield return heartRestoreDelay;
                    }

                    break;
                }

                else continue;
            }
        }
        else {
            for (int i = PlayerHeartsList.Count - 1; i >= 0; i--) {
                if (PlayerHeartsList.GetElement(i).heartState == HeartState.Idle) {
                    if (i - heartsAmount < 0) {
                        int diff = heartsAmount - i - 1;
                        heartsAmount -= diff;
                    }

                    for (int j = 0; j < heartsAmount; j++) {
                        PlayerHeartsList.GetElement(i - j).SetHeartState(HeartState.Broken);                
                        yield return heartRestoreDelay;
                    }
                    break;
                }
                else continue;
            }
        }

        yield return null;
    }
}
