namespace FlappyBird.Rendering;

/// <summary>
/// Generic diff-buffer for any console region. Tracks (char, color) per cell and only
/// writes cells that changed since the last flush — eliminates flicker without full redraws.
///
/// Usage:
///   var buf = new ConsoleBuffer(width: 66, height: 22, originX: 0, originY: 3);
///   buf.Set(x, y, '█', ConsoleColor.White);
///   buf.Flush();   // writes only changed cells
///   buf.Clear();   // reset to spaces for next frame
/// </summary>
public sealed class ConsoleBuffer
{
    private readonly int _w, _h, _ox, _oy;

    private readonly char[]          _curChar,  _prevChar;
    private readonly ConsoleColor[]  _curColor, _prevColor;

    private const ConsoleColor DEFAULT_COLOR = ConsoleColor.White;

    public int Width  => _w;
    public int Height => _h;

    public ConsoleBuffer(int width, int height, int originX = 0, int originY = 0)
    {
        _w = width; _h = height; _ox = originX; _oy = originY;
        int n = width * height;
        _curChar   = new char[n];
        _prevChar  = new char[n];
        _curColor  = new ConsoleColor[n];
        _prevColor = new ConsoleColor[n];
        Array.Fill(_curChar,  ' ');
        Array.Fill(_prevChar, '\0');   // '\0' forces first-frame full write
        Array.Fill(_curColor,  DEFAULT_COLOR);
        Array.Fill(_prevColor, DEFAULT_COLOR);
    }

    // ── Write API ─────────────────────────────────────────────────────────

    public void Set(int x, int y, char ch, ConsoleColor color = DEFAULT_COLOR)
    {
        if (x < 0 || x >= _w || y < 0 || y >= _h) return;
        int i = y * _w + x;
        _curChar[i]  = ch;
        _curColor[i] = color;
    }

    /// <summary>Writes a string left-to-right starting at (x, y), clipping at width.</summary>
    public void SetString(int x, int y, string text, ConsoleColor color = DEFAULT_COLOR)
    {
        for (int i = 0; i < text.Length && x + i < _w; i++)
            Set(x + i, y, text[i], color);
    }

    /// <summary>Fills an entire row with spaces (used to clear a line before writing).</summary>
    public void ClearRow(int y, ConsoleColor color = DEFAULT_COLOR)
    {
        for (int x = 0; x < _w; x++) Set(x, y, ' ', color);
    }

    /// <summary>Resets all cells to spaces for the next frame (call at start of each Update).</summary>
    public void Clear(ConsoleColor color = DEFAULT_COLOR)
    {
        Array.Fill(_curChar,  ' ');
        Array.Fill(_curColor, color);
    }

    // ── Flush ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Writes only cells that changed since the last Flush.
    /// Groups consecutive changed cells into a single Write call per row to minimise
    /// SetCursorPosition overhead.
    /// </summary>
    public void Flush()
    {
        for (int y = 0; y < _h; y++)
        {
            int x = 0;
            while (x < _w)
            {
                int i = y * _w + x;
                if (_curChar[i] == _prevChar[i] && _curColor[i] == _prevColor[i])
                { x++; continue; }

                // Start of a changed run — set cursor once
                Console.SetCursorPosition(_ox + x, _oy + y);
                ConsoleColor runColor = _curColor[i];
                Console.ForegroundColor = runColor;

                // Write consecutive cells with the same color
                while (x < _w)
                {
                    int j = y * _w + x;
                    if (_curChar[j] == _prevChar[j] && _curColor[j] == _prevColor[j]) break;

                    if (_curColor[j] != runColor)
                    {
                        runColor = _curColor[j];
                        Console.ForegroundColor = runColor;
                    }

                    Console.Write(_curChar[j]);
                    _prevChar[j]  = _curChar[j];
                    _prevColor[j] = _curColor[j];
                    x++;
                }
            }
        }
        Console.ResetColor();
    }

    /// <summary>Forces the next Flush to redraw all cells (e.g., after Console.Clear).</summary>
    public void Invalidate()
    {
        Array.Fill(_prevChar,  '\0');
        Array.Fill(_prevColor, DEFAULT_COLOR);
    }
}
