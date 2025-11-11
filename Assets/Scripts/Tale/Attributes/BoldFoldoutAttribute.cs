using System;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[AttributeUsage(AttributeTargets.Field)]
public class BoldFoldoutAttribute : PropertyAttribute { }

[CustomPropertyDrawer(typeof(BoldFoldoutAttribute))]
public class BoldFoldoutDrawer : PropertyDrawer {
    GUIStyle style = null;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        if (style == null) {
            style = new GUIStyle(EditorStyles.foldout);
            style.fontStyle = FontStyle.Bold;

            var color = EditorStyles.boldLabel.onFocused.textColor;
            style.normal.textColor = color;
            style.onNormal.textColor = color;
            style.active.textColor = color;
            style.onActive.textColor = color;
        }

        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded,
            label,
            true,
            style
        );

        if (property.isExpanded) {
            EditorGUI.indentLevel++;

            var yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            var start = property.Copy();
            var end = start.GetEndProperty();
            start.NextVisible(true);

            while (!SerializedProperty.EqualContents(start, end)) {
                var propertyHeight = EditorGUI.GetPropertyHeight(start, true);

                EditorGUI.PropertyField(
                    new Rect(position.x, position.y + yOffset, position.width, propertyHeight),
                    start,
                    true
                );

                yOffset += propertyHeight + EditorGUIUtility.standardVerticalSpacing;

                if (!start.NextVisible(false)) {
                    break;
                }
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        if (!property.isExpanded) {
            return EditorGUIUtility.singleLineHeight;
        }

        var totalHeight = EditorGUIUtility.singleLineHeight;

        var start = property.Copy();
        var end = start.GetEndProperty();
        start.NextVisible(true);

        while (!SerializedProperty.EqualContents(start, end)) {
            totalHeight += EditorGUI.GetPropertyHeight(start, true) + EditorGUIUtility.standardVerticalSpacing;

            if (!start.NextVisible(false)) {
                break;
            }
        }

        return totalHeight;
    }
}
#endif