using FlappyBird.Audio;
using FlappyBird.Audio.Song;
using FlappyBird.Localization;
using FlappyBird.Settings;
using FlappyBird.UI.Border;

namespace FlappyBird.UI;

/// <summary>
/// Three-screen settings UI:
///   Main  → Language / Audio ▶ / Theme ▶
///   Audio → Music On/Off · Music Volume · Effects Volume
///   Theme → Preset · Primary Color · Accent Color · Border Style
/// Every Draw*() clears the console before painting to avoid stale content.
/// </summary>
public static class SettingsMenu
{
    // ── Color palette for customization ──────────────────────────────────────
    private static readonly (ConsoleColor Color, string Name)[] Palette =
    [
        (ConsoleColor.Cyan,        "Cyan"),
        (ConsoleColor.White,       "White"),
        (ConsoleColor.Green,       "Green"),
        (ConsoleColor.Magenta,     "Magenta"),
        (ConsoleColor.Yellow,      "Yellow"),
        (ConsoleColor.Red,         "Red"),
        (ConsoleColor.Blue,        "Blue"),
        (ConsoleColor.Gray,        "Gray"),
        (ConsoleColor.DarkCyan,    "Dark Cyan"),
        (ConsoleColor.DarkGreen,   "Dark Green"),
        (ConsoleColor.DarkMagenta, "Dark Magenta"),
    ];

    // ── Main screen ───────────────────────────────────────────────────────────
    private static int  _mainIdx;
    private const  int  MAIN_COUNT = 3;

