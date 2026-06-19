using FlappyBird.Audio.Enum;

namespace FlappyBird.Audio;

/// <summary>
/// Silent audio provider — used on non-Windows platforms or when sound is disabled.
/// </summary>
public sealed class NullAudioProvider : IAudioProvider
{
    public static readonly NullAudioProvider Instance = new();

    public bool IsPlaying => false;
    public void StartMusic((Note note, int duration)[] melody) { }
    public void StopMusic()                                    { }
    public void PlayEffect(SoundEffect effect)                 { }
}
