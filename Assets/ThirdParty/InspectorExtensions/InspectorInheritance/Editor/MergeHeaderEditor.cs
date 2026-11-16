using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Emo.InspectorInheritance.Editor
{
    /// <summary>
    /// Fallback inspector for MonoBehaviours. Components without MergeHeader use Unity's default drawing.
    /// More specific CustomEditors take precedence.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true)]
    internal sealed class MergeHeaderEditor : UnityEditor.Editor
    {
        private sealed class Section
        {
            public string Name;
            public bool IsMergeSection;
            public bool HasUnityHeader;
            public readonly List<string> PropertyNames = new List<string>();
        }

        private sealed class Layout
        {
            public bool HasMergeHeader;
            public readonly List<Section> Sections = new List<Section>();
            public readonly List<DisplayGroup> DisplayGroups = new List<DisplayGroup>();
        }

        private sealed class DisplayGroup
        {
            public string Name;
            public bool IsMerged;
            public bool HasUnityHeader;
            public readonly List<Section> Sections = new List<Section>();
        }

        private static readonly Dictionary<Type, Layout> Layouts = new Dictionary<Type, Layout>();

        [InitializeOnLoadMethod]
        private static void WarmLayoutsAfterScriptReload()
        {
            Layouts.Clear();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<MonoBehaviour>())
                GetLayout(type);
        }

        public override void OnInspectorGUI()
        {
            Layout layout = GetLayout(target.GetType());
            if (!layout.HasMergeHeader)
            {
                DrawDefaultInspector();
                return;
            }

            serializedObject.Update();
            SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");
            if (scriptProperty != null)
            {
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.PropertyField(scriptProperty);
            }
            DrawMergedLayout(layout);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawMergedLayout(Layout layout)
        {
            var drawnProperties = new HashSet<string>(StringComparer.Ordinal);
            foreach (DisplayGroup group in layout.DisplayGroups)
            {
                if (group.IsMerged && !string.IsNullOrEmpty(group.Name) && !group.HasUnityHeader)
                {
                    EditorGUILayout.Space(3f);
                    EditorGUILayout.LabelField(group.Name, EditorStyles.boldLabel);
                }

                foreach (Section candidate in group.Sections)
                    DrawSectionProperties(candidate, drawnProperties);
            }

            // Preserve serialized fields not recognized by reflection (for example inherited/private edge cases).
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.depth != 0 || iterator.propertyPath == "m_Script" || drawnProperties.Contains(iterator.propertyPath))
                    continue;

                EditorGUILayout.PropertyField(iterator, true);
            }
        }

        private void DrawSectionProperties(Section section, HashSet<string> drawnProperties)
        {
            foreach (string propertyName in section.PropertyNames)
            {
                if (!drawnProperties.Add(propertyName))
                    continue;

                SerializedProperty property = serializedObject.FindProperty(propertyName);
                if (property != null && property.propertyPath != "m_Script")
                    EditorGUILayout.PropertyField(property, true);
            }
        }

        private static Layout GetLayout(Type type)
        {
            if (Layouts.TryGetValue(type, out Layout cached))
                return cached;

            Layout layout = BuildLayout(type);
            Layouts[type] = layout;
            return layout;
        }

        private static Layout BuildLayout(Type type)
        {
            var layout = new Layout();
            var hierarchy = new Stack<Type>();
            for (Type current = type; current != null && current != typeof(MonoBehaviour); current = current.BaseType)
                hierarchy.Push(current);

            foreach (Type current in hierarchy)
            {
                Section activeSection = null;
                FieldInfo[] fields = current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                Array.Sort(fields, (a, b) => a.MetadataToken.CompareTo(b.MetadataToken));

                foreach (FieldInfo field in fields)
                {
                    if (!IsSerializedField(field))
                        continue;

                    HeaderAttribute header = field.GetCustomAttribute<HeaderAttribute>();
                    global::MergeHeaderAttribute mergeHeader = field.GetCustomAttribute<global::MergeHeaderAttribute>();
                    if (header != null || mergeHeader != null)
                    {
                        string name = mergeHeader != null ? mergeHeader.HeaderName : header.header;
                        activeSection = new Section
                        {
                            Name = name,
                            IsMergeSection = mergeHeader != null,
                            HasUnityHeader = header != null
                        };
                        layout.Sections.Add(activeSection);
                        layout.HasMergeHeader |= mergeHeader != null;
                    }

                    if (activeSection != null)
                        activeSection.PropertyNames.Add(field.Name);
                    else
                        layout.Sections.Add(new Section { Name = string.Empty, PropertyNames = { field.Name } });
                }
            }

            BuildDisplayGroups(layout);
            return layout;
        }

        private static void BuildDisplayGroups(Layout layout)
        {
            var mergedNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (Section section in layout.Sections)
            {
                if (section.IsMergeSection)
                    mergedNames.Add(section.Name);
            }

            var addedMergedNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (Section section in layout.Sections)
            {
                bool isMerged = mergedNames.Contains(section.Name);
                if (isMerged && !addedMergedNames.Add(section.Name))
                    continue;

                var group = new DisplayGroup { Name = section.Name, IsMerged = isMerged };
                foreach (Section candidate in layout.Sections)
                {
                    if (isMerged && !StringComparer.Ordinal.Equals(candidate.Name, section.Name))
                        continue;
                    if (!isMerged && !ReferenceEquals(candidate, section))
                        continue;

                    group.Sections.Add(candidate);
                    group.HasUnityHeader |= candidate.HasUnityHeader;
                }

                layout.DisplayGroups.Add(group);
            }
        }

        private static bool IsSerializedField(FieldInfo field)
        {
            if (field.IsStatic || field.IsLiteral || field.IsInitOnly || field.IsNotSerialized
                || field.IsDefined(typeof(HideInInspector), false))
                return false;

            return field.IsPublic
                   || field.IsDefined(typeof(SerializeField), false)
                   || field.IsDefined(typeof(SerializeReference), false);
        }
    }
}
