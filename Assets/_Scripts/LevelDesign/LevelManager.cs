using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public class LevelManager : MonoBehaviour {
    public static LevelManager instance;
    public WorldMapManager WorldMapManager { get; private set; }
    public Player Player { get; private set; }
    [field: SerializeField] public GameObject PlayerPrefab { get; private set; }

    public List<World> WorldsList;
    public World currentWorld;
    public World lastUnlockedWorld;
    public Level currentLevel;
    public Level lastUnlockedLevel;
    public Level selectedLevel;

    public bool startedLevel = false;
    public bool isInGameplay;
    public float currentTimer;

    private Coroutine loadLevelCoroutine;
    private Coroutine playerSpawnCoroutine;

    public static Action OnLevelLoaded;
    public static Action<Player> OnPlayerSpawn;
    
    private void OnEnable() {
        WorldMapManager.OnWorldMapLoaded += SpawnPlayer;
        Checkpoint.OnCheckpointActive += SetCurrentCheckpoint;
        HealthSystem.OnLivesDepleted += GameOver;
    }

    private void OnDisable() {
        WorldMapManager.OnWorldMapLoaded -= SpawnPlayer;
        Checkpoint.OnCheckpointActive -= SetCurrentCheckpoint;
        HealthSystem.OnLivesDepleted -= GameOver;

        Player.OnPlayerDeathEnd -= RespawnPlayer;
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
        if (WorldMapManager == null) WorldMapManager = WorldMapManager.instance;

        // Player.transform.gameObject.SetActive(false);
        LoadLevelData();
    }

    private void Update() {
        if (isInGameplay) {
            currentTimer += Time.deltaTime;
        }
    }

    private void SetCurrentCheckpoint(Checkpoint checkpoint) {
        if (currentLevel.currentCheckpoint.checkpointOrderID < checkpoint.checkpointOrderID) currentLevel.furthestCheckpoint = checkpoint;
        currentLevel.currentCheckpoint = checkpoint;

        if (currentLevel.currentCheckpoint.isFinalCheckpoint) {
            Debug.Log($"Level completed!");
        }
    }

    public void SpawnPlayer() {
        var playerObj = GameObject.Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity);
        Player = playerObj.GetComponent<Player>();
        Player.transform.SetParent(null);
        Player.transform.position = currentLevel.startingSpawnpoint.CheckpointTransform.position;
        Player.HealthSystem.SetInvulnerability(Player.HealthSystem.LastDamageInfo);

        Player.OnPlayerDeathEnd += RespawnPlayer;

        OnPlayerSpawn?.Invoke(Player);
    }

    public void RespawnPlayer() {
        if (Player.HealthSystem.IsRespawneable && Player.HealthSystem.CanRespawn) {
            playerSpawnCoroutine = StartCoroutine(RespawnPlayerRoutine(2f));
        }
        else {

        }
    }

    public void LoadLevelData() {
        currentLevel = selectedLevel;
        selectedLevel = null;

        LoadLevel();
    }

    public void LoadLevel() {
        loadLevelCoroutine = StartCoroutine(LoadLevelRoutine());
    }

    public void FinishLevel() {

    }

    public void RestartLevel() {
        // corutina?
    }

    public void SaveLevelData() {

    }

    public void RemoveCoin() {

    }

    public void GameOver() {
        loadLevelCoroutine = StartCoroutine(GameOverRoutine());
    }

    public IEnumerator RespawnPlayerRoutine(float respawnSeconds) {
        Player.gameObject.SetActive(false);
        yield return new WaitForSeconds(respawnSeconds);
        Player.gameObject.SetActive(true);
        Player.transform.position = currentLevel.currentCheckpoint.CheckpointTransform.position;
        Player.Init();
        Player.HealthSystem.InitializeHealth();

        OnPlayerSpawn?.Invoke(Player);
        yield return null;
    }

    public IEnumerator LoadLevelRoutine() {
        currentLevel.SpawnLevelStructure();
        // send event to scene manager to enable scene transition
        yield return new WaitForSecondsRealtime(3f);
        OnLevelLoaded?.Invoke();
        yield return null;
    }

    public IEnumerator GameOverRoutine() {
        yield return new WaitForSecondsRealtime(3f);
        Player.gameObject.SetActive(false);
        Debug.Log($"Its Game Over");
        // send event to scene manager to enable scene transition
        yield return null;
    }
}
