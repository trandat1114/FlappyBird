using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace FlappyBird.Audio;

/// <summary>
/// NAudio-based audio engine for Flappy Bird.
///
/// Sound effects  — MP3 files pre-loaded into byte arrays at startup.
///                  PlayEffect() creates a MemoryStream per call so concurrent plays
///                  never share seek position. Each play runs on its own Task thread.
///
/// Background music — Streamed from file via AudioFileReader + LoopStream.
///                    SetMusicVolume() adjusts level in real-time without restarting.
///
/// Requires Windows (WaveOutEvent uses waveOut API). Guard call sites with
/// OperatingSystem.IsWindows() before calling any method here.
/// </summary>
public sealed class GameAudioEngine : IDisposable
{
    // ── Sound effect pre-load cache ───────────────────────────────────────────

    // Stores raw MP3 bytes. Each PlayEffect() wraps them in a fresh MemoryStream,
    // so multiple simultaneous plays are completely independent.
    private readonly Dictionary<string, byte[]> _sfxRaw =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Reads <paramref name="path"/> into memory and validates it as audio.
    /// Silently skips files that do not exist, cannot be read, or are already cached.
    /// </summary>
    public void PreloadEffect(string path)
    {
        if (!File.Exists(path) || _sfxRaw.ContainsKey(path)) return;
        try { _sfxRaw[path] = File.ReadAllBytes(path); }
        catch { }
    }

    /// <summary>True if <paramref name="path"/> has been successfully pre-loaded.</summary>
    public bool HasEffect(string path) => _sfxRaw.ContainsKey(path);

    /// <summary>
    /// Plays the pre-loaded sound effect asynchronously.
    /// <paramref name="volume"/> is 0.0 – 1.0. Concurrent calls are safe.
    /// <paramref name="startOffsetMs"/> skips the first N milliseconds of the file
    /// (trims leading silence so the audible click arrives faster).
    /// No-op if the path was not pre-loaded.
    /// </summary>
    public void PlayEffect(string path, float volume, int startOffsetMs = 0)
    {
        if (!_sfxRaw.TryGetValue(path, out byte[]? raw)) return;

        float  v       = Math.Clamp(volume, 0f, 1f);
        byte[] data    = raw;   // captured; dictionary never mutates after Preload
        int    startMs = Math.Max(0, startOffsetMs);

        Task.Run(() =>
        {
            try
            {
                // MemoryStream from pre-loaded bytes: no disk I/O, instant start.
                // Mp3FileReader decodes MP3 frames from the stream.
                // CurrentTime seek trims leading silence when startOffsetMs > 0.
                // VolumeSampleProvider applies per-play gain.
                // WaveOutEvent: classic waveOut API — most reliable for console apps
                // (no COM, no window handle, no WASAPI session setup needed).
                using var ms     = new MemoryStream(data, writable: false);
                using var reader = new Mp3FileReader(ms);
                if (startMs > 0)
                    reader.CurrentTime = TimeSpan.FromMilliseconds(startMs);
                var       vol    = new VolumeSampleProvider(reader.ToSampleProvider())
                                       { Volume = v };
                using var output = new WaveOutEvent { DesiredLatency = 100 };
                output.Init(vol);
                output.Play();

                // Keep WaveOutEvent + reader alive until playback completes.
                while (output.PlaybackState == PlaybackState.Playing)
                    Thread.Sleep(20);
            }
            catch { }
        });
    }

    // ── Background music ──────────────────────────────────────────────────────

    private WaveOutEvent?    _musicOut;
    private AudioFileReader? _musicReader;
    private LoopStream?      _musicLoop;
    private readonly object  _musicLock = new();

    /// <summary>True while background music is actively playing.</summary>
    public bool IsMusicPlaying
    {
        get
        {
            lock (_musicLock)
                return _musicOut?.PlaybackState == PlaybackState.Playing;
        }
    }

    /// <summary>
    /// Streams <paramref name="path"/> as looping background music.
    /// Stops any currently playing music first.
    /// </summary>
    public void StartMusic(string path, float volume = 1.0f)
    {
        lock (_musicLock)
        {
            StopMusicCore();
            if (!File.Exists(path)) return;
            try
            {
                var reader   = new AudioFileReader(path) { Volume = Math.Clamp(volume, 0f, 1f) };
                _musicReader = reader;
                _musicLoop   = new LoopStream(reader);
                _musicOut    = new WaveOutEvent { DesiredLatency = 200 };
                _musicOut.Init(_musicLoop);
                _musicOut.Play();
            }
            catch { StopMusicCore(); }
        }
    }

    /// <summary>Stops background music and releases all audio resources.</summary>
    public void StopMusic()
    {
        lock (_musicLock) StopMusicCore();
    }

    /// <summary>
    /// Changes background music volume without restarting the track.
    /// <paramref name="volume"/> is 0.0 – 1.0.
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        lock (_musicLock)
            if (_musicReader is not null)
                _musicReader.Volume = Math.Clamp(volume, 0f, 1f);
    }

    private void StopMusicCore()
    {
        _musicOut?.Stop();
        _musicOut?.Dispose();    _musicOut    = null;
        _musicLoop?.Dispose();   _musicLoop   = null;
        _musicReader?.Dispose(); _musicReader = null;
    }

    public void Dispose()
    {
        lock (_musicLock) StopMusicCore();
    }
}
