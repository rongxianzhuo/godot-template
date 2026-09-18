using Godot;
using System.Collections.Generic;

namespace GodotTemplate.Features.Match3;

/// <summary>
/// Bev's sprite path configuration. Maps algorithm <see cref="GemType"/> values to
/// Bev's PNG assets (and which import settings to use). Edit here when Bev ships
/// new art — no other file needs to change.
///
/// Import settings (per Bev, see docs/ASSET_INTEGRATION.md):
///   - Filter:  Linear (art has hard pixel edges that look better without smoothing)
///   - Mipmaps: Off (UI sprites don't need them; saves GPU memory)
///   - Compression: VRAM Compressed (ETC2 / ASTC on Android via Godot default)
///   - Alpha: Premultiplied for the *_alpha.png sprites (cleaner sprite edge blending)
/// </summary>
public static class SpritePaths
{
    // ---- gem sprites (Bev's 6 *_alpha.png variants are the alpha-blended ones we use) ----
    public const string GemRedAlpha    = "res://assets/gems/gem_red_alpha.png";
    public const string GemOrangeAlpha = "res://assets/gems/gem_orange_alpha.png";
    public const string GemYellowAlpha = "res://assets/gems/gem_yellow_alpha.png";
    public const string GemGreenAlpha  = "res://assets/gems/gem_green_alpha.png";
    public const string GemBlueAlpha   = "res://assets/gems/gem_blue_alpha.png";
    public const string GemPurpleAlpha = "res://assets/gems/gem_purple_alpha.png";

    public static readonly IReadOnlyDictionary<GemType, string> GemTypeToSprite =
        new Dictionary<GemType, string>
        {
            { GemType.Red,    GemRedAlpha    },
            { GemType.Orange, GemOrangeAlpha },
            { GemType.Yellow, GemYellowAlpha },
            { GemType.Green,  GemGreenAlpha  },
            { GemType.Blue,   GemBlueAlpha   },
            { GemType.Purple, GemPurpleAlpha },
        };

    // ---- UI / backgrounds ----
    public const string ButtonNormal     = "res://assets/ui/button_normal.png";
    public const string ButtonPressed    = "res://assets/ui/button_pressed.png";
    public const string BackgroundTitle  = "res://assets/backgrounds/bg_title.png";
    public const string BackgroundGame   = "res://assets/backgrounds/bg_game.png";

    /// <summary> Load a gem sprite for a color. Returns null if the gem is empty. </summary>
    public static Texture2D? LoadGem(GemType type)
    {
        if (type == GemType.None) return null;
        return GD.Load<Texture2D>(GemTypeToSprite[type]);
    }

    public static Texture2D LoadButton(bool pressed = false) =>
        GD.Load<Texture2D>(pressed ? ButtonPressed : ButtonNormal);

    public static Texture2D LoadBackground(string scene) =>
        GD.Load<Texture2D>(scene == "title" ? BackgroundTitle : BackgroundGame);
}