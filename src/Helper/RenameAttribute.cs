using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class RenameAttribute : PropertyAttribute
{
    public string name;

    public RenameAttribute(string name) =>
        this.name = name;
}

[CustomPropertyDrawer(typeof(RenameAttribute))]
public class RenameEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) =>
        EditorGUI.PropertyField(position, property, new GUIContent((attribute as RenameAttribute).name));
}
#endif