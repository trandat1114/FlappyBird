using FlappyBird.Audio.Enum;

namespace FlappyBird.Audio;

/// <summary>
/// Stores per-sound start-offset values (milliseconds to skip at the beginning of each MP3).
/// Trimming leading silence makes the audible click arrive faster after the game event fires.
/// </summary>
public sealed class AudioOffsetConfig
{
    public int JumpStartOffsetMs  { get; set; } = 0;
    public int ScoreStartOffsetMs { get; set; } = 0;

    public static readonly int MinOffsetMs = 0;
    public static readonly int MaxOffsetMs = 2000;

    /// <summary>Returns the start offset for <paramref name="effect"/>.</summary>
    public int GetStartOffset(SoundEffect effect) => effect switch
    {
        SoundEffect.Jump  => JumpStartOffsetMs,
        SoundEffect.Score => ScoreStartOffsetMs,
        _                 => 0,
    };

    /// <summary>Clamps and applies <paramref name="deltaMs"/> to the active sound's offset.</summary>
    public void Adjust(SoundEffect effect, int deltaMs)
    {
        switch (effect)
        {
            case SoundEffect.Jump:
                JumpStartOffsetMs = Math.Clamp(JumpStartOffsetMs + deltaMs, MinOffsetMs, MaxOffsetMs);
                break;
            case SoundEffect.Score:
                ScoreStartOffsetMs = Math.Clamp(ScoreStartOffsetMs + deltaMs, MinOffsetMs, MaxOffsetMs);
                break;
        }
    }

    public void Reset() { JumpStartOffsetMs = 0; ScoreStartOffsetMs = 0; }

    public AudioOffsetConfig Clone() => new()
    {
        JumpStartOffsetMs  = JumpStartOffsetMs,
        ScoreStartOffsetMs = ScoreStartOffsetMs,
    };
}
