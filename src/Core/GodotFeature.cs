using System;

namespace GodotTemplate.Core;

/// <summary>
/// Marks a class as a discoverable Godot feature. The <see cref="Bootstrap"/>
/// node will scan the assembly for classes decorated with this attribute,
/// instantiate them, and add them as children of the bootstrap root.
///
/// Use named-argument syntax in attribute brackets:
///   [GodotFeature(Order = 100, Category = "ui")]
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class GodotFeatureAttribute : Attribute
{
    /// <summary>Loading order. Lower numbers load first.</summary>
    public int Order { get; set; }

    /// <summary>Optional category tag for grouping/logging.</summary>
    public string Category { get; set; } = "default";
}
