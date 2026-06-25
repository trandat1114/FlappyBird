using FlappyBird.Models;
using FlappyBird.Settings;

namespace FlappyBird.Game.Modes.TwoPlayer
{
    /// <summary>
    /// Renderer TwoPlayer — side-by-side layout (80×24).
    /// P1: cols 0-39, P2: cols 40-79, footer: row 23.
    /// Game area per panel: rows 1-21 (21 rows, ≈ 1:1 with GameHeight=20).
    /// </summary>
    public class TwoPlayerRenderer
    {
        // ── LAYOUT ──────────────────────────────────────────────────────────
        private const int CONSOLE_W   = TwoPlayerBuffer.CONSOLE_WIDTH;         // 80
        private const int PANEL_W     = CONSOLE_W / 2;                         // 40
        private const int INNER_W     = PANEL_W - 2;                           // 38
        private const int GAME_DISP_H = 21;                                    // rows 1..21 inside panel
        private const int FOOTER_Y    = TwoPlayerBuffer.TOTAL_DISPLAY_HEIGHT - 1; // 23
        private const int P2_X        = PANEL_W;                               // P2 starts at col 40

        // ── CHARACTERS ──────────────────────────────────────────────────────
        private const char PipeChar = '█';
        private const char BirdChar = '♦';
        private const char BgDot    = '·';

        private readonly TwoPlayerBuffer _buf;

        private readonly char[,] _p1 = new char[GAME_DISP_H, INNER_W];
        private readonly char[,] _p2 = new char[GAME_DISP_H, INNER_W];
        private readonly char[,] _bg = new char[GAME_DISP_H, INNER_W];

        public TwoPlayerRenderer(TwoPlayerBuffer buf)
        {
            _buf = buf;
            for (int y = 0; y < GAME_DISP_H; y++)
                for (int x = 0; x < INNER_W; x++)
                    _bg[y, x] = (x + y) % 8 == 0 ? BgDot : ' ';
        }

        // ── PUBLIC API ───────────────────────────────────────────────────────

        public void RenderDualSideBySideToBuffer(GameState p1, GameState p2)
        {
            RenderPlayerPanel(p1, "PLAYER 1", 0,    _p1);
            RenderPlayerPanel(p2, "PLAYER 2", P2_X, _p2);
        }

        public void RenderDualPlayerFooterToBuffer(GameState p1, GameState p2)
        {
            string s1 = p1.GameOver
                ? $"P1:{p1.Score,3}pts (OUT)"
                : $"P1:{p1.Score,3}pts Lv.{p1.DifficultyLevel}";
            string s2 = p2.GameOver
                ? $"P2:{p2.Score,3}pts (OUT)"
                : $"P2:{p2.Score,3}pts Lv.{p2.DifficultyLevel}";
            string line = $" {s1}  │  {s2}  │  [W]/[↑] Jump  [SPACE] Start  [ESC]";
            WriteFooterRow(line, ConsoleColor.White);
        }

        public void RenderCountdownOverlayToBuffer(int value)
        {
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

            OverlayDigit(digit, 0,    color);
            OverlayDigit(digit, P2_X, color);
        }

        // ── PRIVATE – PANEL ──────────────────────────────────────────────────

        private void RenderPlayerPanel(GameState gs, string label, int xOff, char[,] gameBuf)
        {
            var pc = GameSettings.Instance.PrimaryColor;

            // Row 0: top border with centred label
            string topInner = BuildTopInner(label);
            _buf.WriteToBuffer(xOff,               0, '╔', pc);
            for (int x = 0; x < INNER_W; x++)
                _buf.WriteToBuffer(xOff + 1 + x,   0, topInner[x], pc);
            _buf.WriteToBuffer(xOff + PANEL_W - 1, 0, '╗', pc);

            // Rows 1..GAME_DISP_H: game content
            BuildGameContent(gameBuf, gs);
            FlushContent(gameBuf, xOff);

            // Overlay "GAME OVER!" at vertical centre when player is dead
            if (gs.GameOver)
            {
                int midRow = 1 + GAME_DISP_H / 2;  // row 11
                WriteOverlay(xOff + 1, midRow, INNER_W, "GAME OVER!", ConsoleColor.Red);
            }

            // Row 22: bottom border
            _buf.WriteToBuffer(xOff,               22, '╚', pc);
            for (int x = 1; x < PANEL_W - 1; x++)
                _buf.WriteToBuffer(xOff + x,       22, '═', pc);
            _buf.WriteToBuffer(xOff + PANEL_W - 1, 22, '╝', pc);
        }

