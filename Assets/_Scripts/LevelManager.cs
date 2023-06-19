using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class LevelManager : MonoBehaviour {
    public static LevelManager instance;
    public WorldMapManager WorldMapManager { get; private set; }
    public Player Player { get; private set; }

    public List<World> WorldsList;
    public World currentWorld;
    public World lastUnlockedWorld;
    public Level currentLevel;
    public Level lastUnlockedLevel;

    public bool startedLevel = false;
    public bool isInGameplay;
    public float currentTimer;
    
    private void OnEnable() {
        WorldMapManager.OnWorldLoaded += SpawnPlayer;
    }

    private void OnDisable() {
        WorldMapManager.OnWorldLoaded -= SpawnPlayer;
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
    }

    private void Update() {
        if (isInGameplay) {
            currentTimer += Time.deltaTime;
        }
    }

    // private IEnumerator StartLevelRoutine() {
    //     new WaitForSeconds(3f);
    //     return null;
    // }

    public void SpawnPlayer() {
        Player.transform.gameObject.SetActive(true);
        // Player.transform.position = currentLevel.startingSpawnpoint.transform.position;
    }

    public void FinishedLevel() {

    }

    public void RestartLevel() {
        // corutina?
    }

    public void LoadLevelData() {
    }

    public void SaveLevelData() {

    }

    public void RemoveCoin() {

    }
}
