using System;
using UnityEngine;

/// <summary>Starts a header section that can be merged with matching Header sections in base classes.</summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class MergeHeaderAttribute : PropertyAttribute
{
    public string HeaderName { get; }

    public MergeHeaderAttribute(string headerName)
    {
        HeaderName = headerName;
    }
}
