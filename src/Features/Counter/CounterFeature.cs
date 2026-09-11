using Godot;
using GodotTemplate.Core;

namespace GodotTemplate.Features.Counter;

/// <summary>
/// Displays a frame counter in the center of the screen. The counter
/// increments by 1 every frame.
///
/// Drop-in feature: this file alone is enough to add this behavior to the
/// project. The <see cref="Bootstrap"/> picks it up via reflection.
/// </summary>
[GodotFeature(Order = 100, Category = "ui")]
public partial class CounterFeature : Control
{
    private int _counter;
    private Label _label = null!;

    public override void _Ready()
    {
        // Build the UI programmatically — no .tscn involved.
        _label = new Label
        {
            Text = "0",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        _label.SetAnchorsPreset(LayoutPreset.Center);
        _label.AddThemeFontSizeOverride("font_size", 96);
        AddChild(_label);
    }

    public override void _Process(double delta)
    {
        _counter++;
        _label.Text = _counter.ToString();
    }
}
