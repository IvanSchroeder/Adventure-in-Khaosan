using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;
using System.Linq;

public class LevelManager : MonoBehaviour {
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

        if (PlayerInstance != null) {
            PlayerInstance.OnLivesDepleted -= GameOver;
            PlayerInstance.OnPlayerDeathEnd -= RespawnPlayer;
        }

        CheckpointsList = new List<Checkpoint>();
        startingCheckpoint = null;
        currentCheckpoint = null;
        lastCheckpoint = null;

        // List<BoolSO> abilities = new List<BoolSO>(EnabledAbilitiesList.UnifyLists([DisableAbilities]));
        List<BoolSO> abilities = new List<BoolSO>();
        List<BoolSO>[] abilitiesToUnify = { EnabledAbilitiesList , DisabledAbilitiesList };
        abilities = new List<BoolSO>(abilities.UnifyLists(abilitiesToUnify));

        foreach (BoolSO ability in abilities) {
            ability.OnValueChange -= ChangeAbilityInList;
        }
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
        PlayerInstance.transform.position = currentCheckpoint.BasepointTransform.position;
        // PlayerInstance.transform.position = startingCheckpoint.SpawnpointTransform.position;

        PlayerInstance.OnPlayerDeathEnd += RespawnPlayer;
        PlayerInstance.OnLivesDepleted += GameOver;

        PlayerInstance.HealthSystem.HasInfiniteLives = currentLevel.enableInfiniteLives;

        OnPlayerSpawn?.Invoke();

        Debug.Log($"Respawned player");
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

        startingCheckpoint.InteractableSystem.RequiresInput = false;

        Debug.Log("Enabled Checkpoints");
    }

    public void EnableAbilities() {
        EnabledAbilitiesList = new List<BoolSO>();

        foreach (BoolSO ability in currentLevel.AbilitiesToEnableList) {
            ability.OnValueChange += ChangeAbilityInList;
            ability.Value = true;
            // EnabledAbilitiesList.Add(ability);
        }
    }

    public void DisableAbilities() {
        DisabledAbilitiesList = new List<BoolSO>();

        foreach (BoolSO ability in currentLevel.AbilitiesToDisableList) {
            ability.OnValueChange += ChangeAbilityInList;
            ability.Value = false;
            // DisabledAbilitiesList.Add(ability);
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
        Debug.Log("Spawned Level structure");
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
        //PlayerInstance.PlayerSprite.enabled = false;
        // yield return new WaitForSeconds(playerRespawnTimer);
        //PlayerInstance.PlayerSprite.enabled = true;
        // PlayerInstance.transform.position = currentCheckpoint.BasepointTransform.position;
        // // PlayerInstance.transform.position = currentCheckpoint.SpawnpointTransform.position;
        // PlayerInstance.HealthSystem.InitializeHealth();
        // OnPlayerSpawn?.Invoke();

        // PlayerInstance.PlayerSprite.enabled = false;
        if (PlayerInstance.IsNotNull()) {
            PlayerInstance.gameObject.Destroy();
            Debug.Log($"Destroyed player");
        }
        yield return new WaitForSeconds(playerRespawnTimer);
        SpawnPlayer();

        yield return null;
    }
}
