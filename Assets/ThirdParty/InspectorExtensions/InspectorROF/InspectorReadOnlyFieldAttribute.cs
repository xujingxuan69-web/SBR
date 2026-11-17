using System;
using System.Diagnostics;
using UnityEngine;

/// <summary>Displays a serialized field in the Inspector without allowing edits.</summary>
[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class InspectorReadOnlyFieldAttribute : PropertyAttribute
{
}

