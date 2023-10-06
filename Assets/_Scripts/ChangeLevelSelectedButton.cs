using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLevelSelectedButton : MonoBehaviour {
    public void ChangeLevelSelected(Level levelToChange) {
        LevelManager.instance.selectedLevel = levelToChange;
    }
}
