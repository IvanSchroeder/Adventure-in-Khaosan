using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public class Enemy : Entity {
    public EnemyData enemyData;

    public Action<Enemy> OnEnemySpawn;
    public Action<Enemy> OnEnemyDeath;

    private void OnEnable() {
    }

    private void OnDisable() {
    }
    
    private void Awake() {

    }
}
