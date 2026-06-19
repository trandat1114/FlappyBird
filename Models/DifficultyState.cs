namespace FlappyBird.Models;

/// <summary>
/// Difficulty progression state — extracted from GameState (Phase 3).
/// </summary>
public class DifficultyState
{
    public int Level     { get; set; } = 1;
    public int PipeSpeed { get; set; } = 4;

    public void Reset()
    {
        Level     = 1;
        PipeSpeed = 4;
    }

    /// <summary>Advances level based on score; returns true when level changed.</summary>
    public bool Update(int score)
    {
        int newLevel = (score / 5) + 1;
        if (newLevel == Level) return false;

        Level = newLevel;
        PipeSpeed = Level switch
        {
            <= 2 => 4,
            <= 4 => 3,
            <= 6 => 2,
            _    => 1,
        };
        return true;
    }

    /// <summary>Current gap size, narrowing as level rises.</summary>
    public int GapSize => Math.Max(
        GameState.MinGapSize,
        GameState.BaseGapSize - (Level / 3));
}
