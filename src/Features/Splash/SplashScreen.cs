using Godot;

namespace GodotTemplate.Features.Splash;

/// <summary>
/// Splash screen — shown for <see cref="SplashDurationSec"/> at app launch,
/// then transitions to <c>res://src/Main.tscn</c> (which loads Bootstrap →
/// Match3Feature → ScreenManager → TitleScreen).
///
/// Attached to <c>assets/scenes/SplashScreen.tscn</c> root (NOT a
/// <c>[GodotFeature]</c>). Splash is independent of game logic — just a
/// brief loading animation while the engine finishes loading assets for
/// Main.tscn. Per Mark D+17 decision, this is the canonical Godot pattern
/// (Custom Splash Screen Demo): SplashScreen.tscn IS <c>run/main_scene</c>,
/// and <c>_Ready</c> calls <c>ChangeSceneToFile("res://src/Main.tscn")</c>
/// after a timeout.
///
/// v1.1 polish backlog #2 — B-v3 variant per Christine's variants doc §6:
///   - Icon (240, 140) 240×240 cosmic purple
///   - Title 52px y=400 #FFD955 + white outline (AAA contrast 7:1)
///   - Separator (260, 470) 200×2 #FFD955
///   - Subtitle 18px y=490 #FFFFFF "60 Levels · 3 Themes" (AAA contrast 6:1)
///   - LoadingDots y=1180, 5 dots 8×8 white, 24px spacing
///
/// Animation (per build spec §3.3): 5-dot alpha-pulse loop
/// (0.6s down to 0.3 + 0.6s up to 1.0, 0.1s stagger between dots,
/// SineInOut easing). Loops indefinitely until splash transition.
///
/// v1.1 trade-off: timing-based transition (not a real "loading complete"
/// signal). Acceptable for v1.1 because there's no real async work —
/// the engine has already loaded assets by the time this _Ready fires;
/// the 2s just gives the player a moment to see the splash.
/// Future v2.0: replace timer with actual async asset load completion.
/// </summary>
public partial class SplashScreen : Control
{
    /// <summary>Wall-clock seconds before splash → Main.tscn transition.</summary>
    private const float SplashDurationSec = 2.0f;

    // Reference the 5 dots from the .tscn (LoadingDots/Dot1 .. Dot5).
    private ColorRect[] _dots = null!;

    public override void _Ready()
    {
        GD.Print("[SplashScreen] _Ready — starting B-v3 splash");

        // Resolve the 5 dot ColorRects from the HBoxContainer.
        // The .tscn guarantees exactly 5 children (Dot1..Dot5) per build spec §3.1.
        var loadingDots = GetNode<HBoxContainer>("LoadingDots");
        int childCount = loadingDots.GetChildCount();
        if (childCount != 5)
        {
            GD.PushWarning($"[SplashScreen] expected 5 dots, got {childCount} — animation may be off");
        }
        _dots = new ColorRect[childCount];
        for (int i = 0; i < childCount; i++)
        {
            _dots[i] = loadingDots.GetChild<ColorRect>(i);
        }

        // Start the looping alpha-pulse animation on each dot, staggered.
        StartDotAnimation();

        // Schedule the scene transition after SplashDurationSec.
        // SceneTreeTimer fires Timeout once; engine handles disposal.
        GetTree().CreateTimer(SplashDurationSec).Timeout += OnSplashComplete;
    }

    /// <summary>
    /// Per build spec §3.3: each dot pulses alpha 0.3 → 1.0 → 0.3 over
    /// 0.6s+0.6s with 0.1s stagger between dots (chasing-dot effect),
    /// SineInOut easing, loops indefinitely until OnSplashComplete
    /// changes the scene (which frees this node + cancels tweens).
    /// </summary>
    private void StartDotAnimation()
    {
        for (int i = 0; i < _dots.Length; i++)
        {
            var dot = _dots[i];
            var tween = CreateTween();
            tween.SetLoops(); // permanent loop until node freed
            tween.SetTrans(Tween.TransitionType.Sine);
            tween.SetEase(Tween.EaseType.InOut);
            tween.TweenInterval(i * 0.1);  // stagger start time per dot
            tween.TweenProperty(dot, "modulate:a", 0.3f, 0.6);
            tween.TweenProperty(dot, "modulate:a", 1.0f, 0.6);
        }
    }

    private void OnSplashComplete()
    {
        GD.Print($"[SplashScreen] {SplashDurationSec}s elapsed → switching to Main.tscn");
        // ChangeSceneToFile replaces the current scene; this node + tweens
        // are freed automatically. No manual cleanup needed.
        GetTree().ChangeSceneToFile("res://src/Main.tscn");
    }
}
