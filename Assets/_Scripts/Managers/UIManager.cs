using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour {
    public static UIManager instance;

    public GameObject GameplayUIPrefab;
    public GameObject HeartSlotPrefab;
    public GameObject GameplayUI;
    public Canvas PlayerResources;
    public GameObject HeartsGroup;
    public Canvas PlayerCoins;
    public PlayerData playerData;
    public List<PlayerHeart> PlayerHeartsList;

    private void OnEnable() {
        LevelManager.OnLevelLoaded += InitializeHearts;
        LevelManager.OnPlayerSpawn += RestoreAllHearts;
        Player.OnPlayerDamaged += ReduceHearts;
        
    }

    private void OnDisable() {
        LevelManager.OnLevelLoaded -= InitializeHearts;
        LevelManager.OnPlayerSpawn -= RestoreAllHearts;
        Player.OnPlayerDamaged -= ReduceHearts;
    }

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    public void InitializeHearts() {
        for (int i = 0; i < playerData.maxHearts; i++) {
            GameObject slot = Instantiate(HeartSlotPrefab, HeartsGroup.transform.position, Quaternion.identity);
            slot.name = $"HeartSlot_0{i}";
            slot.transform.SetParent(HeartsGroup.transform);
            slot.transform.localScale = new Vector3(1, 1, 1);

            PlayerHeart heart = slot.GetComponentInChildren<PlayerHeart>();
            heart.transform.name = $"Heart_0{i}";
            PlayerHeartsList.Add(heart);

            heart.DisableSprite();

            // if (i < playerData.currentHearts) {
            //     heart.SetHeartState(PlayerHeart.HeartState.Restored);
            // }
            // else {
            //     heart.DisableSprite();
            // }
        }
    }

    public void RestoreAllHearts() {
        foreach (PlayerHeart heart in PlayerHeartsList) {
            heart.SetHeartState(PlayerHeart.HeartState.Restored);
        }
    }

    public void ReduceHearts() {
        PlayerHeartsList[playerData.currentHearts].SetHeartState(PlayerHeart.HeartState.Broken);
    }

    public void EnablePlayerUI() {
        // GameplayUI = Instantiate(GameplayUIPrefab);
    }
}
