using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class Level : ScriptableObject {
    [Header("Level Info")]

    [Space(5)]

    public string levelName;
    public int levelID;
    public GameObject levelStructure;
    // public List<CoinItem> CoinItems;
    // public List<FoodItem> FoodItemsList;
    // public List<Enemy> EnemiesList;
    public List<Checkpoint> CheckpointsList;
    public Checkpoint startingSpawnpoint;
    public Checkpoint currentCheckpoint;
    public Checkpoint furthestCheckpoint;

    [Space(20)]

    [Header("Completion States")]

    [Space(5)]
    public bool isUnlocked = false;
    public bool collectedAllCoins = false;
    public bool collectedAllDishes = false;
    public bool killedAllEnemies = false;
    public bool noHitFinished = false;
    public bool finishedInTimeRecord = false;
    public bool isFinished = false;
    public bool isFullyCompleted = false;

    [Space(20)]
    
    [Header("Entities Info")]

    [Space(5)]
    public int totalCoinsAmount;
    public int totalEnemiesAmount;
    public float baseRecordTime = 60f;

    [Space(20)]
    
    [Header("Check Parameters")]
    public float currentRecordTime;
    public bool wasHit = false;
    

    public void CheckCompletion() {

    }

    public void CheckRecordTime(float currentTimer) {

    }

    public void SetHitStatus() => wasHit = true;
}