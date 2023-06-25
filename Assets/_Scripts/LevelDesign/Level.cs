using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;
using System.Linq;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Assets/Data/Level Design/Level")]
[Serializable]
public class Level : ScriptableObject {
    [Header("Level Info")]

    [Space(5)]

    public string levelName;
    public int levelID;
    public int levelNumber;
    public GameObject worldMap;
    // public List<CoinItem> CoinItems;
    // public List<FoodItem> FoodItemsList;
    // public List<Enemy> EnemiesList;

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
    // public float baseRecordTime = 60f;
    public float baseRecordTime = 60f;

    [Space(20)]
    
    [Header("Check Parameters")]
    public float currentRecordTime;
    public bool wasHit = false;
    
    public void CheckCompletion(float currentTimer) {
        CheckRecordTime(currentTimer);
    }

    public void CheckRecordTime(float currentTimer) {
        if (currentTimer < currentRecordTime) {
            currentRecordTime = currentTimer;
        }
    }

    public void SetHitStatus() => wasHit = true;
}
