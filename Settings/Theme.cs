using FlappyBird.UI.Border;

namespace FlappyBird.Settings;

/// <summary>
/// A named visual theme: border style + three semantic colors.
/// Pass to <see cref="GameSettings.ApplyTheme"/> to activate.
/// </summary>
public sealed record Theme(
    string       Name,
    BorderStyle  Border,
    ConsoleColor Primary,
    ConsoleColor Accent,
    ConsoleColor Danger
);

/// <summary>Built-in themes. Add custom ones by constructing a Theme record.</summary>
public static class Themes
{
    public static readonly Theme Classic = new(
        "Classic", BorderStyle.Double,
        Primary: ConsoleColor.Cyan,
        Accent:  ConsoleColor.Yellow,
        Danger:  ConsoleColor.Red);

    public static readonly Theme Matrix = new(
        "Matrix", BorderStyle.Single,
        Primary: ConsoleColor.Green,
        Accent:  ConsoleColor.DarkGreen,
        Danger:  ConsoleColor.Red);

    public static readonly Theme Retro = new(
        "Retro", BorderStyle.ASCII,
        Primary: ConsoleColor.White,
        Accent:  ConsoleColor.Gray,
        Danger:  ConsoleColor.DarkRed);

    public static readonly Theme Neon = new(
        "Neon", BorderStyle.Rounded,
        Primary: ConsoleColor.Magenta,
        Accent:  ConsoleColor.Cyan,
        Danger:  ConsoleColor.Red);

    public static readonly Theme[] All = [Classic, Matrix, Retro, Neon];
}
