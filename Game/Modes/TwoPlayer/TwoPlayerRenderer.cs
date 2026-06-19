using FlappyBird.Localization;
using FlappyBird.Models;

namespace FlappyBird.Game.Modes.TwoPlayer
{
    /// <summary>
    /// Renderer cho TwoPlayer – cùng bảng ký tự và kỹ thuật SinglePlayerRenderer:
    ///   • Pre-allocated buffers, Buffer.BlockCopy background
    ///   • Ống cap ▀/▄, thân 3-col
    ///   • Chim ♦/^/v + cánh ~/_
    ///   • Màu nhất quán: Cyan=viền, Green=ống, Yellow=chim, DarkGray=nền
    /// </summary>
    public class TwoPlayerRenderer
    {
        // ── LAYOUT ──────────────────────────────────────────────────────────
        private const int BORDER_W = 66;
        private const int GAME_DISPLAY_H = 11;      // nén từ 22 game rows
        private const int GAME_CONTENT_W = BORDER_W - 2; // 64
        private const int PANEL_H = 15;             // header(3) + game(11) + border(1)
        private const int TOTAL_H = 36;             // 2*PANEL_H + footer(6)

        // ── CHARACTERS ──────────────────────────────────────────────────────
        private const char PipeChar = '█';
        private const char BirdChar = '♦';
        private const char BgDot = '·';

        private readonly TwoPlayerBuffer _buf;

        // Pre-allocated game content buffers – không new mỗi frame
        private readonly char[,] _p1 = new char[GAME_DISPLAY_H, GAME_CONTENT_W];
        private readonly char[,] _p2 = new char[GAME_DISPLAY_H, GAME_CONTENT_W];
        private readonly char[,] _bg = new char[GAME_DISPLAY_H, GAME_CONTENT_W];

        public TwoPlayerRenderer(TwoPlayerBuffer buf)
        {
            _buf = buf;
            // Pre-compute background một lần – BlockCopy vào p1/p2 mỗi frame thay vì loop
            for (int y = 0; y < GAME_DISPLAY_H; y++)
                for (int x = 0; x < GAME_CONTENT_W; x++)
                    _bg[y, x] = (x + y) % 8 == 0 ? BgDot : ' ';
        }

        // ── PUBLIC API ───────────────────────────────────────────────────────

        public void RenderDualStackedScreensToBuffer(GameState p1, GameState p2)
        {
            RenderPlayerPanel(p1, "PLAYER 1", 0, _p1);
            RenderPlayerPanel(p2, "PLAYER 2", PANEL_H, _p2);
        }

        public void RenderDualPlayerFooterToBuffer(GameState p1, GameState p2)
        {
            int fy = TOTAL_H - 6;
            WriteBorder(fy, '╔', '═', '╗', ConsoleColor.Yellow);
            WriteInfo(fy + 1, BuildScoreLine(p1, p2), ConsoleColor.White, ConsoleColor.Yellow);
            WriteBorder(fy + 2, '╠', '═', '╣', ConsoleColor.Cyan);
            WriteInfo(fy + 3,
                $" {L.Get(L.TP_JUMP_P1)}  │  {L.Get(L.TP_JUMP_P2)}  │  [ESC] {L.Get(L.CTRL_EXIT)[5..]}",
                ConsoleColor.Gray, ConsoleColor.Cyan);
            WriteInfo(fy + 4,
                $" {L.Get(L.TP_START_RESTART)}",
                ConsoleColor.Gray, ConsoleColor.Cyan);
            WriteBorder(fy + 5, '╚', '═', '╝', ConsoleColor.Cyan);
        }

        /// <summary>Flash "GAME OVER" tại ranh giới giữa 2 panel (trước khi menu hiện)</summary>
        public void RenderGameOverOverlayToBuffer()
        {
            const string txt = "  *** GAME OVER! ***  ";
            int sx = (BORDER_W - txt.Length) / 2;
            for (int x = 0; x < BORDER_W; x++)
                _buf.WriteToBuffer(x, PANEL_H, ' ', ConsoleColor.Red);
            for (int i = 0; i < txt.Length; i++)
                _buf.WriteToBuffer(sx + i, PANEL_H, txt[i], ConsoleColor.Red);
        }

