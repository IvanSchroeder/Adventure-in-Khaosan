using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;
using System.Linq;
using DG.Tweening;
using UnityEngine.SceneManagement;

public enum SceneIndexes {
    MANAGERS = 0,
    TITLE_SCREEN = 1,
    LEVEL = 2,
}

public enum GameState {
    None,
    MainMenu,
    Gameplay,
    Restarting
}

public class LevelManager : MonoBehaviour {
    public GameState CurrentGameState;

    public static LevelManager instance;
    public WorldMapManager WorldMapManagerInstance { get; private set; }
    public LevelStructure LevelStructure { get; private set; }
    [field: SerializeField] public Player PlayerInstance { get; private set; }

    public List<BoolSO> EnabledAbilitiesList;
    public List<BoolSO> DisabledAbilitiesList;

    [field: SerializeField] public GameObject PlayerPrefab { get; private set; }
    public List<World> WorldsList;
    public World selectedWorld;
    public World currentWorld;
    public World lastUnlockedWorld;
    public Level selectedLevel;
    public Level currentLevel;
    public Level lastUnlockedLevel;

    public List<Coin> CoinsList;
    public IntSO coinsCollectedCount;
    public int coinsInLevelCount;
    // public List<FoodItem> FoodItemsList;
    // public List<Enemy> EnemiesList;
    public List<Checkpoint> CheckpointsList = new List<Checkpoint>();
    public Checkpoint startingCheckpoint;
    public Checkpoint currentCheckpoint;
    public Checkpoint lastCheckpoint;

    public bool startedLevel = false;
    public bool isInGameplay;
    public bool enableTimer;
    public FloatSO currentTimer;
    public float playerRespawnTimer = 1f;
    public float pauseLerpSpeed = 1f;

    public float levelFinishDelaySeconds;
    private WaitForSeconds levelFinishDelay;

    private Coroutine levelHandlerCoroutine;
    private Coroutine playerSpawnCoroutine;

    public static Action OnLevelLoaded;
    public static Action OnLevelFinished;
    public static Action<bool> OnNewTimeRecord;
    public static Action OnLevelRestart;
    public static Action OnGamePaused;
    public static Action OnGameUnpaused;
    public static Action OnGameOver;
    public static Action OnPlayerSpawn;
    public static Action OnAllCoinsCollected;
    public static Action<GameState> OnGameStateChanged;

    private void OnValidate() {
        levelFinishDelay = new WaitForSeconds(levelFinishDelaySeconds);
    }
    
    private void OnEnable() {
        WorldMapManager.OnWorldMapLoaded += SpawnPlayer;
        UIManager.OnPause += PauseLevel;
        UIManager.OnPauseAnimationCompleted += UnpauseLevel;

        Checkpoint.OnCheckpointActive += SetCurrentCheckpoint;

        Coin.OnCoinCreated += RegisterCoin;
        Coin.OnCoinPickup += RemoveCoin;

        startingCheckpoint = null;
        currentCheckpoint = null;
        lastCheckpoint = null;
    }

    private void OnDisable() {
        WorldMapManager.OnWorldMapLoaded -= SpawnPlayer;
        UIManager.OnPause -= PauseLevel;
        UIManager.OnPauseAnimationCompleted -= UnpauseLevel;

        Checkpoint.OnCheckpointActive -= SetCurrentCheckpoint;

        Coin.OnCoinCreated -= RegisterCoin;
        Coin.OnCoinPickup -= RemoveCoin;

        if (PlayerInstance != null) {
            PlayerInstance.OnEntityDamaged -= CheckPlayerHit;
            PlayerInstance.OnLivesDepleted -= GameOver;
            PlayerInstance.OnPlayerDeathEnd -= RespawnPlayer;
        }

        CheckpointsList = new List<Checkpoint>();
        startingCheckpoint = null;
        currentCheckpoint = null;
        lastCheckpoint = null;

        List<BoolSO> abilities = new List<BoolSO>();
        List<BoolSO>[] abilitiesToUnify = { EnabledAbilitiesList , DisabledAbilitiesList };
        abilities = new List<BoolSO>(abilities.UnifyLists(abilitiesToUnify));

        foreach (BoolSO ability in abilities) {
            ability.OnValueChange -= ChangeAbilityInList;
        }
    }

