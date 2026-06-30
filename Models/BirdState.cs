namespace FlappyBird.Models;

/// <summary>
/// Pure bird physics and animation state — extracted from GameState (Phase 3).
/// GameState embeds this until full migration allows passing BirdState directly.
/// </summary>
public class BirdState
{
    public const int X = GameState.BirdX;

    public int   Y             { get; set; } = 10;
    public float Yf            { get; set; } = 10f;
    public float Velocity      { get; set; } = 0f;
    public int   AnimationFrame { get; set; } = 0;

    public void Reset()
    {
        Y              = 10;
        Yf             = 10f;
        Velocity       = 0f;
        AnimationFrame = 0;
    }

    /// <summary>True when bird is rising quickly.</summary>
    public bool IsRising  => Velocity < -0.12f;
    /// <summary>True when bird is falling quickly.</summary>
    public bool IsFalling => Velocity > 0.12f;
}
