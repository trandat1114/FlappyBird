using System.Collections.Concurrent;
using System.Runtime.Versioning;
using FlappyBird.Audio.Enum;
using FlappyBird.Settings;

namespace FlappyBird.Audio;

/// <summary>
/// Central audio hub.
///
/// Background music : Console.Beep note loop (Windows only; the Harry Potter melody).
/// Sound effects    : MP3 files via GameAudioEngine (NAudio).
///                   Respects <see cref="AudioOffset"/> start-offset per sound.
///                   Fallback to Console.Beep if files are not found.
///
/// Call <see cref="SetAudioDir"/> once at startup before any playback.
/// Call <see cref="LoadAudioOffset"/> once at startup to restore saved calibration.
/// </summary>
public static class AudioManager
{
    // ── Phase-4 provider bridge (architectural hook, not yet wired for all paths) ──
    public static IAudioProvider Provider { get; set; } = ConsoleBeepAudioProvider.Instance;

    // ── NAudio engine (Windows only) ───────────────────────────────────────────
    private static readonly GameAudioEngine? _engine =
        OperatingSystem.IsWindows() ? new GameAudioEngine() : null;

    // ── Audio file paths ───────────────────────────────────────────────────────
    private static string _flapPath  = "";
    private static string _pointPath = "";

    /// <summary>
    /// Sets the directory containing flap.mp3 and point.mp3, and pre-loads both
    /// files into the NAudio engine for low-latency playback.
    /// </summary>
    public static void SetAudioDir(string audioDir)
    {
        _flapPath  = Path.Combine(audioDir, "flap.mp3");
        _pointPath = Path.Combine(audioDir, "point.mp3");

        if (_engine is not null)
        {
            _engine.PreloadEffect(_flapPath);
            _engine.PreloadEffect(_pointPath);
        }
    }

    // ── Per-sound start-offset calibration ────────────────────────────────────

    /// <summary>
    /// Live offset config. Written by the calibration menu; read by PlaySoundEffect.
    /// Call <see cref="LoadAudioOffset"/> at startup to restore from disk.
    /// </summary>
    public static AudioOffsetConfig AudioOffset { get; private set; } = new();

    /// <summary>Replaces <see cref="AudioOffset"/> with the value loaded from disk.</summary>
    public static void LoadAudioOffset()
        => AudioOffset = AudioOffsetSerializer.Load();

    // ── Background music (Console.Beep note loop) ──────────────────────────────
    private static CancellationTokenSource? _musicCts;
    private static volatile bool _isMusicPlaying;

    /// <summary>True while the Harry Potter background music loop is active.</summary>
    public static bool IsPlaying => _isMusicPlaying;

    private const int NOTE_SEP = 20;

    public static void StartBackgroundMusic((Note note, int duration)[] melody)
    {
        if (_isMusicPlaying) return;
        _musicCts       = new CancellationTokenSource();
        _isMusicPlaying = true;
        var token       = _musicCts.Token;

        // Guard must be inside the lambda — the analyzer cannot track
        // OperatingSystem.IsWindows() across lambda boundaries.
        Task.Factory.StartNew(() =>
        {
            if (OperatingSystem.IsWindows()) MusicLoop(melody, token);
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public static void StopBackgroundMusic()
    {
        if (!_isMusicPlaying) return;
        _musicCts?.Cancel();
        Thread.Sleep(50);
        _musicCts?.Dispose();
        _musicCts       = null;
        _isMusicPlaying = false;
    }

    // ── Sound effects ──────────────────────────────────────────────────────────
    private static readonly ConcurrentBag<CancellationTokenSource> _fxTokens = [];

    /// <summary>
    /// Plays a sound effect using the current <see cref="GameSettings.EffectsVolume"/>
    /// and the per-sound start offset from <see cref="AudioOffset"/>.
    /// </summary>
    public static void PlaySoundEffect(SoundEffect effect)
    {
        int vol = GameSettings.Instance.EffectsVolume;
        if (vol <= 0) return;
        PlaySoundEffectWithOffset(effect, AudioOffset.GetStartOffset(effect), vol / 100f);
    }

    /// <summary>
    /// Plays a sound effect with an explicit start offset (used by calibration engine).
    /// <paramref name="startOffsetMs"/>: skip this many ms from the start of the file.
    /// Uses a guaranteed minimum volume of 50 % for the calibration UI.
    /// </summary>
    public static void PlaySoundEffectWithOffset(SoundEffect effect, int startOffsetMs)
    {
        float volF = Math.Max(GameSettings.Instance.EffectsVolume, 50) / 100f;
        PlaySoundEffectWithOffset(effect, startOffsetMs, volF);
    }

    private static void PlaySoundEffectWithOffset(SoundEffect effect, int startOffsetMs, float volF)
    {
        switch (effect)
        {
            case SoundEffect.Jump:
                if (OperatingSystem.IsWindows())
                {
                    if (_engine is not null && _engine.HasEffect(_flapPath))
                        _engine.PlayEffect(_flapPath, volF, startOffsetMs);
                    else
                        BeepAsync((Note.C5, 80));
                }
                break;

            case SoundEffect.Score:
                if (OperatingSystem.IsWindows())
                {
                    if (_engine is not null && _engine.HasEffect(_pointPath))
                        _engine.PlayEffect(_pointPath, volF, startOffsetMs);
                    else
                        BeepAsync((Note.E5, 120), (Note.G5, 120));
                }
                break;

            case SoundEffect.GameOver:
                if (OperatingSystem.IsWindows())
                    BeepAsync((Note.G4, 80), (Note.F4, 80), (Note.E4, 150));
                break;
        }
    }

    public static void StopAllSounds()
    {
        StopBackgroundMusic();
        _engine?.Dispose();
        while (_fxTokens.TryTake(out var cts))
            try { cts.Cancel(); cts.Dispose(); } catch { }
    }

    // ── Console.Beep helpers (Windows-only) ────────────────────────────────────

    [SupportedOSPlatform("windows")]
    private static void BeepAsync(params (Note note, int duration)[] seq)
    {
        var cts   = new CancellationTokenSource();
        _fxTokens.Add(cts);
        var token = cts.Token;

        Task.Factory.StartNew(() =>
        {
            try
            {
                Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                foreach (var (note, dur) in seq)
                {
                    if (token.IsCancellationRequested) break;
                    Console.Beep((int)note, dur);
                    if (seq.Length > 1) Thread.Sleep(30);
                }
            }
            catch { }
            finally { try { cts.Dispose(); } catch { } }
        }, token, TaskCreationOptions.None, TaskScheduler.Default);
    }

    [SupportedOSPlatform("windows")]
    private static void MusicLoop((Note note, int duration)[] melody, CancellationToken token)
    {
        try
        {
            Thread.CurrentThread.Priority = ThreadPriority.Normal;
            while (!token.IsCancellationRequested)
            {
                foreach (var (note, dur) in melody)
                {
                    if (token.IsCancellationRequested) break;
                    try
                    {
                        if (note == Note.Rest)
                            token.WaitHandle.WaitOne(dur);
                        else
                            Console.Beep((int)note, Math.Max(dur - NOTE_SEP, 50));

                        if (!token.IsCancellationRequested)
                            token.WaitHandle.WaitOne(NOTE_SEP);
                    }
                    catch (OperationCanceledException) { break; }
                    catch
                    {
                        if (!token.IsCancellationRequested) token.WaitHandle.WaitOne(50);
                    }
                }
                if (!token.IsCancellationRequested) token.WaitHandle.WaitOne(500);
            }
        }
        catch { }
        finally { _isMusicPlaying = false; }
    }
}