        public void RenderCountdownOverlayToBuffer(int value)
        {
            // 5-row compact digits – khớp chiều cao GAME_DISPLAY_H=11
            string[] digit = value switch
            {
                3 => ["  ████", "     █", "  ████", "     █", "  ████"],
                2 => [" █████", "     █", " █████", " █    ", " █████"],
                1 => ["  ██  ", "   █  ", "   █  ", "   █  ", " █████"],
                _ => [" ████ ", " █  █ ", " ████ ", " █  █ ", " ████ "]
            };
            ConsoleColor color = value == 1
                ? (DateTime.Now.Millisecond < 500 ? ConsoleColor.Red : ConsoleColor.Yellow)
                : value <= 0 ? ConsoleColor.Green : ConsoleColor.Yellow;

            OverlayDigit(digit, 3, color);              // P1 game area từ row 3
            OverlayDigit(digit, PANEL_H + 3, color);    // P2 game area từ row PANEL_H+3
        }

        // ── PRIVATE – PANEL ──────────────────────────────────────────────────

        private void RenderPlayerPanel(GameState gs, string label, int panelY, char[,] gameBuf)
        {
            WriteBorder(panelY, '╔', '═', '╗', ConsoleColor.Cyan);
            string info = gs.GameOver
                ? $" {label}  │  Score: {gs.Score,3}  │  Level: {gs.DifficultyLevel,2}  │  [{L.Get(L.TP_OUT)}]"
                : $" {label}  │  Score: {gs.Score,3}  │  Level: {gs.DifficultyLevel,2}";
            WriteInfo(panelY + 1, info, ConsoleColor.White, ConsoleColor.Cyan);
            WriteBorder(panelY + 2, '╠', '═', '╣', ConsoleColor.Cyan);

            BuildGameContent(gameBuf, gs);
            FlushContent(gameBuf, panelY + 3);

            WriteBorder(panelY + 3 + GAME_DISPLAY_H, '╚', '═', '╝', ConsoleColor.Cyan);
        }

        /// <summary>
        /// Xây dựng nội dung game vào buffer char[11][64]:
        ///   1) BlockCopy background  2) Vẽ ống  3) Vẽ chim
        /// </summary>
        private void BuildGameContent(char[,] buf, GameState gs)
        {
            // 1. Background từ pre-computed (một lần copy, không loop)
            Buffer.BlockCopy(_bg, 0, buf, 0, GAME_DISPLAY_H * GAME_CONTENT_W * sizeof(char));

            // 2. Pipes – cùng chất lượng SinglePlayer nhưng scale Y 22→11
            foreach (var pipe in gs.Pipes)
            {
                int cx = pipe.X - 1;
                if (cx < 0 || cx >= GAME_CONTENT_W) continue;

                int topH = ScaleY(pipe.TopHeight);
                int botCap = ScaleY(GameState.GameHeight - pipe.BottomHeight - 1);

                // Thân ống trên
                for (int y = 0; y < topH && y < GAME_DISPLAY_H; y++)
                    DrawPipeRow(buf, y, cx);
                // Cap ống trên ▀
                if (topH < GAME_DISPLAY_H)
                    DrawCapRow(buf, topH, cx, '▀');
                // Cap ống dưới ▄
                if (botCap > topH && botCap < GAME_DISPLAY_H)
                    DrawCapRow(buf, botCap, cx, '▄');
                // Thân ống dưới
                for (int y = botCap + 1; y < GAME_DISPLAY_H; y++)
                    DrawPipeRow(buf, y, cx);
            }

            // 3. Chim – cùng char và cánh như SinglePlayerRenderer
            int bx = GameState.BirdX - 1;
            int by = ScaleY(gs.BirdY);
            if ((uint)bx < GAME_CONTENT_W && (uint)by < GAME_DISPLAY_H)
            {
                buf[by, bx] = gs.BirdVelocity < -0.12f ? '^'
                             : gs.BirdVelocity > 0.12f ? 'v'
                             : BirdChar;
                if (bx - 1 >= 0)
                {
                    gs.BirdAnimationFrame = (gs.BirdAnimationFrame + 1) % 4;
                    buf[by, bx - 1] = gs.BirdAnimationFrame < 2 ? '~' : '_';
                }
            }
        }

