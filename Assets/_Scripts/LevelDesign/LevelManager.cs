using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public class LevelManager : MonoBehaviour {
    public static LevelManager instance;
    public WorldMapManager WorldMapManager { get; private set; }
    public Player Player { get; private set; }

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
    
    private void OnEnable() {
        WorldMapManager.OnWorldMapLoaded += SpawnPlayer;
    }

    private void OnDisable() {
        WorldMapManager.OnWorldMapLoaded -= SpawnPlayer;
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

        Player.transform.gameObject.SetActive(false);
        LoadLevelData();
    }

    private void Update() {
        if (isInGameplay) {
            currentTimer += Time.deltaTime;
        }
    }

    public void SpawnPlayer() {
        Player.transform.gameObject.SetActive(true);
        // Player.transform.position = currentLevel.startingSpawnpoint.transform.position;
    }

    public void LoadLevelData() {
        currentLevel = selectedLevel;
        selectedLevel = null;

        Checkpoint[] Checkpoints = GameObject.FindObjectsOfType<Checkpoint>();

        foreach (Checkpoint checkpoint in Checkpoints) {
            currentLevel.CheckpointsList.Add(checkpoint);
        }

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
        yield return new WaitForSeconds(2f);
        OnLevelLoaded?.Invoke();
        yield return null;
    }
}
