using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using FlappyBird.Audio.Enum;

namespace FlappyBird.Audio;

public static class AudioManager
{
    /// <summary>
    /// Active audio provider. Replace to swap implementation:
    ///   AudioManager.Provider = NullAudioProvider.Instance;  // silence
    ///   AudioManager.Provider = ConsoleBeepAudioProvider.Instance;  // default
    /// SettingsMenu toggles music by calling StartBackgroundMusic / StopBackgroundMusic.
    /// </summary>
    public static IAudioProvider Provider { get; set; } = ConsoleBeepAudioProvider.Instance;

    private static CancellationTokenSource? _cancellationTokenSource;
    private static readonly ConcurrentBag<CancellationTokenSource> _soundEffectTokens = new();
    public static bool _isPlaying = false;
    
    // Thông số tối ưu cho âm thanh mượt mà
    private const int NOTE_SEPARATION = 20; // Giảm khoảng cách giữa các nốt
    private const int THREAD_PRIORITY_BOOST = 10; // Boost priority cho audio threads
    
    public static void StartBackgroundMusic((Note note, int duration)[] melody)
    {
        if (_isPlaying) return;

        _cancellationTokenSource = new CancellationTokenSource();
        _isPlaying = true;

        // Tạo task với priority cao hơn cho âm thanh
        var musicTask = Task.Factory.StartNew(
            () => PlayBackgroundMusicLoop(melody, _cancellationTokenSource.Token),
            _cancellationTokenSource.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
        );
    }

    public static void StopBackgroundMusic()
    {
        if (!_isPlaying) return;

        _cancellationTokenSource?.Cancel();
        
        // Đợi một chút để task kết thúc gracefully
        Thread.Sleep(50);
        
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _isPlaying = false;
    }

    public static void StopAllSounds()
    {
        // Dừng nhạc nền trước
        StopBackgroundMusic();

        // Dừng tất cả sound effects
        var tokens = new List<CancellationTokenSource>();
        while (_soundEffectTokens.TryTake(out var token))
        {
            tokens.Add(token);
        }

        // Cancel và dispose tất cả tokens
        Parallel.ForEach(tokens, token =>
        {
            try
            {
                token.Cancel();
                Thread.Sleep(10); // Cho phép task kết thúc
                token.Dispose();
            }
            catch
            {
                // Ignore disposal errors
            }
        });
    }

    public static void PlaySoundEffect(SoundEffect effect)
    {
        if (!OperatingSystem.IsWindows()) return;

        var tokenSource = new CancellationTokenSource();
        _soundEffectTokens.Add(tokenSource);

        // Sử dụng Task.Factory với priority cao hơn
        Task.Factory.StartNew(async () =>
        {
            try
            {
                // Boost thread priority cho sound effects
                Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;

                switch (effect)
                {
                    case SoundEffect.Jump:
                        await PlaySoundSequence(tokenSource.Token, 
                            (Note.C5, 80));
                        break;

                    case SoundEffect.Score:
                        await PlaySoundSequence(tokenSource.Token,
                            (Note.E5, 120),
                            (Note.G5, 120));
                        break;

                    case SoundEffect.GameOver:
                        await PlaySoundSequence(tokenSource.Token,
                            (Note.G4, 80),
                            (Note.F4, 80),
                            (Note.E4, 150));
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                // Task was cancelled - this is expected
            }
            catch
            {
                // Ignore other audio errors
            }
            finally
            {
                // Cleanup
                try
                {
                    tokenSource.Dispose();
                }
                catch { }
            }
        }, tokenSource.Token, TaskCreationOptions.None, TaskScheduler.Default);
    }

    // Helper method để phát sequence âm thanh mượt mà
    private static async Task PlaySoundSequence(CancellationToken token, params (Note note, int duration)[] sequence)
    {
        foreach (var (note, duration) in sequence)
        {
            if (token.IsCancellationRequested) break;

            try
            {
                if (OperatingSystem.IsWindows())
                {
                    Console.Beep((int)note, duration);
                }
                
                // Khoảng nghỉ ngắn giữa các nốt cho mượt mà
                if (sequence.Length > 1)
                {
                    await Task.Delay(30, token);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // Ignore beep errors
            }
        }
    }

    private static void PlayBackgroundMusicLoop((Note note, int duration)[] melody, CancellationToken cancellationToken)
    {
        if (!OperatingSystem.IsWindows()) return;

        try
        {
            // Boost thread priority cho background music
            Thread.CurrentThread.Priority = ThreadPriority.Normal;

            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var (note, duration) in melody)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        if (note == Note.Rest)
                        {
                            // Sử dụng cancellation-aware sleep
                            cancellationToken.WaitHandle.WaitOne(duration);
                        }
                        else
                        {
                            // Điều chỉnh duration để tránh overlap
                            var adjustedDuration = Math.Max(duration - NOTE_SEPARATION, 50);
                            Console.Beep((int)note, adjustedDuration);
                        }

                        // Khoảng nghỉ nhỏ giữa các nốt để tránh overlap
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            cancellationToken.WaitHandle.WaitOne(NOTE_SEPARATION);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch
                    {
                        // Ignore individual beep errors và tiếp tục
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            cancellationToken.WaitHandle.WaitOne(50);
                        }
                    }
                }

                // Khoảng nghỉ giữa các lần lặp melody
                if (!cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.WaitHandle.WaitOne(500);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Background music was cancelled - this is expected
        }
        catch
        {
            // Ignore other audio errors
        }
    }
}