        private static string BuildTopInner(string label)
        {
            string lbl     = $" {label} ";
            int    padLeft  = (INNER_W - lbl.Length) / 2;
            int    padRight = INNER_W - padLeft - lbl.Length;
            return new string('═', padLeft) + lbl + new string('═', padRight);
        }

        // ── BUILD GAME CONTENT ────────────────────────────────────────────────

        private void BuildGameContent(char[,] buf, GameState gs)
        {
            Buffer.BlockCopy(_bg, 0, buf, 0, GAME_DISP_H * INNER_W * sizeof(char));

            foreach (var pipe in gs.Pipes)
            {
                int cx = ScaleX(pipe.X);
                if (cx < 0 || cx >= INNER_W) continue;

                int topH   = ScaleY(pipe.TopHeight);
                int botCap = ScaleY(GameState.GameHeight - pipe.BottomHeight - 1);

                for (int y = 0; y < topH && y < GAME_DISP_H; y++)
                    DrawPipeRow(buf, y, cx);
                if (topH < GAME_DISP_H)
                    DrawCapRow(buf, topH, cx, '▀');
                if (botCap > topH && botCap < GAME_DISP_H)
                    DrawCapRow(buf, botCap, cx, '▄');
                for (int y = botCap + 1; y < GAME_DISP_H; y++)
                    DrawPipeRow(buf, y, cx);
            }

            int bx = ScaleX(GameState.BirdX);
            int by = ScaleY(gs.BirdY);
            if ((uint)bx < INNER_W && (uint)by < GAME_DISP_H)
            {
                buf[by, bx] = gs.BirdVelocity < -0.12f ? '^'
                             : gs.BirdVelocity >  0.12f ? 'v'
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
            if (cx - 1 >= 0)      buf[y, cx - 1] = PipeChar;
            buf[y, cx]            = PipeChar;
            if (cx + 1 < INNER_W) buf[y, cx + 1] = PipeChar;
        }

        private static void DrawCapRow(char[,] buf, int y, int cx, char cap)
        {
            for (int dx = -2; dx <= 2; dx++)
            {
                int x = cx + dx;
                if ((uint)x < INNER_W) buf[y, x] = cap;
            }
        }

        private void FlushContent(char[,] buf, int xOff)
        {
            var pc = GameSettings.Instance.PrimaryColor;
            for (int y = 0; y < GAME_DISP_H; y++)
            {
                _buf.WriteToBuffer(xOff,               y + 1, '║', pc);
                for (int x = 0; x < INNER_W; x++)
                    _buf.WriteToBuffer(xOff + 1 + x,   y + 1, buf[y, x], CharColor(buf[y, x]));
                _buf.WriteToBuffer(xOff + PANEL_W - 1, y + 1, '║', pc);
            }
        }

        // ── OVERLAYS ─────────────────────────────────────────────────────────

        private void WriteOverlay(int xStart, int row, int width, string text, ConsoleColor color)
        {
            int pad = (width - text.Length) / 2;
            for (int i = 0; i < width; i++)
            {
                int ti = i - pad;
                char ch = (ti >= 0 && ti < text.Length) ? text[ti] : ' ';
                _buf.WriteToBuffer(xStart + i, row, ch, color);
            }
        }

        private void OverlayDigit(string[] rows, int panelXOff, ConsoleColor color)
        {
            int startY = 1 + (GAME_DISP_H - rows.Length) / 2;
            int startX = panelXOff + 1 + (INNER_W - rows[0].Length) / 2;
            for (int r = 0; r < rows.Length; r++)
                for (int c = 0; c < rows[r].Length; c++)
                    if (rows[r][c] != ' ')
                        _buf.WriteToBuffer(startX + c, startY + r, rows[r][c], color);
        }

        private void WriteFooterRow(string text, ConsoleColor fg)
        {
            for (int i = 0; i < CONSOLE_W; i++)
                _buf.WriteToBuffer(i, FOOTER_Y,
                    i < text.Length ? text[i] : ' ',
                    i < text.Length ? fg : ConsoleColor.DarkGray);
        }

        // ── HELPERS ──────────────────────────────────────────────────────────

        private static int ScaleX(int gx) => (int)(gx * INNER_W / (float)GameState.GameWidth);
        private static int ScaleY(int gy) => (int)(gy * GAME_DISP_H / (float)GameState.GameHeight);

        private static ConsoleColor CharColor(char ch) => ch switch
        {
            '█'                   => ConsoleColor.Green,
            '▀' or '▄'           => ConsoleColor.DarkGreen,
            '♦' or '^' or 'v'    => ConsoleColor.Yellow,
            '~' or '_'           => ConsoleColor.DarkYellow,
            '·'                   => ConsoleColor.DarkGray,
            _                     => ConsoleColor.White
        };
    }
}