    private void Awake() {
        if (instance.IsNull()) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }

        coinsCollectedCount.Value = 0;
        coinsInLevelCount = 0;

        secondsInLoadingScreen = new WaitForSecondsRealtime(secondsToWaitInLoadingScreen);
        secondsAfterLevelSpawn = new WaitForSecondsRealtime(secondsToWaitAfterLevelSpawn);
    }

    private void Start() {
        if (WorldMapManagerInstance.IsNull()) WorldMapManagerInstance = WorldMapManager.instance;

        CurrentGameState = GameState.None;

        StartCoroutine(InitializeGameSession(ChangeGameState(GameState.MainMenu)));
    }

    // public void ChangeScene(IEnumerator )

    private void Update() {
        if (isInGameplay && enableTimer) {
            currentTimer.Value += Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter)) {
            PauseEditor();
        }
        // else if (Input.GetKeyDown(KeyCode.G)) {
        //     StartCoroutine(LoadScene((int)SceneIndexes.LEVEL, true, (int)SceneIndexes.TITLE_SCREEN, ChangeGameState(GameState.Gameplay)));
        // }
        else if (Input.GetKeyDown(KeyCode.M)) {
            BackToMainMenu();
        }
    }

    public void ExitGame() {
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void PauseEditor() {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = UnityEditor.EditorApplication.isPaused.Toggle();
        #endif
    }

    public void StartGame(Level levelToSelect) {
        selectedLevel = levelToSelect;

        StartCoroutine(LoadGameplayScene(true, (int)SceneIndexes.TITLE_SCREEN, ChangeGameState(GameState.Gameplay)));
    }

    public void BackToMainMenu() {
        StartCoroutine(LoadMainMenuScene(true, (int)SceneIndexes.LEVEL, ChangeGameState(GameState.MainMenu)));
    }

    public void RestartLevel() {
        // corutina?
        startedLevel = false;
        isInGameplay = false;
        enableTimer = false;
        currentTimer.Value = 0f;
        if (currentLevel.IsNotNull()) currentLevel.totalCoinsAmount = 0;

        selectedLevel = currentLevel;

        OnLevelRestart?.Invoke();

        StartCoroutine(RestartGameplayScene(ChangeGameState(GameState.Restarting)));
    }

    public void GameOver() {
        if (levelHandlerCoroutine.IsNotNull()) {
            StopCoroutine(levelHandlerCoroutine);
            levelHandlerCoroutine = null;
        }

        levelHandlerCoroutine = StartCoroutine(GameOverRoutine());
    }

    public void PauseLevel(bool pause) {
        if (pause)
            SetTimeScale(0f);
    }

    public void UnpauseLevel(bool pause) {
        if (!pause)
            SetTimeScale(1f);
    }

    [SerializeField] private CanvasGroup loadingScreenCanvasGroup;
    [SerializeField] private float secondsToWaitInLoadingScreen;
    [SerializeField] private float fadeInSeconds = 1f;
    [SerializeField] private float fadeOutSeconds = 1f;

    private WaitForSecondsRealtime secondsInLoadingScreen;

    public static event Action OnGameSessionInitialized;
    public static event Action OnMainMenuLoadStart;
    public static event Action OnMainMenuLoadEnd;
    public static event Action OnGameplayScreenLoadStart;
    public static event Action OnGameplayScreenLoadEnd;
    public static event Action OnLevelSelectionScreenStart;
    public static event Action OnLevelSelectionScreenEnd;

    private IEnumerator ChangeGameState(GameState state) {
        if (CurrentGameState != state) {
            CurrentGameState = state;
            OnGameStateChanged?.Invoke(CurrentGameState);

            switch (CurrentGameState) {
                case GameState.Gameplay:
                    yield return StartCoroutine(LoadLevelScreenRoutine());
                break;
                case GameState.MainMenu:
                    yield return StartCoroutine(LoadMainMenuScreenRoutine());
                break;
                case GameState.Restarting:
                    yield return StartCoroutine(LoadLevelScreenRoutine());
                    CurrentGameState = GameState.Gameplay;
                break;

            }

            yield return null;
        }
        else yield return null;
    }

    private IEnumerator InitializeGameSession(IEnumerator midLoadRoutine = null, IEnumerator endLoadRoutine = null) {
        int sceneToLoad = (int)SceneIndexes.TITLE_SCREEN;
        Debug.Log($"Loading {SceneManager.GetSceneByBuildIndex(sceneToLoad)} scene");

        loadingScreenCanvasGroup.alpha = 1f;
        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

        while (!loadingOperation.isDone) {
            yield return null;
        }

        Debug.Log($"Waiting for {secondsToWaitInLoadingScreen} seconds");
        yield return secondsInLoadingScreen;

        Time.timeScale = 1f;

        OnGameSessionInitialized?.Invoke();

        if (midLoadRoutine != null) yield return StartCoroutine(midLoadRoutine);

        yield return StartCoroutine(ScreenFade(loadingScreenCanvasGroup, 0f, fadeOutSeconds));

        if (endLoadRoutine != null) yield return StartCoroutine(endLoadRoutine);

        Debug.Log($"Finished loading {SceneManager.GetSceneByBuildIndex(sceneToLoad)} scene");
    }

    private IEnumerator LoadMainMenuScene(bool unloadCurrentScene = false, int sceneToUnload = default, IEnumerator midLoadRoutine = null, IEnumerator endLoadRoutine = null) {
        int sceneToLoad = (int)SceneIndexes.TITLE_SCREEN;

        yield return StartCoroutine(ScreenFade(loadingScreenCanvasGroup, 1f, fadeInSeconds));
        OnMainMenuLoadStart?.Invoke();
        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

        while (!loadingOperation.isDone) {
            yield return null;
        }

        if (unloadCurrentScene) {
            AsyncOperation unloadingOperation = SceneManager.UnloadSceneAsync(sceneToUnload);

            while (!unloadingOperation.isDone) {
                yield return null;
            }
        }

        OnMainMenuLoadEnd?.Invoke();
        yield return secondsInLoadingScreen;

        Time.timeScale = 1f;

        if (midLoadRoutine != null) yield return StartCoroutine(midLoadRoutine);
        
        yield return StartCoroutine(ScreenFade(loadingScreenCanvasGroup, 0f, fadeOutSeconds));
        if (endLoadRoutine != null) yield return StartCoroutine(endLoadRoutine);
    }

    private IEnumerator LoadGameplayScene(bool unloadCurrentScene = false, int sceneToUnload = default, IEnumerator midLoadRoutine = null, IEnumerator endLoadRoutine = null) {
        int sceneToLoad = (int)SceneIndexes.LEVEL;

        yield return StartCoroutine(ScreenFade(loadingScreenCanvasGroup, 1f, fadeInSeconds));
        OnGameplayScreenLoadStart?.Invoke();
        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

        while (!loadingOperation.isDone) {
            yield return null;
        }

        if (unloadCurrentScene) {
            AsyncOperation unloadingOperation = SceneManager.UnloadSceneAsync(sceneToUnload);

            while (!unloadingOperation.isDone) {
                yield return null;
            }
        }

        yield return secondsInLoadingScreen;

        Time.timeScale = 1f;

        OnGameplayScreenLoadEnd?.Invoke();

        if (midLoadRoutine != null) yield return StartCoroutine(midLoadRoutine);

        yield return StartCoroutine(ScreenFade(loadingScreenCanvasGroup, 0f, fadeOutSeconds));

        if (endLoadRoutine != null) yield return StartCoroutine(endLoadRoutine);
    }

    private IEnumerator RestartGameplayScene(IEnumerator midLoadRoutine = null, IEnumerator endLoadRoutine = null) {
        yield return StartCoroutine(ScreenFade(loadingScreenCanvasGroup, 1f, fadeInSeconds));
        KillPlayer();
        DeleteLevelStructure();
        yield return secondsInLoadingScreen;

        Time.timeScale = 1f;

        OnGameplayScreenLoadEnd?.Invoke();

        if (midLoadRoutine != null) yield return StartCoroutine(midLoadRoutine);

        yield return StartCoroutine(ScreenFade(loadingScreenCanvasGroup, 0f, fadeOutSeconds));

        if (endLoadRoutine != null) yield return StartCoroutine(endLoadRoutine);
    }

    private void SetCurrentCheckpoint(object sender, Checkpoint checkpoint) {
        currentCheckpoint = checkpoint;

        if (currentCheckpoint.isFinalCheckpoint) {
            FinishLevel();
        }
    }

    public void SaveLevelData() {

    }

    public void FinishLevel() {
        if (levelHandlerCoroutine.IsNotNull()) {
            StopCoroutine(levelHandlerCoroutine);
            levelHandlerCoroutine = null;
        }

        levelHandlerCoroutine = StartCoroutine(LevelFinishRoutine());
    }

    public IEnumerator LoadMainMenuScreenRoutine() {
        // currentLevel = null;
        // selectedLevel = null;
        // selectedWorld = null;

        startedLevel = false;
        isInGameplay = false;
        enableTimer = false;
        currentTimer.Value = 0f;
        if (currentLevel.IsNotNull()) currentLevel.totalCoinsAmount = 0;

        DeleteLevelStructure();
        KillPlayer();

        yield return null;
    }

    [SerializeField] private float secondsToWaitAfterLevelSpawn;
    private WaitForSecondsRealtime secondsAfterLevelSpawn;

    public IEnumerator LoadLevelScreenRoutine() {
        yield return new WaitForSecondsRealtime(2f);

        currentLevel = selectedLevel;
        selectedLevel = null;

        EnableAbilities();
        DisableAbilities();

        SpawnLevelStructure();

        OnLevelLoaded?.Invoke();

        yield return secondsAfterLevelSpawn;

        EnableCheckpoints();
        startedLevel = true;
        isInGameplay = true;
        enableTimer = true;
        currentLevel.totalCoinsAmount = coinsInLevelCount;

        yield return null;
    }
    public IEnumerator LevelFinishRoutine() {
        enableTimer = false;
        currentLevel.CheckCompletion(currentTimer.Value, coinsCollectedCount.Value);

        yield return levelFinishDelay;
        
        if (currentLevel.enableLevelTimer) {
            bool record = currentLevel.GetRecordStatus();
            OnNewTimeRecord?.Invoke(record);
        }

        isInGameplay = false;
        OnLevelFinished?.Invoke();

        yield return null;
    }

    public void SetTimeScale(float scale, bool instant = false) {
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, scale, instant == false ? pauseLerpSpeed : 0f).SetUpdate(true);
    }

    public void SpawnLevelStructure() {
        var map = GameObject.Instantiate(currentLevel.worldMap);
        LevelStructure = map.GetComponentInHierarchy<LevelStructure>();

        CheckpointsList = new List<Checkpoint>();

        Checkpoint[] checkpointArray = GameObject.FindObjectsOfType<Checkpoint>().OrderBy(ch => ch.checkpointOrderID).ToArray();

        for (int i = 0; i < checkpointArray.Count(); i++) {
            CheckpointsList.Add(checkpointArray[i]);
        }

        startingCheckpoint = CheckpointsList.GetFirstElement();
        currentCheckpoint = startingCheckpoint;
        lastCheckpoint = CheckpointsList.GetLastElement();
    }

    public void DeleteLevelStructure() {
        CheckpointsList = new List<Checkpoint>();

        startingCheckpoint = null;
        currentCheckpoint = null;
        lastCheckpoint = null;

        LevelStructure?.gameObject?.Destroy();
    }

    public void SpawnPlayer() {
        Player player = FindObjectOfType<Player>();

        if (player.IsNotNull()) {
            PlayerInstance = player;
        }
        else {
            var playerObj = GameObject.Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity);
            playerObj.name = "Player";
            PlayerInstance = playerObj.GetComponent<Player>();
        }

        PlayerInstance.transform.SetParent(null);
        PlayerInstance.transform.position = currentCheckpoint.SpawnpointTransform.position;

        PlayerInstance.OnEntityDamaged += CheckPlayerHit;
        PlayerInstance.OnPlayerDeathEnd += RespawnPlayer;
        PlayerInstance.OnLivesDepleted += GameOver;

        PlayerInstance.HealthSystem.HasInfiniteLives = currentLevel.enableInfiniteLives;

        OnPlayerSpawn?.Invoke();

        Debug.Log($"Respawned player");
    }

    public void KillPlayer() {
        if (PlayerInstance.IsNotNull()) {
            PlayerInstance.gameObject.Destroy();
        }
    }

    private IEnumerator ScreenFade(CanvasGroup canvasGroup, float targetAlpha, float duration = 1f) {
        float elapsedTime = 0f;
        float percentageComplete = elapsedTime / duration;
        float startValue = canvasGroup.alpha;

        while (elapsedTime < duration) {
            canvasGroup.alpha = Mathf.Lerp(startValue, targetAlpha, percentageComplete);
            elapsedTime += Time.unscaledDeltaTime;
            percentageComplete = elapsedTime / duration;

            yield return Utils.waitForEndOfFrame;
        }

        canvasGroup.alpha = targetAlpha;
    }

    public IEnumerator GameOverRoutine() {
        enableTimer = false;

        yield return levelFinishDelay;

        isInGameplay = false;
        OnGameOver?.Invoke();

        yield return null;
        // send event to scene manager to enable scene transition
    }

    public IEnumerator RespawnPlayerRoutine() {
        PlayerInstance.Sprite.enabled = false;

        yield return new WaitForSeconds(playerRespawnTimer);

        PlayerInstance.Sprite.enabled = true;
        SpawnPlayer();

        yield return null;
    }

    private void CheckPlayerHit(object sender, OnEntityDamagedEventArgs args) {
        if (!currentLevel.wasHit) currentLevel.SetHitStatus();
    }

    public void RespawnPlayer() {
        if (PlayerInstance.HealthSystem.IsRespawneable && PlayerInstance.HealthSystem.CanRespawn) {
            playerSpawnCoroutine = StartCoroutine(RespawnPlayerRoutine());
        }
    }

    private void RegisterCoin(Coin coin) {
        CoinsList.Add(coin);
        coinsInLevelCount++;
    }

    private void RemoveCoin(Coin coin) {
        CoinsList.Remove(coin);
        coinsCollectedCount.Value++;
        
        if (coinsCollectedCount.Value >= currentLevel.totalCoinsAmount) {
            coinsCollectedCount.Value = currentLevel.totalCoinsAmount;
            OnAllCoinsCollected?.Invoke();
            Debug.Log($"Collected all coins!");
        }
    }

    private void EnableCheckpoints() {
        startingCheckpoint.isStartingCheckpoint = true;
        lastCheckpoint.isFinalCheckpoint = true; 

        startingCheckpoint.InteractableSystem.RequiresInput = false;
    }

    public void EnableAbilities() {
        EnabledAbilitiesList = new List<BoolSO>();

        foreach (BoolSO ability in currentLevel.AbilitiesToEnableList) {
            ability.OnValueChange += ChangeAbilityInList;
            ability.Value = true;
        }
    }

    public void DisableAbilities() {
        DisabledAbilitiesList = new List<BoolSO>();

        foreach (BoolSO ability in currentLevel.AbilitiesToDisableList) {
            ability.OnValueChange += ChangeAbilityInList;
            ability.Value = false;
        }
    }

    public void ChangeAbilityInList(ValueSO<bool> ability) {
        if (ability.Value) {
            if (DisabledAbilitiesList.Find(ability => ability)) DisabledAbilitiesList.Remove(ability as BoolSO);
            EnabledAbilitiesList.Add(ability as BoolSO);
        }
        else {
            if (EnabledAbilitiesList.Find(ability => ability)) EnabledAbilitiesList.Remove(ability as BoolSO);
            DisabledAbilitiesList.Add(ability as BoolSO);
        }
    }
}
