using FlappyBird.Audio.Enum;

namespace FlappyBird.Audio;

/// <summary>
/// Drives the audio offset calibration workflow.
///
/// — Maintains a working copy of <see cref="AudioOffsetConfig"/> that can be
///   discarded (ESC) or committed to disk (Save).
/// — Provides a metronome that fires the active sound at a fixed BPM so the
///   user can hear the current offset setting repeating at a steady rhythm.
/// — <see cref="BeatProgress"/> (0.0–1.0) reports how far through the current
///   beat interval we are; callers poll this to drive the progress bar in the UI.
/// — Thread-safe: the Timer callback fires on a thread-pool thread; BeatProgress
///   and BeatFlash use only volatile/Interlocked reads.
/// </summary>
public sealed class AudioCalibrationEngine : IDisposable
{
    // ── State ─────────────────────────────────────────────────────────────────

    /// <summary>Working config — not saved until <see cref="AudioManager.CommitAudioOffset"/> is called.</summary>
    public AudioOffsetConfig WorkingConfig { get; }

    public SoundEffect ActiveSound { get; set; } = SoundEffect.Jump;

    // ── Metronome ─────────────────────────────────────────────────────────────

    private Timer? _metro;
    private long   _lastBeatTick;           // Environment.TickCount64 at last beat
    private int    _beatFlip;               // toggled each beat for visual indicator

    public bool MetronomeRunning => _metro is not null;
    public int  MetronomeBpm     { get; private set; } = 120;

    /// <summary>Fraction 0.0–1.0 of the current beat interval that has elapsed.</summary>
    public double BeatProgress
    {
        get
        {
            if (!MetronomeRunning) return 0;
            long elapsed  = Environment.TickCount64 - Interlocked.Read(ref _lastBeatTick);
            long interval = 60_000 / MetronomeBpm;
            return Math.Clamp((double)elapsed / interval, 0, 1);
        }
    }

    /// <summary>Alternates true/false on each beat — use for a blinking indicator.</summary>
    public bool BeatFlash => (Interlocked.CompareExchange(ref _beatFlip, 0, 0) & 1) == 0;

    /// <summary>Raised on the timer thread at every beat; UI subscribes to trigger a redraw.</summary>
    public event Action? BeatFired;

    // ── Construction ──────────────────────────────────────────────────────────

    public AudioCalibrationEngine(AudioOffsetConfig workingConfig)
    {
        WorkingConfig = workingConfig;
    }

    // ── Offset adjustment ─────────────────────────────────────────────────────

    public int  CurrentOffset => WorkingConfig.GetStartOffset(ActiveSound);
    public void AdjustOffset(int deltaMs) => WorkingConfig.Adjust(ActiveSound, deltaMs);
    public void SwitchSound()
        => ActiveSound = ActiveSound == SoundEffect.Jump ? SoundEffect.Score : SoundEffect.Jump;

    // ── Playback ──────────────────────────────────────────────────────────────

    /// <summary>Plays the active sound once with the current working offset.</summary>
    public void PlayOnce()
        => AudioManager.PlaySoundEffectWithOffset(ActiveSound, CurrentOffset);

    // ── Metronome control ─────────────────────────────────────────────────────

    public void ToggleMetronome()
    {
        if (MetronomeRunning) StopMetronome();
        else StartMetronome();
    }

    public void AdjustBpm(int delta)
    {
        MetronomeBpm = Math.Clamp(MetronomeBpm + delta, 30, 240);
        if (MetronomeRunning) { StopMetronome(); StartMetronome(); }
    }

    private void StartMetronome()
    {
        Interlocked.Exchange(ref _lastBeatTick, Environment.TickCount64);
        int intervalMs = 60_000 / MetronomeBpm;
        _metro = new Timer(_ =>
        {
            Interlocked.Exchange(ref _lastBeatTick, Environment.TickCount64);
            Interlocked.Increment(ref _beatFlip);
            PlayOnce();
            BeatFired?.Invoke();
        }, null, intervalMs, intervalMs);
    }

    private void StopMetronome()
    {
        _metro?.Dispose();
        _metro = null;
        Interlocked.Exchange(ref _beatFlip, 0);
    }

    public void Dispose() => StopMetronome();
}
