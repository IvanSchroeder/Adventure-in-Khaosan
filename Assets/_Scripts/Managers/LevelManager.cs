using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;
using System.Linq;
using DG.Tweening;

public enum GameState {
    MainMenu,
    Gameplay,
    Paused
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

    private Coroutine loadLevelCoroutine;
    private Coroutine playerSpawnCoroutine;

    public static Action OnLevelLoaded;
    public static Action OnLevelStarted;
    public static Action OnLevelFinished;
    public static Action OnLevelRestart;
    public static Action OnGamePaused;
    public static Action OnGameUnpaused;
    public static Action OnPlayerSpawn;
    public static Action<GameState> OnGameStateChanged;
    
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
    }

    private void Start() {
        if (WorldMapManagerInstance.IsNull()) WorldMapManagerInstance = WorldMapManager.instance;

        CurrentGameState = GameState.Gameplay;

        switch (CurrentGameState) {
            case GameState.Gameplay:
                LoadLevelData();
            break;
            case GameState.MainMenu:
            break;
            case GameState.Paused:
            break;
        }
    }

    private void ChangeGameState(GameState state) {
        if (CurrentGameState == state) return;

        CurrentGameState = state;
        OnGameStateChanged?.Invoke(CurrentGameState);
    }

    private void Update() {
        if (isInGameplay && enableTimer) {
            currentTimer.Value += Time.deltaTime;
        }
    }

    private void SetCurrentCheckpoint(object sender, Checkpoint checkpoint) {
        currentCheckpoint = checkpoint;

        if (currentCheckpoint.isFinalCheckpoint) {
            FinishLevel();
        }
    }

    public void LoadLevelData() {
        currentLevel = selectedLevel;
        selectedLevel = null;

        EnableAbilities();
        DisableAbilities();

        LoadLevel();
    }

    public void SaveLevelData() {

    }

    public void LoadLevel() {
        loadLevelCoroutine = StartCoroutine(LoadLevelRoutine());
    }

    public void FinishLevel() {
        enableTimer = false;
        currentLevel.CheckCompletion(currentTimer.Value, coinsCollectedCount.Value);
        OnLevelFinished?.Invoke();
    }

    public void PauseLevel(bool pause) {
        ChangeGameState(GameState.Paused);
        if (pause)
            SetTimeScale(0f);
    }

    public void UnpauseLevel(bool pause) {
        ChangeGameState(GameState.Gameplay);
        if (!pause)
            SetTimeScale(1f);
    }

    public void SetTimeScale(float scale, bool instant = false) {
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, scale, instant == false ? pauseLerpSpeed : 0f).SetUpdate(true);
    }

    public void RestartLevel() {
        // corutina?
        startedLevel = false;
        currentTimer.Value = 0f;
        enableTimer = false;
        isInGameplay = false;

        Debug.Log($"Show restart menu UI");
    }

    public void GameOver() {
        loadLevelCoroutine = StartCoroutine(GameOverRoutine());
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

    public IEnumerator LoadLevelRoutine() {
        SpawnLevelStructure();
        // send event to UI manager to enable scene transition
        yield return new WaitForSecondsRealtime(3f);
        EnableCheckpoints();
        startedLevel = true;
        isInGameplay = true;
        enableTimer = true;
        currentLevel.totalCoinsAmount = coinsInLevelCount;
        OnLevelStarted?.Invoke();
        OnLevelLoaded?.Invoke();
        yield return null;
    }

    public IEnumerator GameOverRoutine() {
        enableTimer = false;
        Debug.Log($"Game Over");
        yield return new WaitForSecondsRealtime(2f);

        isInGameplay = false;
        Debug.Log($"Show restart menu UI");

        yield return null;
        // send event to scene manager to enable scene transition
    }

    public IEnumerator RespawnPlayerRoutine() {
        // if (PlayerInstance.IsNotNull()) {
        //     PlayerInstance.gameObject.Destroy();
        // }

        PlayerInstance.PlayerSprite.enabled = false;

        yield return new WaitForSeconds(playerRespawnTimer);

        PlayerInstance.PlayerSprite.enabled = true;
        SpawnPlayer();

        yield return null;
    }
}
