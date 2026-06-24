using FlappyBird.Localization;
using FlappyBird.UI;
using FlappyBird.UI.Border;

namespace FlappyBird.Settings;

/// <summary>
/// Runtime settings singleton. Future: persist to JSON via Save()/Load().
/// Call <see cref="Apply"/> after changing properties to sync side-effects.
/// </summary>
public class GameSettings
{
    public static GameSettings Instance { get; private set; } = new();

    public Language     Language      { get; set; } = Language.English;
    public int          TargetFps     { get; set; } = 60;   // 60 or 120
    public bool         MusicEnabled  { get; set; } = true;
    public int          MusicVolume   { get; set; } = 80;   // 0-100 step 10
    public int          EffectsVolume { get; set; } = 60;   // 0-100 step 10
    public BorderStyle  BorderStyle   { get; set; } = BorderStyle.Double;
    public ConsoleColor PrimaryColor  { get; set; } = ConsoleColor.Cyan;
    public ConsoleColor AccentColor   { get; set; } = ConsoleColor.Yellow;

    /// <summary>Face name of the active console font. Empty = system default.</summary>
    public string FontFaceName { get; set; } = "";
    /// <summary>Console font cell height in pixels.</summary>
    public int    FontSize     { get; set; } = 16;

    /// <summary>Derived from colors+border — returns "Custom" when no preset matches.</summary>
    public string CurrentThemeName =>
        Array.Find(Themes.All, t =>
            t.Border  == BorderStyle &&
            t.Primary == PrimaryColor &&
            t.Accent  == AccentColor)?.Name ?? "Custom";

    public void Apply() { L.Current = Language; }

    public void ApplyTheme(Theme theme)
    {
        BorderStyle  = theme.Border;
        PrimaryColor = theme.Primary;
        AccentColor  = theme.Accent;
    }

    /// <summary>Resets all settings to defaults and applies them.</summary>
    public static void Reset()
    {
        Instance = new GameSettings();
        Instance.Apply();
    }

    /// <summary>Creates a UIPanel pre-configured with the current theme.</summary>
    public UIPanel CreatePanel(int width = 66) => new(width, BorderStyle)
    {
        BorderColor  = PrimaryColor,
        TitleColor   = AccentColor,
        ContentColor = ConsoleColor.White,
    };

    /// <summary>Returns the active BorderSet (use in existing rendering code).</summary>
    public BorderSet GetBorderSet() => BorderStyles.Get(BorderStyle);
}
