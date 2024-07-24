using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewFoodItemData", menuName = "Assets/Data/Items/FoodItem")]
public class FoodItemData : ScriptableObject {
    public string foodName;
    [TextArea(3, 10)]
    public string foodDescription;
    public Sprite foodUISprite;
    public Level attachedLevel;
    public bool firstTimeUnlocked;

    public void CheckUnlockStatus() {
        if (!firstTimeUnlocked) firstTimeUnlocked = true;
    }

    public string GetName() {
        return foodName;
    }

    public string GetDescription() {
        return foodDescription;
    }

    public Sprite GetImage() {
        return foodUISprite;
    }
}
