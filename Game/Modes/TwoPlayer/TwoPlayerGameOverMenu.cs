using FlappyBird.Localization;
using FlappyBird.Models;
using FlappyBird.Settings;

namespace FlappyBird.Game.Modes.TwoPlayer
{
    /// <summary>
    /// Game Over panel for TwoPlayer — rendered at the footer (rows FOOTER_Y … FOOTER_Y+5)
    /// via TwoPlayerBuffer diff-write (not Console.Write directly).
    /// </summary>
    public class TwoPlayerGameOverMenu(TwoPlayerBuffer buffer)
    {
        // ── Layout ──────────────────────────────────────────────────────────
        private const int BORDER_W = TwoPlayerBuffer.MENU_BORDER_WIDTH;   // 66
        private const int TOTAL_H  = TwoPlayerBuffer.TOTAL_DISPLAY_HEIGHT; // 36
        private const int FOOTER_H = 4;
        private const int FOOTER_Y = TOTAL_H - FOOTER_H;                  // 20

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

        public void RenderGameOverMenuToBuffer(GameState p1, GameState p2)
        {
            bool   isTie    = p1.GameOver && p2.GameOver && p1.Score == p2.Score;
            string winner   = GetWinner(p1, p2);
            string pts      = L.Get(L.GO_POINTS);
            var    bs       = GameSettings.Instance.GetBorderSet();
            var    winColor = isTie ? ConsoleColor.Yellow : ConsoleColor.Green;

            // Row 0: top border ╔══╗ (Red)
            WriteBorder(FOOTER_Y, bs.TopLeft, bs.Horiz, bs.TopRight, ConsoleColor.Red);

            // Row 1: winner / tie headline + scores
            string headline = isTie
                ? $"  {L.Get(L.TP_TIE_EXCLAIM)}  │  P1: {p1.Score,3} {pts}  │  P2: {p2.Score,3} {pts}"
                : $"  {L.Get(L.TP_WINNER)}: {winner}  │  P1: {p1.Score,3} {pts}  │  P2: {p2.Score,3} {pts}";
            WriteRow(FOOTER_Y + 1, headline, winColor, ConsoleColor.Red);

            // Row 2: options + controls hint
            var opts = Options;
            string opt0 = _selectedIndex == 0 ? $"> {opts[0],-20}" : $"  {opts[0],-20}";
            string opt1 = _selectedIndex == 1 ? $"> {opts[1],-18}" : $"  {opts[1],-18}";
            WriteRowWithSelection(FOOTER_Y + 2,
                $"  {opt0}  │  {opt1}  │  {L.Get(L.TP_GO_CONTROLS)}", _selectedIndex, winColor);

            // Row 3: bottom border ╚══╝
            WriteBorder(FOOTER_Y + 3, bs.BotLeft, bs.Horiz, bs.BotRight, winColor);
        }

        // ── Input ────────────────────────────────────────────────────────────

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

        private void WriteBorder(int y, char l, char m, char r, ConsoleColor c)
        {
            _buf.WriteToBuffer(0, y, l, c);
            for (int x = 1; x < BORDER_W - 1; x++)
                _buf.WriteToBuffer(x, y, m, c);
            _buf.WriteToBuffer(BORDER_W - 1, y, r, c);
        }

        private void WriteRow(int y, string text, ConsoleColor fg, ConsoleColor borderColor)
        {
            var bs = GameSettings.Instance.GetBorderSet();
            _buf.WriteToBuffer(0, y, bs.Vert, borderColor);
            for (int i = 0; i < BORDER_W - 2; i++)
                _buf.WriteToBuffer(i + 1, y, i < text.Length ? text[i] : ' ', fg);
            _buf.WriteToBuffer(BORDER_W - 1, y, bs.Vert, borderColor);
        }

        private void WriteRowWithSelection(int y, string text, int sel, ConsoleColor borderColor)
        {
            var bs = GameSettings.Instance.GetBorderSet();
            _buf.WriteToBuffer(0, y, bs.Vert, borderColor);
            int half = (BORDER_W - 2) / 2;
            for (int i = 0; i < BORDER_W - 2; i++)
            {
                char ch = i < text.Length ? text[i] : ' ';
                bool isLeft  = i < half;
                ConsoleColor fg =
                    (isLeft  && sel == 0 && text.StartsWith("  >")) ||
                    (!isLeft && sel == 1)
                        ? ConsoleColor.Yellow
                        : ConsoleColor.White;
                _buf.WriteToBuffer(i + 1, y, ch, fg);
            }
            _buf.WriteToBuffer(BORDER_W - 1, y, bs.Vert, borderColor);
        }
    }

    public enum GameOverMenuAction { None, Restart, Exit }
}
