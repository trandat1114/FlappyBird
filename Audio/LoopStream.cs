using NAudio.Wave;

namespace FlappyBird.Audio;

/// <summary>
/// Wraps any WaveStream so that it loops indefinitely:
/// when the source reaches EOF, Position resets to 0 and reading continues.
/// Used by GameAudioEngine for seamless background-music looping.
/// </summary>
internal sealed class LoopStream : WaveStream
{
    private readonly WaveStream _source;

    public LoopStream(WaveStream source) => _source = source;

    public override WaveFormat WaveFormat => _source.WaveFormat;

    // Report an effectively infinite length so WaveOutEvent never stops on its own.
    public override long Length => long.MaxValue;

    public override long Position
    {
        get => _source.Position;
        set => _source.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int totalRead = 0;
        while (totalRead < count)
        {
            int read = _source.Read(buffer, offset + totalRead, count - totalRead);
            if (read == 0)
            {
                // Avoid infinite loop on empty / unreadable source
                if (_source.Position == 0) break;
                _source.Position = 0;          // rewind and loop
            }
            else
            {
                totalRead += read;
            }
        }
        return totalRead;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _source.Dispose();
        base.Dispose(disposing);
    }
}