        private static void DrawPipeRow(char[,] buf, int y, int cx)
        {
            if (cx - 1 >= 0) buf[y, cx - 1] = PipeChar;
            buf[y, cx] = PipeChar;
            if (cx + 1 < GAME_CONTENT_W) buf[y, cx + 1] = PipeChar;
        }

        private static void DrawCapRow(char[,] buf, int y, int cx, char cap)
        {
            for (int dx = -2; dx <= 2; dx++)
            {
                int x = cx + dx;
                if ((uint)x < GAME_CONTENT_W) buf[y, x] = cap;
            }
        }

        private void FlushContent(char[,] buf, int bufY)
        {
            for (int y = 0; y < GAME_DISPLAY_H; y++)
            {
                _buf.WriteToBuffer(0, bufY + y, '║', ConsoleColor.Cyan);
                for (int x = 0; x < GAME_CONTENT_W; x++)
                    _buf.WriteToBuffer(x + 1, bufY + y, buf[y, x], CharColor(buf[y, x]));
                _buf.WriteToBuffer(BORDER_W - 1, bufY + y, '║', ConsoleColor.Cyan);
            }
        }

        private void OverlayDigit(string[] rows, int gameAreaY, ConsoleColor color)
        {
            int startY = gameAreaY + (GAME_DISPLAY_H - rows.Length) / 2;
            int startX = BORDER_W / 2 - (rows[0].Length) / 2;
            for (int r = 0; r < rows.Length; r++)
                for (int c = 0; c < rows[r].Length; c++)
                    if (rows[r][c] != ' ')
                        _buf.WriteToBuffer(startX + c, startY + r, rows[r][c], color);
        }

        // ── HELPERS ──────────────────────────────────────────────────────────

        /// <summary>Scale Y: game(0..21) → display(0..10), scale = 0.5</summary>
        private static int ScaleY(int gy) =>
            (int)(gy * GAME_DISPLAY_H / (float)GameState.GameHeight);

        private static ConsoleColor CharColor(char ch) => ch switch
        {
            '█' => ConsoleColor.Green,
            '▀' or '▄' => ConsoleColor.DarkGreen,
            '♦' or '^' or 'v' => ConsoleColor.Yellow,
            '~' or '_' => ConsoleColor.DarkYellow,
            '·' => ConsoleColor.DarkGray,
            _ => ConsoleColor.White
        };

        private static string BuildScoreLine(GameState p1, GameState p2)
        {
            string s1 = p1.GameOver ? $"P1: {p1.Score,3} (OUT)" : $"P1: {p1.Score,3}";
            string s2 = p2.GameOver ? $"P2: {p2.Score,3} (OUT)" : $"P2: {p2.Score,3}";
            return $" {s1}  │  {s2}";
        }

        private void WriteBorder(int y, char l, char m, char r, ConsoleColor c)
        {
            _buf.WriteToBuffer(0, y, l, c);
            for (int x = 1; x < BORDER_W - 1; x++)
                _buf.WriteToBuffer(x, y, m, c);
            _buf.WriteToBuffer(BORDER_W - 1, y, r, c);
        }

        private void WriteInfo(int y, string text, ConsoleColor fg, ConsoleColor borderColor)
        {
            _buf.WriteToBuffer(0, y, '║', borderColor);
            for (int i = 0; i < BORDER_W - 2; i++)
                _buf.WriteToBuffer(i + 1, y, i < text.Length ? text[i] : ' ', fg);
            _buf.WriteToBuffer(BORDER_W - 1, y, '║', borderColor);
        }
    }
}
