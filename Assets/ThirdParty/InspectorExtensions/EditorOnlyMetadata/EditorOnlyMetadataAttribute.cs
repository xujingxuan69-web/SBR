#if UNITY_EDITOR
using System;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class EditorOnlyMetadataAttribute : Attribute
{
}
#endif
