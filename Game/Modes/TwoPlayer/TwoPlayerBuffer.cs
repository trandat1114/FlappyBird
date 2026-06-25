using System.Text;

namespace FlappyBird.Game.Modes.TwoPlayer
{
    /// <summary>
    /// Double-buffer chống flicker cho TwoPlayer (side-by-side layout, 80×24).
    /// Flush() gom các char liên tiếp cùng màu thành một Console.Write(string).
    /// </summary>
    public class TwoPlayerBuffer
    {
        public const int CONSOLE_WIDTH        = 80;
        public const int TOTAL_DISPLAY_HEIGHT = 24;

        private char[,]         _prev   = new char[TOTAL_DISPLAY_HEIGHT, CONSOLE_WIDTH];
        private char[,]         _cur    = new char[TOTAL_DISPLAY_HEIGHT, CONSOLE_WIDTH];
        private ConsoleColor[,] _prevFg = new ConsoleColor[TOTAL_DISPLAY_HEIGHT, CONSOLE_WIDTH];
        private ConsoleColor[,] _curFg  = new ConsoleColor[TOTAL_DISPLAY_HEIGHT, CONSOLE_WIDTH];

        private readonly StringBuilder _sb = new(CONSOLE_WIDTH);

        public bool BufferInitialized { get; private set; } = false;

        public void InitializeBuffers()
        {
            _prev   = new char[TOTAL_DISPLAY_HEIGHT, CONSOLE_WIDTH];
            _cur    = new char[TOTAL_DISPLAY_HEIGHT, CONSOLE_WIDTH];
            _prevFg = new ConsoleColor[TOTAL_DISPLAY_HEIGHT, CONSOLE_WIDTH];
            _curFg  = new ConsoleColor[TOTAL_DISPLAY_HEIGHT, CONSOLE_WIDTH];

            for (int y = 0; y < TOTAL_DISPLAY_HEIGHT; y++)
                for (int x = 0; x < CONSOLE_WIDTH; x++)
                {
                    _prev[y, x]   = '\0';
                    _cur[y, x]    = ' ';
                    _prevFg[y, x] = ConsoleColor.White;
                    _curFg[y, x]  = ConsoleColor.White;
                }

            BufferInitialized = true;
        }

        /// <summary>Reset _prev to '\0' so FlushBufferToConsole redraws all cells after Console.Clear().</summary>
        public void ForceFullRedraw()
        {
            for (int y = 0; y < TOTAL_DISPLAY_HEIGHT; y++)
                for (int x = 0; x < CONSOLE_WIDTH; x++)
                    _prev[y, x] = '\0';
        }

        public void ClearCurrentBuffer()
        {
            for (int y = 0; y < TOTAL_DISPLAY_HEIGHT; y++)
                for (int x = 0; x < CONSOLE_WIDTH; x++)
                {
                    _cur[y, x]   = ' ';
                    _curFg[y, x] = ConsoleColor.White;
                }
        }

        public void WriteToBuffer(int x, int y, char ch, ConsoleColor fg = ConsoleColor.White)
        {
            if ((uint)x < CONSOLE_WIDTH && (uint)y < TOTAL_DISPLAY_HEIGHT)
            {
                _cur[y, x]   = ch;
                _curFg[y, x] = fg;
            }
        }

        public void FlushBufferToConsole()
        {
            for (int y = 0; y < TOTAL_DISPLAY_HEIGHT; y++)
            {
                int x = 0;
                while (x < CONSOLE_WIDTH)
                {
                    if (_cur[y, x] == _prev[y, x] && _curFg[y, x] == _prevFg[y, x])
                    {
                        x++;
                        continue;
                    }

                    Console.SetCursorPosition(x, y);
                    ConsoleColor activeColor = _curFg[y, x];
                    Console.ForegroundColor = activeColor;
                    _sb.Clear();

                    while (x < CONSOLE_WIDTH)
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

                    if (_sb.Length > 0)
                        Console.Write(_sb.ToString());
                }
            }

            Console.ResetColor();
        }
    }
}
