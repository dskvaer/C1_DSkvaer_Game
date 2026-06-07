using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class InspectorLabelAttribute : PropertyAttribute {
    public string label;
    public InspectorLabelAttribute(string label)
    {
        this.label = label;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(InspectorLabelAttribute))]
public class InspectorLabelDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        InspectorLabelAttribute labelAttribute = attribute as InspectorLabelAttribute;
        label.text = labelAttribute.label;
        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
#endif
