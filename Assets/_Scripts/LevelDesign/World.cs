using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

[CreateAssetMenu(fileName = "NewWorld", menuName = "Assets/Data/Level Design/World")]
public class World : ScriptableObject {
    public string worldName;
    public int worldID;
    public int worldNumber;
    public bool isUnlocked = false;
    public bool isFinished = false;
    public bool isCompleted = false;

    public List<Level> LevelsList;
}
