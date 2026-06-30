using FlappyBird.Audio.Enum;

namespace FlappyBird.Audio;

/// <summary>
/// Abstraction over audio output. Swap implementations via
/// <see cref="AudioManager.Provider"/> without changing call sites.
/// </summary>
public interface IAudioProvider
{
    void StartMusic((Note note, int duration)[] melody);
    void StopMusic();
    void PlayEffect(SoundEffect effect);
    bool IsPlaying { get; }
}