    public static void Show()
    {
        Console.CursorVisible = false;
        _mainIdx = 0;
        DrawMain();

        while (true)
        {
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.UpArrow:
                    _mainIdx = (_mainIdx - 1 + MAIN_COUNT) % MAIN_COUNT;
                    DrawMain(); break;
                case ConsoleKey.DownArrow:
                    _mainIdx = (_mainIdx + 1) % MAIN_COUNT;
                    DrawMain(); break;
                case ConsoleKey.Enter:
                case ConsoleKey.Spacebar:
                    DoMain(_mainIdx); break;
                case ConsoleKey.R:
                    GameSettings.Reset(); DrawMain(); break;
                case ConsoleKey.Escape:
                    return;
            }
        }
    }

    private static void DoMain(int idx)
    {
        var s = GameSettings.Instance;
        switch (idx)
        {
            case 0:
                s.Language = s.Language == Language.English
                    ? Language.Vietnamese
                    : Language.English;
                s.Apply();
                DrawMain();
                break;
            case 1:
                ShowAudio();
                DrawMain();
                break;
            case 2:
                ShowTheme();
                DrawMain();
                break;
        }
    }

    private static void DrawMain()
    {
        Console.Clear();
        var s     = GameSettings.Instance;
        var panel = s.CreatePanel(66);

        Console.SetCursorPosition(0, 0);
        panel.PrintTop();
        panel.PrintTitle(L.Get(L.SETTINGS_TITLE));
        panel.PrintSep();

        // Language
        string langVal = s.Language == Language.English
            ? "[English] / Tiếng Việt"
            : "English / [Tiếng Việt]";
        PrintMainRow(panel, 0, $"{L.Get(L.SETTINGS_LANGUAGE),-17}: {langVal}");

        // Audio summary
        string audioSummary = $"Music: {(s.MusicEnabled ? "On" : "Off")} {s.MusicVolume,3}% | FX: {s.EffectsVolume}%  ▶";
        PrintMainRow(panel, 1, $"{"Audio",-17}  {audioSummary}");

        // Theme summary
        PrintMainRow(panel, 2, $"{"Theme",-17}  {s.CurrentThemeName}  ▶");

        panel.PrintSep();
        panel.PrintRow($"  {L.Get(L.SETTINGS_HINT)}", ConsoleColor.Gray);
        panel.PrintBottom();
    }

    private static void PrintMainRow(UIPanel panel, int idx, string content)
    {
        bool   sel    = idx == _mainIdx;
        string prefix = sel ? "► " : "  ";
        ConsoleColor fg = sel ? ConsoleColor.Yellow : ConsoleColor.White;
        panel.PrintRow($"{prefix}{content}", fg);
    }

    // ── Audio sub-screen ──────────────────────────────────────────────────────
    private static int _audioIdx;
    private const  int AUDIO_COUNT = 3;

    private static void ShowAudio()
    {
        _audioIdx = 0;
        DrawAudio();

        while (true)
        {
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.UpArrow:
                    _audioIdx = (_audioIdx - 1 + AUDIO_COUNT) % AUDIO_COUNT;
                    DrawAudio(); break;
                case ConsoleKey.DownArrow:
                    _audioIdx = (_audioIdx + 1) % AUDIO_COUNT;
                    DrawAudio(); break;
                case ConsoleKey.LeftArrow:
                    StepAudio(-1); DrawAudio(); break;
                case ConsoleKey.RightArrow:
                    StepAudio(+1); DrawAudio(); break;
                case ConsoleKey.Enter:
                case ConsoleKey.Spacebar:
                    StepAudio(+1); DrawAudio(); break;
                case ConsoleKey.Escape:
                    return;
            }
        }
    }

    private static void StepAudio(int d)
    {
        var s = GameSettings.Instance;
        switch (_audioIdx)
        {
            case 0:
                s.MusicEnabled = !s.MusicEnabled;
                if (s.MusicEnabled && s.MusicVolume > 0)
                    AudioManager.StartBackgroundMusic(HarryPotter.Melody);
                else
                    AudioManager.StopBackgroundMusic();
                break;
            case 1:
                s.MusicVolume = Math.Clamp(RoundStep(s.MusicVolume, d), 0, 100);
                if (s.MusicEnabled)
                {
                    if (s.MusicVolume == 0) AudioManager.StopBackgroundMusic();
                    else if (!AudioManager._isPlaying)
                        AudioManager.StartBackgroundMusic(HarryPotter.Melody);
                }
                break;
            case 2:
                s.EffectsVolume = Math.Clamp(RoundStep(s.EffectsVolume, d), 0, 100);
                break;
        }
    }

    private static int RoundStep(int value, int delta)
        => ((value / 10) + delta) * 10;

    private static void DrawAudio()
    {
        Console.Clear();
        var s     = GameSettings.Instance;
        var panel = s.CreatePanel(66);

        Console.SetCursorPosition(0, 0);
        panel.PrintTop();
        panel.PrintTitle("AUDIO");
        panel.PrintSep();

        string musicToggle = s.MusicEnabled ? "[On] / Off" : " On / [Off]";
        PrintAudioRow(panel, 0, $"Background Music  : {musicToggle}");
        PrintAudioRow(panel, 1, $"Music Volume      : {VolBar(s.MusicVolume)}");
        PrintAudioRow(panel, 2, $"Effects Volume    : {VolBar(s.EffectsVolume)}");

        panel.PrintSep();
        panel.PrintRow("  ↑↓: Select   ←→ / Enter: Adjust   ESC: Back", ConsoleColor.Gray);
        panel.PrintBottom();
    }

    private static void PrintAudioRow(UIPanel panel, int idx, string content)
    {
        bool   sel    = idx == _audioIdx;
        string prefix = sel ? "► " : "  ";
        ConsoleColor fg = sel ? ConsoleColor.Yellow : ConsoleColor.White;
        panel.PrintRow($"  {prefix}{content}", fg);
    }

    private static string VolBar(int vol)
    {
        int filled = vol / 10;
        return $"[{new string('█', filled)}{new string('░', 10 - filled)}] {vol,3}%";
    }

    // ── Theme sub-screen ──────────────────────────────────────────────────────
    private static int _themeIdx;
    private const  int THEME_COUNT = 4;

    private static void ShowTheme()
    {
        _themeIdx = 0;
        DrawTheme();

        while (true)
        {
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.UpArrow:
                    _themeIdx = (_themeIdx - 1 + THEME_COUNT) % THEME_COUNT;
                    DrawTheme(); break;
                case ConsoleKey.DownArrow:
                    _themeIdx = (_themeIdx + 1) % THEME_COUNT;
                    DrawTheme(); break;
                case ConsoleKey.LeftArrow:
                    StepTheme(-1); DrawTheme(); break;
                case ConsoleKey.RightArrow:
                    StepTheme(+1); DrawTheme(); break;
                case ConsoleKey.Enter:
                case ConsoleKey.Spacebar:
                    StepTheme(+1); DrawTheme(); break;
                case ConsoleKey.Escape:
                    return;
            }
        }
    }

    private static void StepTheme(int d)
    {
        var s = GameSettings.Instance;
        switch (_themeIdx)
        {
            case 0: // Preset
                int cur = Array.FindIndex(Themes.All, t => t.Name == s.CurrentThemeName);
                cur = cur < 0 ? (d > 0 ? 0 : Themes.All.Length - 1)
                              : (cur + d + Themes.All.Length) % Themes.All.Length;
                s.ApplyTheme(Themes.All[cur]);
                break;
            case 1: // Primary color
                s.PrimaryColor = CyclePalette(s.PrimaryColor, d);
                break;
            case 2: // Accent color
                s.AccentColor = CyclePalette(s.AccentColor, d);
                break;
            case 3: // Border style
                s.BorderStyle = (BorderStyle)(((int)s.BorderStyle + d + 4) % 4);
                break;
        }
    }

    private static ConsoleColor CyclePalette(ConsoleColor current, int delta)
    {
        int i = Array.FindIndex(Palette, p => p.Color == current);
        return Palette[(i < 0 ? 0 : i + delta + Palette.Length) % Palette.Length].Color;
    }

    private static string PaletteName(ConsoleColor c)
    {
        var p = Array.Find(Palette, p => p.Color == c);
        return p.Name ?? c.ToString();
    }

    private static void DrawTheme()
    {
        Console.Clear();
        var s     = GameSettings.Instance;
        var panel = s.CreatePanel(66);
        var bs    = s.GetBorderSet();

        Console.SetCursorPosition(0, 0);
        panel.PrintTop();
        panel.PrintTitle("THEME");
        panel.PrintSep();

        // Preset row
        string themeName = s.CurrentThemeName;
        string presetStr = string.Join(" / ", Themes.All.Select(t =>
            t.Name == themeName ? $"[{t.Name}]" : t.Name));
        if (themeName == "Custom") presetStr += " / [Custom]";
        PrintThemeRow(panel, 0, $"Preset         : {presetStr}");

        // Primary / Accent color rows — written manually to support inline color preview
        WriteColorRow(bs, 1, "Primary Color", s.PrimaryColor, s.PrimaryColor);
        WriteColorRow(bs, 2, "Accent Color ", s.AccentColor,  s.PrimaryColor);

        // Border style row
        string borderStr = string.Join(" / ",
            System.Enum.GetValues<BorderStyle>().Select(b =>
                b == s.BorderStyle ? $"[{b}]" : b.ToString()));
        PrintThemeRow(panel, 3, $"Border Style   : {borderStr}");

        panel.PrintSep();
        panel.PrintRow("  ↑↓: Select   ←→ / Enter: Cycle   ESC: Back", ConsoleColor.Gray);
        panel.PrintBottom();
    }

    private static void PrintThemeRow(UIPanel panel, int idx, string content)
    {
        bool   sel    = idx == _themeIdx;
        string prefix = sel ? "► " : "  ";
        ConsoleColor fg = sel ? ConsoleColor.Yellow : ConsoleColor.White;
        panel.PrintRow($"  {prefix}{content}", fg);
    }

    /// <summary>
    /// Writes a single border row with a colored ■ preview for a color value.
    /// Inner width = 64: 4 (indent+indicator) + 13 (label) + 3 (" : ") + 2 ("■ ") + 42 (name pad) = 64.
    /// </summary>
    private static void WriteColorRow(BorderSet bs, int idx, string label, ConsoleColor previewColor, ConsoleColor borderColor)
    {
        bool   sel       = idx == _themeIdx;
        string indicator = sel ? "► " : "  ";
        ConsoleColor textFg = sel ? ConsoleColor.Yellow : ConsoleColor.White;

        Console.ForegroundColor = borderColor;
        Console.Write(bs.Vert);                                   // 1

        Console.ForegroundColor = textFg;
        string lead = $"    {indicator}{label.PadRight(13)} : "; // 4+2+13+3 = 22
        Console.Write(lead);

        Console.ForegroundColor = previewColor;
        Console.Write("■ ");                                  // ■ + space = 2

        Console.ForegroundColor = textFg;
        Console.Write(PaletteName(previewColor).PadRight(40));    // 40

        Console.ForegroundColor = borderColor;
        Console.WriteLine(bs.Vert);                               // 1
        Console.ResetColor();
    }
}
