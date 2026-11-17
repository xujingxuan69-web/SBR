#if UNITY_EDITOR
using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class ShowEditorOnlyMetadataAttribute : PropertyAttribute
{
}
#endif
