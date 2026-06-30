using System.Text;
using FlappyBird.Rendering;

namespace FlappyBird.Game.Modes.TwoPlayer
{
    /// <summary>
    /// Double-buffer anti-flicker for TwoPlayer (side-by-side layout).
    /// Respects ConsoleLayout constraints: min 24×78, max 24×80.
    /// Call Resize() at the start of each Render() to adapt when user resizes terminal.
    /// </summary>
    public class TwoPlayerBuffer
    {
        // Current terminal dimensions (updated by Resize(), respects ConsoleLayout constraints)
        public int Width  { get; private set; }
        public int Height { get; private set; }

        private char[,]         _prev   = new char[1, 1];
        private char[,]         _cur    = new char[1, 1];
        private ConsoleColor[,] _prevFg = new ConsoleColor[1, 1];
        private ConsoleColor[,] _curFg  = new ConsoleColor[1, 1];

        private readonly StringBuilder _sb = new(200);

        public bool BufferInitialized { get; private set; } = false;

        public TwoPlayerBuffer()
        {
            Width  = ConsoleLayout.W;
            Height = ConsoleLayout.H;
            AllocArrays();
        }

        // ── Lifecycle ──────────────────────────────────────────────────────────

        public void InitializeBuffers() => AllocArrays();

        /// <summary>
        /// Checks console dimensions (via ConsoleLayout) and reallocates if changed.
        /// Returns true when a resize occurred (caller should Console.Clear + ForceFullRedraw).
        /// </summary>
        public bool Resize()
        {
            int w = ConsoleLayout.W;
            int h = ConsoleLayout.H;
            if (w == Width && h == Height) return false;
            Width  = w;
            Height = h;
            AllocArrays();
            return true;
        }

        private void AllocArrays()
        {
            _prev   = new char[Height, Width];
            _cur    = new char[Height, Width];
            _prevFg = new ConsoleColor[Height, Width];
            _curFg  = new ConsoleColor[Height, Width];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    _cur[y, x]   = ' ';
                    _curFg[y, x] = ConsoleColor.White;
                }
            ForceFullRedraw();
            BufferInitialized = true;
        }

        /// <summary>Reset _prev to '\0' so FlushBufferToConsole redraws all cells after Console.Clear().</summary>
        public void ForceFullRedraw()
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    _prev[y, x] = '\0';
        }

        public void ClearCurrentBuffer()
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    _cur[y, x]   = ' ';
                    _curFg[y, x] = ConsoleColor.White;
                }
        }

        public void WriteToBuffer(int x, int y, char ch, ConsoleColor fg = ConsoleColor.White)
        {
            if ((uint)x < Width && (uint)y < Height)
            {
                _cur[y, x]   = ch;
                _curFg[y, x] = fg;
            }
        }

        public void FlushBufferToConsole()
        {
            // Snapshot terminal bounds once — prevents crash when terminal shrinks between Resize() and here.
            int bufW = Console.BufferWidth;
            int bufH = Console.BufferHeight;

            for (int y = 0; y < Height; y++)
            {
                if (y >= bufH) break;
                int x = 0;
                while (x < Width)
                {
                    if (x >= bufW) break;
                    if (_cur[y, x] == _prev[y, x] && _curFg[y, x] == _prevFg[y, x])
                    { x++; continue; }

                    Console.SetCursorPosition(x, y);
                    ConsoleColor activeColor = _curFg[y, x];
                    Console.ForegroundColor = activeColor;
                    _sb.Clear();

                    while (x < Width && x < bufW)
                    {
                        bool changed = _cur[y, x] != _prev[y, x] || _curFg[y, x] != _prevFg[y, x];
                        if (!changed) break;

                        ConsoleColor fg = _curFg[y, x];
                        if (fg != activeColor)
                        {
                            Console.Write(_sb.ToString());
                            _sb.Clear();
                            activeColor = fg;
                            Console.ForegroundColor = activeColor;
                        }

                        _sb.Append(_cur[y, x]);
                        _prev[y, x]   = _cur[y, x];
                        _prevFg[y, x] = _curFg[y, x];
                        x++;
                    }

                    if (_sb.Length > 0) Console.Write(_sb.ToString());
                }
            }
            Console.ResetColor();
        }
    }
}
