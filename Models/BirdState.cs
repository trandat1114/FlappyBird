namespace FlappyBird.Models;

/// <summary>
/// Pure bird physics and animation state — extracted from GameState (Phase 3).
/// GameState embeds this until full migration allows passing BirdState directly.
/// </summary>
public class BirdState
{
    public const int X = GameState.BirdX;

    public int   Y             { get; set; } = 11;
    public float Yf            { get; set; } = 11f;
    public float Velocity      { get; set; } = 0f;
    public int   AnimationFrame { get; set; } = 0;

    public void Reset()
    {
        Y              = 11;
        Yf             = 11f;
        Velocity       = 0f;
        AnimationFrame = 0;
    }

    /// <summary>True when bird is rising quickly.</summary>
    public bool IsRising  => Velocity < -0.12f;
    /// <summary>True when bird is falling quickly.</summary>
    public bool IsFalling => Velocity > 0.12f;
}
