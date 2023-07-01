using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;
using System.Linq;

public class LevelManager : MonoBehaviour {
    public static LevelManager instance;
    public Player PlayerInstance { get; private set; }
    public WorldMapManager WorldMapManagerInstance { get; private set; }
    public LevelStructure LevelStructure { get; private set; }

    [field: SerializeField] public GameObject PlayerPrefab { get; private set; }
    public List<World> WorldsList;
    public World selectedWorld;
    public World currentWorld;
    public World lastUnlockedWorld;
    public Level selectedLevel;
    public Level currentLevel;
    public Level lastUnlockedLevel;

    // public List<CoinItem> CoinItems;
    // public List<FoodItem> FoodItemsList;
    // public List<Enemy> EnemiesList;
    public List<Checkpoint> CheckpointsList = new List<Checkpoint>();
    public Checkpoint startingCheckpoint;
    public Checkpoint currentCheckpoint;
    public Checkpoint lastCheckpoint;

    public bool startedLevel = false;
    public bool isInGameplay;
    public FloatSO currentTimer;
    public float playerRespawnTimer = 1f;

    private Coroutine loadLevelCoroutine;
    private Coroutine playerSpawnCoroutine;

    public static Action OnLevelLoaded;
    public static Action OnLevelStarted;
    public static Action OnLevelFinished;
    public static Action OnPlayerSpawn;
    
    private void OnEnable() {
        WorldMapManager.OnWorldMapLoaded += SpawnPlayer;
        Checkpoint.OnCheckpointActive += SetCurrentCheckpoint;

        startingCheckpoint = null;
        currentCheckpoint = null;
        lastCheckpoint = null;
    }

    private void OnDisable() {
        WorldMapManager.OnWorldMapLoaded -= SpawnPlayer;
        Checkpoint.OnCheckpointActive -= SetCurrentCheckpoint;

        PlayerInstance.OnLivesDepleted -= GameOver;
        PlayerInstance.OnPlayerDeathEnd -= RespawnPlayer;

        CheckpointsList = new List<Checkpoint>();
        startingCheckpoint = null;
        currentCheckpoint = null;
        lastCheckpoint = null;
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
        if (WorldMapManagerInstance == null) WorldMapManagerInstance = WorldMapManager.instance;

        LoadLevelData();
    }

    private void Update() {
        if (isInGameplay) {
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

        LoadLevel();
    }

    public void SaveLevelData() {

    }

    public void LoadLevel() {
        loadLevelCoroutine = StartCoroutine(LoadLevelRoutine());
    }

    public void FinishLevel() {
        Debug.Log($"Level completed!");
        currentLevel.CheckCompletion(currentTimer.Value);
        OnLevelFinished?.Invoke();
    }

    public void RestartLevel() {
        // corutina?
        startedLevel = false;
        currentTimer.Value = 0f;
    }

    public void GameOver() {
        loadLevelCoroutine = StartCoroutine(GameOverRoutine());
    }

    public void SpawnLevelStructure() {
        var map = GameObject.Instantiate(currentLevel.worldMap);
        LevelStructure = map.GetComponentInHierarchy<LevelStructure>();

        CheckpointsList = new List<Checkpoint>();

        Checkpoint[] checkpointArray = GameObject.FindObjectsOfType<Checkpoint>();

        for (int i = 0; i < checkpointArray.Count(); i++) {
            checkpointArray[i].checkpointOrderID = i;
            CheckpointsList.Add(checkpointArray[i]);
            Debug.Log($"Checkpoint {checkpointArray[i]} added");
        }

        CheckpointsList.OrderBy(ch => ch.checkpointOrderID);

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
        PlayerInstance.transform.position = startingCheckpoint.SpawnpointTransform.position;

        PlayerInstance.OnPlayerDeathEnd += RespawnPlayer;
        PlayerInstance.OnLivesDepleted += GameOver;

        PlayerInstance.HealthSystem.HasInfiniteLives = currentLevel.enableInfiniteLives;

        OnPlayerSpawn?.Invoke();
    }

    public void RespawnPlayer() {
        if (PlayerInstance.HealthSystem.IsRespawneable && PlayerInstance.HealthSystem.CanRespawn) {
            playerSpawnCoroutine = StartCoroutine(RespawnPlayerRoutine());
        }
    }

    public void RemoveCoin() {

    }

    private void EnableCheckpoints() {
        startingCheckpoint.isStartingCheckpoint = true;
        lastCheckpoint.isFinalCheckpoint = true;

        startingCheckpoint.InteractableSystem.Interact();
    }

    public IEnumerator LoadLevelRoutine() {
        SpawnLevelStructure();
        if (!currentLevel.isFinished) currentLevel.currentRecordTime = currentLevel.baseRecordTime;
        // send event to scene manager to enable scene transition
        yield return new WaitForSecondsRealtime(3f);
        EnableCheckpoints();
        isInGameplay = true;
        startedLevel = true;
        OnLevelStarted?.Invoke();
        OnLevelLoaded?.Invoke();
        yield return null;
    }

    public IEnumerator GameOverRoutine() {
        isInGameplay = false;
        yield return new WaitForSecondsRealtime(3f);
        PlayerInstance.gameObject.SetActive(false);
        Debug.Log($"Game Over");
        // send event to scene manager to enable scene transition
        yield return null;
    }

    public IEnumerator RespawnPlayerRoutine() {
        PlayerInstance.gameObject.SetActive(false);
        yield return new WaitForSeconds(playerRespawnTimer);
        PlayerInstance.gameObject.SetActive(true);
        PlayerInstance.transform.position = currentCheckpoint.SpawnpointTransform.position;
        PlayerInstance.Init();
        PlayerInstance.HealthSystem.InitializeHealth();

        OnPlayerSpawn?.Invoke();
        yield return null;
    }
}
