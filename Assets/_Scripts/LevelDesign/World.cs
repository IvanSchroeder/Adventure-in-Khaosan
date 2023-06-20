using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class World : ScriptableObject {
    public string worldName;
    public int worldID;
    public bool isUnlocked = false;
    public bool isFinished = false;
    public bool isCompleted = false;

    public List<Level> LevelsList;
}
