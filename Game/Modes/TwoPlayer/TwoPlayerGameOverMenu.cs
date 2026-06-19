using FlappyBird.Localization;
using FlappyBird.Models;

namespace FlappyBird.Game.Modes.TwoPlayer
{
    /// <summary>
    /// Game Over menu cho TwoPlayer – hiển thị tại footer (rows 30-35),
    /// nhất quán với SinglePlayer game over layout và dùng TwoPlayerBuffer.
    /// </summary>
    public class TwoPlayerGameOverMenu(TwoPlayerBuffer buffer)
    {
        // ── LAYOUT ──────────────────────────────────────────────────────────
        private const int BORDER_W = TwoPlayerBuffer.MENU_BORDER_WIDTH;      // 66
        private const int TOTAL_H = TwoPlayerBuffer.TOTAL_DISPLAY_HEIGHT;    // 36
        private const int FOOTER_H = 6;
        private const int FOOTER_Y = TOTAL_H - FOOTER_H;                     // 30

        private readonly TwoPlayerBuffer _buf = buffer;

        private bool _show = false;
        private int _selectedIndex = 0;
        private DateTime _startTime = DateTime.MinValue;

        private static string[] Options => [L.Get(L.GO_PLAY_AGAIN), L.Get(L.GO_MAIN_MENU)];

        public bool ShowGameOverMenu => _show;
        public DateTime GameOverTime => _startTime;

        public void StartGameOverMenu()
        {
            _show = true;
            _selectedIndex = 0;
            _startTime = DateTime.Now;
        }

        public void ResetGameOverMenu()
        {
            _show = false;
            _selectedIndex = 0;
            _startTime = DateTime.MinValue;
        }

        public bool CanReceiveInput() =>
            _show && DateTime.Now - _startTime > TimeSpan.FromMilliseconds(800);

        // ── RENDER ──────────────────────────────────────────────────────────

        /// <summary>
        /// Viết game over panel vào buffer tại rows FOOTER_Y..FOOTER_Y+5.
        /// Gọi sau khi renderer đã vẽ 2 game panel để không bị đè.
        /// </summary>
        public void RenderGameOverMenuToBuffer(GameState p1, GameState p2)
        {
            string winner = GetWinner(p1, p2);
            bool isTie = p1.GameOver && p2.GameOver && p1.Score == p2.Score;
            ConsoleColor winColor = isTie ? ConsoleColor.Yellow : ConsoleColor.Green;
            string pts = L.Get(L.GO_POINTS);

            // Row 0: top border  ╔══...══╗  (Red)
            WriteBorder(FOOTER_Y, '╔', '═', '╗', ConsoleColor.Red);

            // Row 1: winner + scores
            string info = isTie
                ? $"  {L.Get(L.TP_TIE_EXCLAIM)}  │  P1: {p1.Score,3} {pts}  │  P2: {p2.Score,3} {pts}"
                : $"  {L.Get(L.TP_WINNER)}: {winner}  │  P1: {p1.Score,3} {pts}  │  P2: {p2.Score,3} {pts}";
            WriteRow(FOOTER_Y + 1, info, winColor, ConsoleColor.Red);

            // Row 2: separator  ╠══...══╣  (Cyan)
            WriteBorder(FOOTER_Y + 2, '╠', '═', '╣', ConsoleColor.Cyan);

            // Row 3: menu options
            string opt0 = _selectedIndex == 0 ? $"> {Options[0],-30}" : $"  {Options[0],-30}";
            string opt1 = _selectedIndex == 1 ? $"> {Options[1],-28}" : $"  {Options[1],-28}";
            string opts = $"  {opt0}  │  {opt1}";
            WriteRowWithSelection(FOOTER_Y + 3, opts, _selectedIndex, ConsoleColor.Cyan);

            // Row 4: controls hint
            WriteRow(FOOTER_Y + 4, $"  {L.Get(L.TP_GO_CONTROLS)}", ConsoleColor.Gray, ConsoleColor.Cyan);

            // Row 5: bottom border  ╚══...══╝  (Cyan)
            WriteBorder(FOOTER_Y + 5, '╚', '═', '╝', ConsoleColor.Cyan);
        }

        // ── INPUT ────────────────────────────────────────────────────────────

        public GameOverMenuAction HandleGameOverMenuInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.DownArrow:
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

        // ── HELPERS ──────────────────────────────────────────────────────────

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

        private void WriteBorder(int y, char l, char m, char r, ConsoleColor c)
        {
            _buf.WriteToBuffer(0, y, l, c);
            for (int x = 1; x < BORDER_W - 1; x++)
                _buf.WriteToBuffer(x, y, m, c);
            _buf.WriteToBuffer(BORDER_W - 1, y, r, c);
        }

        private void WriteRow(int y, string text, ConsoleColor fg, ConsoleColor borderColor)
        {
            _buf.WriteToBuffer(0, y, '║', borderColor);
            for (int i = 0; i < BORDER_W - 2; i++)
                _buf.WriteToBuffer(i + 1, y, i < text.Length ? text[i] : ' ', fg);
            _buf.WriteToBuffer(BORDER_W - 1, y, '║', borderColor);
        }

        private void WriteRowWithSelection(int y, string text, int sel, ConsoleColor borderColor)
        {
            _buf.WriteToBuffer(0, y, '║', borderColor);
            // Option 0 occupies roughly left half, option 1 right half
            // Just render text but highlight the selected option's ">" marker in Yellow
            for (int i = 0; i < BORDER_W - 2; i++)
            {
                char ch = i < text.Length ? text[i] : ' ';
                ConsoleColor fg = (i < (BORDER_W - 2) / 2 && sel == 0 && text.StartsWith("  >"))
                                  || (i >= (BORDER_W - 2) / 2 && sel == 1)
                    ? ConsoleColor.Yellow
                    : ConsoleColor.White;
                _buf.WriteToBuffer(i + 1, y, ch, fg);
            }
            _buf.WriteToBuffer(BORDER_W - 1, y, '║', borderColor);
        }
    }

    public enum GameOverMenuAction { None, Restart, Exit }
}
