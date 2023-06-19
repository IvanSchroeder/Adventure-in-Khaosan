using UnityEditor;
using UnityEngine;
using System;

[CustomPropertyDrawer(typeof(RangeInt), true)]
[CustomPropertyDrawer(typeof(RangeFloat), true)]
[CustomPropertyDrawer(typeof(RangeColor), true)]
public class RangeDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var labelWidth = 30;
        var maxElements = 2;
        var margin = 5;
        var labels = 0;
        var fields = 0;

        var widthPer = Mathf.Max(1, (position.width / maxElements) - labelWidth);

        // Equally spaced elements
        var minLbl = new Rect(position.x + (labelWidth * labels++) + (widthPer * fields), position.y, labelWidth, position.height);
        var minRct = new Rect(position.x + (labelWidth * labels) + (widthPer * fields++), position.y, widthPer - margin, position.height);
        var maxLbl = new Rect(position.x + (labelWidth * labels++) + (widthPer * fields), position.y, labelWidth, position.height);
        var maxRct = new Rect(position.x + (labelWidth * labels) + (widthPer * fields++), position.y, widthPer - margin, position.height);

        EditorGUI.LabelField(minLbl, "Min");
        EditorGUI.PropertyField(minRct, property.FindPropertyRelative("Min"), GUIContent.none);
        EditorGUI.LabelField(maxLbl, "Max");
        EditorGUI.PropertyField(maxRct, property.FindPropertyRelative("Max"), GUIContent.none);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}

[Serializable]
public abstract class Range<T> {
    public T Min;
    public T Max;

    public abstract T RandomInRange();
}

[Serializable]
public class RangeInt : Range<int> {
    public override int RandomInRange() {
        if (Min <= Max) return UnityEngine.Random.Range(Min, Max);
        return UnityEngine.Random.Range(Max, Min);
    }
}

[Serializable]
public class RangeFloat : Range<float> {
    public RangeFloat() {
        Min = 0;
        Max = 1;
    }

    public override float RandomInRange() {
        if (Min <= Max) return UnityEngine.Random.Range(Min, Max);
        return UnityEngine.Random.Range(Max, Min);
    }
}

[Serializable]
public class RangeColor : Range<Color> {
    public RangeColor() {
        Min = Color.HSVToRGB(0f, 0f, 0f);
        Max = Color.HSVToRGB(0.999f, 1f, 1f);
    }

    public override Color RandomInRange() {
        float minH, minS, minV, maxH, maxS, maxV;
        Color.RGBToHSV(Min, out minH, out minS, out minV);
        Color.RGBToHSV(Max, out maxH, out maxS, out maxV);
        return UnityEngine.Random.ColorHSV(minH, maxH, minS, maxS, minV, maxV);
    }
}
