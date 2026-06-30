namespace FlappyBird.Rendering;

/// <summary>
/// Source of truth for terminal dimensions and responsive scaling.
/// - Polls actual terminal size via Console.WindowWidth/Height
/// - Enforces constraints: min 24×78, max 24×80 (single player) or 24×80 (all modes)
/// - All renderers read dimensions from here instead of Console directly
/// </summary>
public static class ConsoleLayout
{
    // ── RESPONSIVE SIZE CONSTRAINTS ─────────────────────────────────────
    public const int MIN_HEIGHT = 24;        // game 20 + footer 4
    public const int MIN_WIDTH  = 78;        // single player game area
    public const int MAX_HEIGHT = 24;        // never exceed 24 rows
    public const int MAX_WIDTH  = 80;        // two player needs 80 (40+40), single 78

    // ── ACTUAL CONSTRAINED DIMENSIONS ───────────────────────────────────
    public static int W => Clamp(Console.WindowWidth,  MIN_WIDTH,  MAX_WIDTH);
    public static int H => Clamp(Console.WindowHeight, MIN_HEIGHT, MAX_HEIGHT);

    private static int _prevW = -1, _prevH = -1;
    private static int _prevRawW = -1, _prevRawH = -1;

    /// <summary>Returns true (once) when the terminal was resized since the last call.
    /// Tracks both clamped and raw dimensions so a resize from 85→80 cols is also detected.</summary>
    public static bool HasResized()
    {
        int w = W, h = H;
        int rawW = Console.WindowWidth, rawH = Console.WindowHeight;
        if (w == _prevW && h == _prevH && rawW == _prevRawW && rawH == _prevRawH) return false;
        _prevW = w; _prevH = h;
        _prevRawW = rawW; _prevRawH = rawH;
        return true;
    }

    /// <summary>Seed the snapshot without reporting a resize (call at game start).</summary>
    public static void Snapshot()
    {
        _prevW = W; _prevH = H;
        _prevRawW = Console.WindowWidth; _prevRawH = Console.WindowHeight;
    }

    /// <summary>Clamp value between min and max (inclusive).</summary>
    private static int Clamp(int value, int min, int max)
        => Math.Min(Math.Max(value, min), max);
}
