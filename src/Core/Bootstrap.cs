using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace GodotTemplate.Core;

/// <summary>
/// Root scene script. Scans the loaded assemblies for classes marked with
/// <see cref="GodotFeatureAttribute"/>, instantiates them, and adds them
/// as children of this node.
///
/// Agents should never need to edit this file. To add a new feature, drop a
/// new .cs file under <c>src/Features/&lt;FeatureName&gt;/</c> decorated with
/// <c>[GodotFeature]</c> and it will be auto-loaded.
/// </summary>
public partial class Bootstrap : Node
{
    public override void _Ready()
    {
        var featureTypes = DiscoverFeatureTypes();
        GD.Print($"[Bootstrap] Discovered {featureTypes.Count} feature(s):");

        foreach (var type in featureTypes)
        {
            try
            {
                if (Activator.CreateInstance(type) is not Node instance)
                {
                    GD.PushError($"[Bootstrap] {type.Name} is not a Node, skipped.");
                    continue;
                }
                instance.Name = type.Name;
                AddChild(instance);
                GD.Print($"[Bootstrap]   + {type.Name} (order={GetOrder(type)})");
            }
            catch (Exception e)
            {
                GD.PushError($"[Bootstrap] Failed to instantiate {type.Name}: {e.Message}");
            }
        }
    }

    private static List<Type> DiscoverFeatureTypes()
    {
        var results = new List<Type>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in SafeGetTypes(assembly))
            {
                var attr = type.GetCustomAttribute<GodotFeatureAttribute>();
                if (attr is null) continue;
                if (type.IsAbstract || type.IsGenericTypeDefinition) continue;
                results.Add(type);
            }
        }
        return results
            .OrderBy(t => GetOrder(t))
            .ThenBy(t => t.FullName, StringComparer.Ordinal)
            .ToList();
    }

    private static int GetOrder(Type t) =>
        t.GetCustomAttribute<GodotFeatureAttribute>()?.Order ?? 0;

    private static Type[] SafeGetTypes(Assembly asm)
    {
        try
        {
            return asm.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null).ToArray()!;
        }
    }
}
