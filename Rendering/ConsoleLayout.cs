namespace FlappyBird.Rendering;

/// <summary>
/// Source of truth for terminal dimensions. Poll HasResized() each frame;
/// all renderers read W/H from here instead of hard-coding 80×24.
/// </summary>
public static class ConsoleLayout
{
    public static int W => Console.WindowWidth;
    public static int H => Console.WindowHeight;

    private static int _prevW = -1, _prevH = -1;

    /// <summary>Returns true (once) when the terminal was resized since the last call.</summary>
    public static bool HasResized()
    {
        int w = W, h = H;
        if (w == _prevW && h == _prevH) return false;
        _prevW = w; _prevH = h;
        return true;
    }

    /// <summary>Seed the snapshot without reporting a resize (call at game start).</summary>
    public static void Snapshot() { _prevW = W; _prevH = H; }
}
