using FlappyBird.Localization;
using FlappyBird.Models;
using FlappyBird.Settings;

namespace FlappyBird.Game.Modes.TwoPlayer
{
    /// <summary>
    /// Game Over for TwoPlayer — 4-row bordered panel at rows 20-23,
    /// matching SinglePlayer's footer visual structure.
    /// </summary>
    public class TwoPlayerGameOverMenu(TwoPlayerBuffer buffer)
    {
        private int ConsoleW   => _buf.Width;
        // Footer occupies the bottom 4 rows: top border, result, options, bottom border.
        private int FooterTopY => _buf.Height - 4; // row 20

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
        /// 4-row game over footer (rows 20-23):
        ///   Row 20: ╔══ red border ══╗
        ///   Row 21: ║  [winner / TIE result]  ║
        ///   Row 22: ║  ► Play Again  │  Main Menu  ║
        ///   Row 23: ╚══════════════════════════════╝
        /// </summary>
        public void RenderGameOverMenuToBuffer(GameState p1, GameState p2)
        {
            bool   isTie    = p1.GameOver && p2.GameOver && p1.Score == p2.Score;
            string winner   = GetWinner(p1, p2);
            string pts      = L.Get(L.GO_POINTS);
            var    opts     = Options;
            var    winColor = isTie ? ConsoleColor.Yellow : ConsoleColor.Green;
            var    bc       = ConsoleColor.Red;
            int    ftop     = FooterTopY;

            var bs = GameSettings.Instance.GetBorderSet();

            // Row 20: top border (red, style from theme)
            WriteBorderRow(ftop, bs.TopLeft, bs.TopRight, bc);

            // Row 21: result info
            string result = isTie
                ? $"  TIE!  P1: {p1.Score,3} {pts}  │  P2: {p2.Score,3} {pts}"
                : $"  {winner} wins  │  P1: {p1.Score,3} {pts}  │  P2: {p2.Score,3} {pts}";
            WriteContentRow(ftop + 1, result, winColor, bc);

            // Row 22: options (two-tone: selected=yellow, unselected=gray)
            bool sel0 = _selectedIndex == 0;
            bool sel1 = _selectedIndex == 1;
            string opt0 = (sel0 ? " ► " : "   ") + opts[0];
            string opt1 = (sel1 ? " ► " : "   ") + opts[1];
            WriteOptionsRow(ftop + 2, opt0, sel0, opt1, sel1, bc);

            // Row 23: bottom border (red, style from theme)
            WriteBorderRow(ftop + 3, bs.BotLeft, bs.BotRight, bc);
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

        private void WriteBorderRow(int row, char left, char right, ConsoleColor c)
        {
            var bs = GameSettings.Instance.GetBorderSet();
            int w  = ConsoleW;
            _buf.WriteToBuffer(0, row, left, c);
            for (int x = 1; x < w - 1; x++) _buf.WriteToBuffer(x, row, bs.Horiz, c);
            _buf.WriteToBuffer(w - 1, row, right, c);
        }

        private void WriteContentRow(int row, string text, ConsoleColor fg, ConsoleColor bc)
        {
            var bs = GameSettings.Instance.GetBorderSet();
            int w  = ConsoleW;
            _buf.WriteToBuffer(0, row, bs.Vert, bc);
            for (int x = 1; x < w - 1; x++)
            {
                int ti = x - 1;
                _buf.WriteToBuffer(x, row,
                    ti < text.Length ? text[ti] : ' ',
                    ti < text.Length ? fg : ConsoleColor.DarkGray);
            }
            _buf.WriteToBuffer(w - 1, row, bs.Vert, bc);
        }

        private void WriteOptionsRow(int row, string opt0, bool sel0, string opt1, bool sel1, ConsoleColor bc)
        {
            var bs = GameSettings.Instance.GetBorderSet();
            int w     = ConsoleW;
            int inner = w - 2;
            int leftW = inner / 2;           // 39 at width=80
            int sepCol = 1 + leftW;          // 40
            int rightStart = sepCol + 1;     // 41

            _buf.WriteToBuffer(0, row, bs.Vert, bc);

            // Left option
            var c0 = sel0 ? ConsoleColor.Yellow : ConsoleColor.Gray;
            for (int i = 0; i < leftW; i++)
            {
                char ch = i < opt0.Length ? opt0[i] : ' ';
                _buf.WriteToBuffer(1 + i, row, ch, i < opt0.Length ? c0 : ConsoleColor.DarkGray);
            }

            // Separator
            _buf.WriteToBuffer(sepCol, row, '│', ConsoleColor.White);

            // Right option
            var c1 = sel1 ? ConsoleColor.Yellow : ConsoleColor.Gray;
            int rightW = w - 2 - leftW - 1; // 38 at width=80
            for (int i = 0; i < rightW; i++)
            {
                char ch = i < opt1.Length ? opt1[i] : ' ';
                _buf.WriteToBuffer(rightStart + i, row, ch, i < opt1.Length ? c1 : ConsoleColor.DarkGray);
            }

            _buf.WriteToBuffer(w - 1, row, bs.Vert, bc);
        }
    }

    public enum GameOverMenuAction { None, Restart, Exit }
}
