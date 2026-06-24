using System.Collections.Concurrent;
using FlappyBird.Audio.Enum;

namespace FlappyBird.Audio;

/// <summary>
/// Windows Console.Beep implementation of IAudioProvider.
/// Wraps the existing AudioManager logic so the rest of the codebase
/// can call through the interface without knowing the implementation.
/// </summary>
public sealed class ConsoleBeepAudioProvider : IAudioProvider
{
    public static readonly ConsoleBeepAudioProvider Instance = new();

    private CancellationTokenSource? _cts;
    private readonly ConcurrentBag<CancellationTokenSource> _fxTokens = new();
    private const int NOTE_GAP = 20;

    public bool IsPlaying { get; private set; }

    public void StartMusic((Note note, int duration)[] melody)
    {
        if (!OperatingSystem.IsWindows() || IsPlaying) return;

        _cts = new CancellationTokenSource();
        IsPlaying = true;
        var token = _cts.Token;

        Task.Factory.StartNew(() =>
        {
            if (!OperatingSystem.IsWindows()) return;
            try
            {
                Thread.CurrentThread.Priority = ThreadPriority.Normal;
                while (!token.IsCancellationRequested)
                {
                    foreach (var (note, dur) in melody)
                    {
                        if (token.IsCancellationRequested) break;
                        if (note == Note.Rest)
                            token.WaitHandle.WaitOne(dur);
                        else
                            Console.Beep((int)note, Math.Max(dur - NOTE_GAP, 50));
                        token.WaitHandle.WaitOne(NOTE_GAP);
                    }
                    token.WaitHandle.WaitOne(500);
                }
            }
            catch { }
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public void StopMusic()
    {
        if (!IsPlaying) return;
        _cts?.Cancel();
        Thread.Sleep(50);
        _cts?.Dispose();
        _cts = null;
        IsPlaying = false;
    }

    public void PlayEffect(SoundEffect effect)
    {
        if (!OperatingSystem.IsWindows()) return;
        var cts = new CancellationTokenSource();
        _fxTokens.Add(cts);

        Task.Factory.StartNew(async () =>
        {
            if (!OperatingSystem.IsWindows()) return;
            try
            {
                Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                (Note, int)[] seq = effect switch
                {
                    SoundEffect.Jump     => [(Note.C5, 80)],
                    SoundEffect.Score    => [(Note.E5, 120), (Note.G5, 120)],
                    SoundEffect.GameOver => [(Note.G4, 80), (Note.F4, 80), (Note.E4, 150)],
                    _                    => [],
                };
                foreach (var (note, dur) in seq)
                {
                    if (cts.Token.IsCancellationRequested) break;
                    Console.Beep((int)note, dur);
                    if (seq.Length > 1) await Task.Delay(30, cts.Token);
                }
            }
            catch { }
            finally { try { cts.Dispose(); } catch { } }
        }, cts.Token, TaskCreationOptions.None, TaskScheduler.Default);
    }

    public void StopAll()
    {
        StopMusic();
        while (_fxTokens.TryTake(out var t))
        {
            try { t.Cancel(); t.Dispose(); } catch { }
        }
    }
}
