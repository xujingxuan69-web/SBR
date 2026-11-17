using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowEditorOnlyMetadataAttribute))]
public sealed class ShowEditorOnlyMetadataDrawer : PropertyDrawer
{
    private readonly struct MetadataField
    {
        public readonly string FieldName;
        public readonly GUIContent Label;
        public readonly bool IsTextArea;

        public MetadataField(string fieldName, GUIContent label, bool isTextArea)
        {
            FieldName = fieldName;
            Label = label;
            IsTextArea = isTextArea;
        }
    }

    private static readonly Dictionary<Type, MetadataField[]> MetadataFields = new();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        UnityEngine.Object referencedObject = property.hasMultipleDifferentValues
            ? null
            : property.objectReferenceValue;
        if (referencedObject == null)
            return height;

        using var serializedTarget = new SerializedObject(referencedObject);
        serializedTarget.UpdateIfRequiredOrScript();
        foreach (MetadataField metadataField in GetMetadataFields(referencedObject.GetType()))
        {
            SerializedProperty metadataProperty = serializedTarget.FindProperty(metadataField.FieldName);
            if (metadataProperty != null)
                height += GetMetadataHeight(metadataProperty, metadataField);
        }

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float propertyHeight = EditorGUIUtility.singleLineHeight;
        var propertyRect = new Rect(position.x, position.y, position.width, propertyHeight);
        Type objectType = fieldInfo != null ? fieldInfo.FieldType : typeof(UnityEngine.Object);
        bool previousMixedValue = EditorGUI.showMixedValue;
        EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
        EditorGUI.BeginChangeCheck();
        UnityEngine.Object newReference = EditorGUI.ObjectField(
            propertyRect, label, property.objectReferenceValue, objectType, false);
        if (EditorGUI.EndChangeCheck())
            property.objectReferenceValue = newReference;
        EditorGUI.showMixedValue = previousMixedValue;

        float y = position.y + propertyHeight + EditorGUIUtility.standardVerticalSpacing;
        UnityEngine.Object referencedObject = property.hasMultipleDifferentValues
            ? null
            : property.objectReferenceValue;
        if (referencedObject == null)
        {
            EditorGUI.EndProperty();
            return;
        }

        using var serializedTarget = new SerializedObject(referencedObject);
        serializedTarget.UpdateIfRequiredOrScript();
        foreach (MetadataField metadataField in GetMetadataFields(referencedObject.GetType()))
        {
            SerializedProperty metadataProperty = serializedTarget.FindProperty(metadataField.FieldName);
            if (metadataProperty == null)
                continue;

            float metadataHeight = GetMetadataHeight(metadataProperty, metadataField)
                - EditorGUIUtility.standardVerticalSpacing;
            var metadataRect = EditorGUI.IndentedRect(
                new Rect(position.x, y, position.width, metadataHeight));

            if (metadataField.IsTextArea)
            {
                var textAreaLabelRect = new Rect(
                    metadataRect.x,
                    metadataRect.y,
                    metadataRect.width,
                    EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(textAreaLabelRect, metadataField.Label);

                DrawUnappliedProperty(metadataRect, metadataProperty, GUIContent.none);
                y += GetMetadataHeight(metadataProperty, metadataField);
                continue;
            }

            float labelWidth = Mathf.Min(EditorGUIUtility.labelWidth, metadataRect.width * 0.45f);
            var labelRect = new Rect(metadataRect.x, metadataRect.y,
                labelWidth, EditorGUIUtility.singleLineHeight);
            var valueRect = new Rect(metadataRect.x + labelWidth, metadataRect.y,
                metadataRect.width - labelWidth, metadataRect.height);
            EditorGUI.LabelField(labelRect, metadataField.Label);
            DrawUnappliedProperty(valueRect, metadataProperty, GUIContent.none);
            y += GetMetadataHeight(metadataProperty, metadataField);
        }

        EditorGUI.EndProperty();
    }

    private static void DrawUnappliedProperty(
        Rect position, SerializedProperty property, GUIContent label)
    {
        bool guiChanged = GUI.changed;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.changed = guiChanged;
    }

    private static string GetDisplayName(FieldInfo field)
    {
        string name = field.Name;
        const string backingFieldSuffix = ">k__BackingField";
        if (name.StartsWith("<", StringComparison.Ordinal)
            && name.EndsWith(backingFieldSuffix, StringComparison.Ordinal))
        {
            name = name.Substring(1, name.Length - backingFieldSuffix.Length - 1);
        }

        return ObjectNames.NicifyVariableName(name);
    }

    private static MetadataField[] GetMetadataFields(Type type)
    {
        if (MetadataFields.TryGetValue(type, out MetadataField[] fields))
            return fields;

        var reflectedFields = new List<FieldInfo>();
        for (Type current = type; current != null && current != typeof(UnityEngine.Object); current = current.BaseType)
        {
            foreach (FieldInfo field in current.GetFields(
                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (field.GetCustomAttribute<EditorOnlyMetadataAttribute>() != null)
                    reflectedFields.Add(field);
            }
        }

        reflectedFields.Sort((left, right) => left.MetadataToken.CompareTo(right.MetadataToken));
        fields = new MetadataField[reflectedFields.Count];
        for (int i = 0; i < reflectedFields.Count; i++)
        {
            FieldInfo field = reflectedFields[i];
            TooltipAttribute tooltip = field.GetCustomAttribute<TooltipAttribute>();
            fields[i] = new MetadataField(
                field.Name,
                new GUIContent(GetDisplayName(field), tooltip != null ? tooltip.tooltip : string.Empty),
                field.GetCustomAttribute<TextAreaAttribute>() != null);
        }

        MetadataFields.Add(type, fields);
        return fields;
    }

    private static float GetMetadataHeight(SerializedProperty property, MetadataField metadataField)
    {
        if (metadataField.IsTextArea)
        {
            return InspectorReadOnlyFieldDrawer.GetReadOnlyPropertyHeight(
                    property, GUIContent.none)
                + EditorGUIUtility.standardVerticalSpacing;
        }

        return InspectorReadOnlyFieldDrawer.GetReadOnlyPropertyHeight(
            property, metadataField.Label)
            + EditorGUIUtility.standardVerticalSpacing;
    }
}
