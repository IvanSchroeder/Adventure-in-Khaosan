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

    public static Action OnLevelLoaded;
    public static Action<Player> OnPlayerSpawn;
    
    private void OnEnable() {
        WorldMapManager.OnWorldMapLoaded += SpawnPlayer;
        Checkpoint.OnCheckpointActive += SetCurrentCheckpoint;
    }

    private void OnDisable() {
        WorldMapManager.OnWorldMapLoaded -= SpawnPlayer;
        Checkpoint.OnCheckpointActive -= SetCurrentCheckpoint;
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
        if (Player == null) Player = FindObjectOfType<Player>();
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
        OnPlayerSpawn?.Invoke(Player);
    }

    public void RespawnPlayer() {
        Player.transform.position = currentLevel.currentCheckpoint.CheckpointTransform.position;
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

    public IEnumerator LoadLevelRoutine() {
        currentLevel.SpawnLevelStructure();
        // send event to scene manager to enable scene transition
        yield return new WaitForSecondsRealtime(3f);
        OnLevelLoaded?.Invoke();
        yield return null;
    }
}
