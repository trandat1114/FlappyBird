using System.Text;

namespace FlappyBird.Game.Modes.TwoPlayer
{
    /// <summary>
    /// Double-buffer chống flicker cho TwoPlayer.
    /// Flush() gom các char liên tiếp cùng màu thành một Console.Write(string)
    /// thay vì gọi SetCursorPosition cho từng char riêng lẻ.
    /// </summary>
    public class TwoPlayerBuffer
    {
        public const int MENU_BORDER_WIDTH = 66;
        public const int TOTAL_DISPLAY_HEIGHT = 24;

        private char[,] _prev = new char[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];
        private char[,] _cur = new char[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];
        private ConsoleColor[,] _prevFg = new ConsoleColor[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];
        private ConsoleColor[,] _curFg = new ConsoleColor[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];

        // Pre-allocated StringBuilder – không new mỗi frame
        private readonly StringBuilder _sb = new(MENU_BORDER_WIDTH);

        public bool BufferInitialized { get; private set; } = false;

        public void InitializeBuffers()
        {
            _prev = new char[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];
            _cur = new char[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];
            _prevFg = new ConsoleColor[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];
            _curFg = new ConsoleColor[TOTAL_DISPLAY_HEIGHT, MENU_BORDER_WIDTH];

            for (int y = 0; y < TOTAL_DISPLAY_HEIGHT; y++)
                for (int x = 0; x < MENU_BORDER_WIDTH; x++)
                {
                    _prev[y, x] = '\0'; // force full redraw lần đầu
                    _cur[y, x] = ' ';
                    _prevFg[y, x] = ConsoleColor.White;
                    _curFg[y, x] = ConsoleColor.White;
                }

            BufferInitialized = true;
        }

        public void ClearCurrentBuffer()
        {
            for (int y = 0; y < TOTAL_DISPLAY_HEIGHT; y++)
                for (int x = 0; x < MENU_BORDER_WIDTH; x++)
                {
                    _cur[y, x] = ' ';
                    _curFg[y, x] = ConsoleColor.White;
                }
        }

        public void WriteToBuffer(int x, int y, char ch, ConsoleColor fg = ConsoleColor.White)
        {
            if ((uint)x < MENU_BORDER_WIDTH && (uint)y < TOTAL_DISPLAY_HEIGHT)
            {
                _cur[y, x] = ch;
                _curFg[y, x] = fg;
            }
        }

        /// <summary>
        /// Flush diff: mỗi row quét một lần, gom run liên tiếp cùng màu vào
        /// một Console.Write(string). Giảm số lần gọi SetCursorPosition từ
        /// O(changed_cells) xuống còn O(changed_runs_per_row).
        /// </summary>
        public void FlushBufferToConsole()
        {
            for (int y = 0; y < TOTAL_DISPLAY_HEIGHT; y++)
            {
                int x = 0;
                while (x < MENU_BORDER_WIDTH)
                {
                    // Bỏ qua cell không đổi
                    if (_cur[y, x] == _prev[y, x] && _curFg[y, x] == _prevFg[y, x])
                    {
                        x++;
                        continue;
                    }

                    // Bắt đầu run – đặt cursor một lần cho cả run
                    Console.SetCursorPosition(x, y);
                    ConsoleColor activeColor = _curFg[y, x];
                    Console.ForegroundColor = activeColor;
                    _sb.Clear();

                    while (x < MENU_BORDER_WIDTH)
                    {
                        bool changed = _cur[y, x] != _prev[y, x] || _curFg[y, x] != _prevFg[y, x];
                        if (!changed) break; // kết thúc run khi gặp cell không đổi

                        ConsoleColor fg = _curFg[y, x];
                        if (fg != activeColor)
                        {
                            // Màu khác – flush run hiện tại rồi bắt đầu run màu mới
                            Console.Write(_sb.ToString());
                            _sb.Clear();
                            activeColor = fg;
                            Console.ForegroundColor = activeColor;
                        }

                        _sb.Append(_cur[y, x]);
                        _prev[y, x] = _cur[y, x];
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
