namespace FlappyBird.Models;

/// <summary>
/// Difficulty progression state — extracted from GameState (Phase 3).
/// </summary>
public class DifficultyState
{
    public int  Level        { get; set; } = 1;
    public int  PipeSpeed    { get; set; } = 3;
    // When true (Manual mode), Update() is a no-op — difficulty stays fixed.
    public bool Fixed        { get; set; } = false;
    // When set (Manual mode), overrides the computed gap.
    public int? FixedGapSize { get; set; } = null;

    public void Reset()
    {
        Level        = 1;
        PipeSpeed    = 3;
        Fixed        = false;
        FixedGapSize = null;
    }

    /// <summary>Jumps directly to a starting level (used by Custom Game).</summary>
    public void SetLevel(int level)
    {
        Level     = level;
        PipeSpeed = level switch { <= 2 => 3, <= 4 => 2, _ => 1 };
    }

    /// <summary>
    /// Advances level based on score; returns true when level changed.
    /// Uses &lt;= instead of == so a manually-elevated starting level never regresses.
    /// </summary>
    public bool Update(int score)
    {
        if (Fixed) return false;

        int newLevel = (score / 5) + 1;
        if (newLevel <= Level) return false;

        Level = newLevel;
        PipeSpeed = Level switch
        {
            <= 2 => 3,
            <= 4 => 2,
            _    => 1,
        };
        return true;
    }

    /// <summary>Current gap size; uses FixedGapSize when set (Manual mode).</summary>
    public int GapSize => FixedGapSize ?? Math.Max(
        GameState.MinGapSize,
        GameState.BaseGapSize - (Level / 3));
}
