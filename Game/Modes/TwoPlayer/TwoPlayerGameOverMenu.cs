using FlappyBird.Localization;
using FlappyBird.Models;
using FlappyBird.Settings;

namespace FlappyBird.Game.Modes.TwoPlayer
{
    /// <summary>
    /// Game Over for TwoPlayer — single footer row (row 23) via TwoPlayerBuffer.
    /// </summary>
    public class TwoPlayerGameOverMenu(TwoPlayerBuffer buffer)
    {
        private const int CONSOLE_W = TwoPlayerBuffer.CONSOLE_WIDTH;            // 80
        private const int FOOTER_Y  = TwoPlayerBuffer.TOTAL_DISPLAY_HEIGHT - 1; // 23

        private readonly TwoPlayerBuffer _buf = buffer;

        private bool     _show          = false;
        private int      _selectedIndex = 0;
        private DateTime _startTime     = DateTime.MinValue;

        private static string[] Options => [L.Get(L.GO_PLAY_AGAIN), L.Get(L.GO_MAIN_MENU)];

        public bool     ShowGameOverMenu => _show;
        public DateTime GameOverTime     => _startTime;

        // ── State ────────────────────────────────────────────────────────────

        public void StartGameOverMenu()
        {
            _show          = true;
            _selectedIndex = 0;
            _startTime     = DateTime.Now;
        }

        public void ResetGameOverMenu()
        {
            _show          = false;
            _selectedIndex = 0;
            _startTime     = DateTime.MinValue;
        }

        public bool CanReceiveInput() =>
            _show && DateTime.Now - _startTime > TimeSpan.FromMilliseconds(800);

        // ── Render ───────────────────────────────────────────────────────────

        /// <summary>
        /// Single-row game over footer at row 23:
        ///   [result info]  │  [► Play Again]  │  [Main Menu]
        /// </summary>
        public void RenderGameOverMenuToBuffer(GameState p1, GameState p2)
        {
            bool   isTie    = p1.GameOver && p2.GameOver && p1.Score == p2.Score;
            string winner   = GetWinner(p1, p2);
            string pts      = L.Get(L.GO_POINTS);
            var    opts     = Options;
            var    winColor = isTie ? ConsoleColor.Yellow : ConsoleColor.Green;

            string result = isTie
                ? $" TIE!  P1:{p1.Score,3} {pts}  P2:{p2.Score,3} {pts}"
                : $" {winner} wins  P1:{p1.Score,3}  P2:{p2.Score,3} {pts}";

            bool sel0 = _selectedIndex == 0;
            bool sel1 = _selectedIndex == 1;

            int pos = 0;
            pos = WriteSeg(pos, result,   winColor);
            pos = WriteSeg(pos, "  │  ",  ConsoleColor.White);
            pos = WriteSeg(pos, (sel0 ? " ► " : "   ") + opts[0],
                               sel0 ? ConsoleColor.Yellow : ConsoleColor.Gray);
            pos = WriteSeg(pos, "  │  ",  ConsoleColor.White);
            pos = WriteSeg(pos, (sel1 ? " ► " : "   ") + opts[1],
                               sel1 ? ConsoleColor.Yellow : ConsoleColor.Gray);
            while (pos < CONSOLE_W)
                _buf.WriteToBuffer(pos++, FOOTER_Y, ' ', ConsoleColor.DarkGray);
        }

        // ── Input ────────────────────────────────────────────────────────────

        public GameOverMenuAction HandleGameOverMenuInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.DownArrow:
                case ConsoleKey.LeftArrow:
                case ConsoleKey.RightArrow:
                    _selectedIndex = _selectedIndex == 0 ? 1 : 0;
                    return GameOverMenuAction.None;

                case ConsoleKey.Enter:
                    return _selectedIndex == 0 ? GameOverMenuAction.Restart : GameOverMenuAction.Exit;

                case ConsoleKey.Spacebar:
                    return GameOverMenuAction.Restart;

                case ConsoleKey.Escape:
                    return GameOverMenuAction.Exit;

                default:
                    return GameOverMenuAction.None;
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        public string GetWinner(GameState p1, GameState p2)
        {
            if (p1.GameOver && p2.GameOver)
            {
                if (p1.Score > p2.Score) return "PLAYER 1";
                if (p2.Score > p1.Score) return "PLAYER 2";
                return L.Get(L.TP_TIE);
            }
            if (p1.GameOver) return "PLAYER 2";
            if (p2.GameOver) return "PLAYER 1";
            return L.Get(L.TP_PLAYING);
        }

        private int WriteSeg(int startPos, string text, ConsoleColor fg)
        {
            for (int i = 0; i < text.Length && startPos + i < CONSOLE_W; i++)
                _buf.WriteToBuffer(startPos + i, FOOTER_Y, text[i], fg);
            return Math.Min(startPos + text.Length, CONSOLE_W);
        }
    }

    public enum GameOverMenuAction { None, Restart, Exit }
}
