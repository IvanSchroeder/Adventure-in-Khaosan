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
    public bool enableInfiniteLives = false;
    public List<BoolSO> AbilitiesToEnableList;
    public List<BoolSO> AbilitiesToDisableList;
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
    public bool finishedInRecordTime = false;
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
    public float personalBestTime = 0f;
    public bool wasHit = false;

    public void InitLevel() {
        wasHit = false;
    }
    
    public void CheckCompletion(float currentTimer, int coinsCollected) {
        CheckRecordTime(currentTimer);
        if (!noHitFinished && !wasHit) noHitFinished = true;
        if (!collectedAllCoins && coinsCollected == totalCoinsAmount) collectedAllCoins = true;
    }

    public void CheckRecordTime(float currentTimer) {
        if ((currentTimer < personalBestTime) || (personalBestTime == 0f)) {
            personalBestTime = currentTimer;
            Debug.Log($"New personal best time! ({personalBestTime})");

            if (!finishedInRecordTime && personalBestTime < baseRecordTime) {
                finishedInRecordTime = true;
                Debug.Log($"Finished in record time! ({personalBestTime})");
            }
        }
    }

    public void SetHitStatus() => wasHit = true;
}
