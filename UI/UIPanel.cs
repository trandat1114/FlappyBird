using FlappyBird.UI.Border;

namespace FlappyBird.UI;

/// <summary>
/// Flexible UI panel engine.
/// - Build* methods return strings for use with Console.WriteLine (sequential rendering).
/// - Print* methods write directly to console at the current cursor position.
/// - WriteRowAt() supports in-place updates via SetCursorPosition.
/// All methods guarantee output exactly <see cref="Width"/> characters wide —
/// content is auto-padded or truncated to <see cref="InnerWidth"/>.
/// </summary>
public class UIPanel
{
    public int Width { get; }
    public int InnerWidth => Width - 2;

    public BorderSet Borders { get; set; }
    public ConsoleColor BorderColor  { get; set; } = ConsoleColor.White;
    public ConsoleColor ContentColor { get; set; } = ConsoleColor.White;
    public ConsoleColor TitleColor   { get; set; } = ConsoleColor.Cyan;

    public UIPanel(int width, BorderStyle style = BorderStyle.Double)
    {
        Width   = width;
        Borders = BorderStyles.Get(style);
    }

    // ── String builders (use with Console.WriteLine) ──────────────────────

    public string BuildTop()    => $"{Borders.TopLeft}{new string(Borders.Horiz, InnerWidth)}{Borders.TopRight}";
    public string BuildBottom() => $"{Borders.BotLeft}{new string(Borders.Horiz, InnerWidth)}{Borders.BotRight}";
    public string BuildSep()    => $"{Borders.TeeRight}{new string(Borders.Horiz, InnerWidth)}{Borders.TeeLeft}";

    public string BuildRow(string content)
    {
        string inner = content.Length > InnerWidth ? content[..InnerWidth] : content.PadRight(InnerWidth);
        return $"{Borders.Vert}{inner}{Borders.Vert}";
    }

    public string BuildTitleRow(string text)
        => BuildRow(text.PadLeft((InnerWidth + text.Length) / 2).PadRight(InnerWidth));

    public string BuildEmptyRow() => BuildRow(string.Empty);

    // ── Sequential print ─────────────────────────────────────────────────

    public void PrintTop(ConsoleColor? c = null)
    {
        Console.ForegroundColor = c ?? BorderColor;
        Console.WriteLine(BuildTop());
        Console.ResetColor();
    }

    public void PrintBottom(ConsoleColor? c = null)
    {
        Console.ForegroundColor = c ?? BorderColor;
        Console.WriteLine(BuildBottom());
        Console.ResetColor();
    }

    public void PrintSep(ConsoleColor? c = null)
    {
        Console.ForegroundColor = c ?? BorderColor;
        Console.WriteLine(BuildSep());
        Console.ResetColor();
    }

    public void PrintTitle(string text, ConsoleColor? fg = null, ConsoleColor? border = null)
    {
        string centered = text.PadLeft((InnerWidth + text.Length) / 2).PadRight(InnerWidth);
        PrintRow(centered, fg ?? TitleColor, border);
    }

    /// <param name="contentBg">Optional background for the inner content area only (borders stay on default bg).</param>
    public void PrintRow(string content, ConsoleColor? fg = null, ConsoleColor? border = null, ConsoleColor? contentBg = null)
    {
        var bc    = border ?? BorderColor;
        string inner = content.Length > InnerWidth ? content[..InnerWidth] : content.PadRight(InnerWidth);
        Console.ForegroundColor = bc;
        Console.Write(Borders.Vert);
        if (contentBg.HasValue) Console.BackgroundColor = contentBg.Value;
        Console.ForegroundColor = fg ?? ContentColor;
        Console.Write(inner);
        Console.ResetColor();
        Console.ForegroundColor = bc;
        Console.WriteLine(Borders.Vert);
        Console.ResetColor();
    }

    public void PrintEmpty() => PrintRow(string.Empty);

    // ── In-place update ──────────────────────────────────────────────────

    public void WriteRowAt(int x, int y, string content, ConsoleColor? fg = null, ConsoleColor? border = null)
    {
        Console.SetCursorPosition(x, y);
        var bc    = border ?? BorderColor;
        string inner = content.Length > InnerWidth ? content[..InnerWidth] : content.PadRight(InnerWidth);
        Console.ForegroundColor = bc;
        Console.Write(Borders.Vert);
        Console.ForegroundColor = fg ?? ContentColor;
        Console.Write(inner);
        Console.ForegroundColor = bc;
        Console.Write(Borders.Vert);
        Console.ResetColor();
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    /// <summary>Centers <paramref name="text"/> in InnerWidth chars.</summary>
    public string Center(string text)
        => text.PadLeft((InnerWidth + text.Length) / 2).PadRight(InnerWidth);

    /// <summary>Left-aligns <paramref name="text"/>, padded to InnerWidth.</summary>
    public string Pad(string text) => text.PadRight(InnerWidth);

    public static UIPanel Default(int width = 66) => new(width);
}
