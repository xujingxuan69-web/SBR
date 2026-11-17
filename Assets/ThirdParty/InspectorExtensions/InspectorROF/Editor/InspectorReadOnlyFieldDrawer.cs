using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(InspectorReadOnlyFieldAttribute))]
public sealed class InspectorReadOnlyFieldDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return GetReadOnlyPropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DrawReadOnlyField(position, property, label);
    }

    public static float GetReadOnlyPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public static void DrawReadOnlyField(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedObject sourceObject = property.serializedObject;
        SerializedObject readOnlyObject;
        if (sourceObject.isEditingMultipleObjects)
        {
            Object[] targets = sourceObject.targetObjects;
            if (targets == null || targets.Length == 0)
                return;

            readOnlyObject = new SerializedObject(targets);
        }
        else
        {
            Object target = sourceObject.targetObject;
            if (target == null)
                return;

            readOnlyObject = new SerializedObject(target);
        }

        using (readOnlyObject)
        {
            readOnlyObject.UpdateIfRequiredOrScript();
            SerializedProperty readOnlyProperty = readOnlyObject.FindProperty(property.propertyPath);
            if (readOnlyProperty == null)
                return;

            bool guiChanged = GUI.changed;
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, readOnlyProperty, label, true);
            EditorGUI.EndProperty();
            GUI.changed = guiChanged;
        }
    }
}
