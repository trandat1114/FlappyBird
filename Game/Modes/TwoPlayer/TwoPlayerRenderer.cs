using FlappyBird.Models;
using FlappyBird.Settings;

namespace FlappyBird.Game.Modes.TwoPlayer
{
    /// <summary>
    /// Renderer TwoPlayer — side-by-side layout, fully responsive.
    /// Layout (24 rows total, same visual footprint as SinglePlayer):
    ///   Row  0        : top border with PLAYER 1 / PLAYER 2 labels
    ///   Rows 1–18     : game content (18 rows = SP interior)
    ///   Row  19       : bottom border
    ///   Row  20       : footer top border    ╔═══╗
    ///   Row  21       : P1/P2 status         ║...║
    ///   Row  22       : controls             ║...║
    ///   Row  23       : footer bottom border ╚═══╝
    /// </summary>
    public class TwoPlayerRenderer
    {
        // ── LAYOUT ───────────────────────────────────────────────────────────
        private int ConsoleW   => _buf.Width;
        private int PanelW     => ConsoleW / 2;
        private int InnerW     => PanelW - 2;
        // GameDispH = SP interior (GameHeight-2=18) so both modes share the same visual height.
        private int GameDispH  => GameState.GameHeight - 2;  // 18 rows
        private int BotBorderY => GameDispH + 1;             // row 19
        private int FooterTopY => BotBorderY + 1;            // row 20
        private int P2X        => PanelW;

        // ── CHARACTERS ──────────────────────────────────────────────────────
        private const char PipeChar = '█';
        private const char BirdChar = '♦';
        private const char BgDot    = '·';

        private readonly TwoPlayerBuffer _buf;

        // Lazily allocated — reallocated when layout dimensions change
        private char[,] _p1 = new char[1, 1];
        private char[,] _p2 = new char[1, 1];
        private char[,] _bg = new char[1, 1];
        private int _lastGameDispH = -1, _lastInnerW = -1;

        public TwoPlayerRenderer(TwoPlayerBuffer buf) => _buf = buf;

        // ── PUBLIC API ───────────────────────────────────────────────────────

        public void RenderDualSideBySideToBuffer(GameState p1, GameState p2)
        {
            EnsureGameBuffers();
            RenderPlayerPanel(p1, "PLAYER 1", 0,   _p1);
            RenderPlayerPanel(p2, "PLAYER 2", P2X, _p2);
        }

        public void RenderDualPlayerFooterToBuffer(GameState p1, GameState p2)
        {
            var pc = GameSettings.Instance.PrimaryColor;

            // Row 20: top border
            WriteFooterBorderRow(FooterTopY, '╔', '╗', pc);

            // Row 21: P1/P2 status
            string s1 = p1.GameOver
                ? $"P1:{p1.Score,3}pts (OUT)"
                : $"P1:{p1.Score,3}pts Lv.{p1.DifficultyLevel}";
            string s2 = p2.GameOver
                ? $"P2:{p2.Score,3}pts (OUT)"
                : $"P2:{p2.Score,3}pts Lv.{p2.DifficultyLevel}";
            WriteFooterContentRow(FooterTopY + 1, $"  {s1}  │  {s2}", ConsoleColor.Yellow, pc);

            // Row 22: controls
            WriteFooterContentRow(FooterTopY + 2,
                "  [W]/[↑] Jump  │  [SPACE] Start  │  [ESC] Exit",
                ConsoleColor.Gray, pc);

            // Row 23: bottom border (buffer flush handles last-row write)
            WriteFooterBorderRow(FooterTopY + 3, '╚', '╝', pc);
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

            OverlayDigit(digit, 0,   color);
            OverlayDigit(digit, P2X, color);
        }

        // ── PRIVATE – PANEL ──────────────────────────────────────────────────

        private void RenderPlayerPanel(GameState gs, string label, int xOff, char[,] gameBuf)
        {
            int iw  = InnerW;
            int gdh = GameDispH;
            var pc  = GameSettings.Instance.PrimaryColor;

            // Row 0: top border with centred label
            string topInner = BuildTopInner(label, iw);
            _buf.WriteToBuffer(xOff,                  0, '╔', pc);
            for (int x = 0; x < iw; x++)
                _buf.WriteToBuffer(xOff + 1 + x,      0, topInner[x], pc);
            _buf.WriteToBuffer(xOff + PanelW - 1,     0, '╗', pc);

            // Rows 1..gdh: game content
            BuildGameContent(gameBuf, gs, gdh, iw);
            FlushContent(gameBuf, xOff, gdh, iw);

            if (gs.GameOver)
                WriteOverlay(xOff + 1, 1 + gdh / 2, iw, "GAME OVER!", ConsoleColor.Red);

            // Row 19: bottom border
            int botRow = BotBorderY;
            _buf.WriteToBuffer(xOff,                  botRow, '╚', pc);
            for (int x = 1; x < PanelW - 1; x++)
                _buf.WriteToBuffer(xOff + x,          botRow, '═', pc);
            _buf.WriteToBuffer(xOff + PanelW - 1,     botRow, '╝', pc);
        }

        private static string BuildTopInner(string label, int innerW)
        {
            string lbl      = $" {label} ";
            int    padLeft  = (innerW - lbl.Length) / 2;
            int    padRight = innerW - padLeft - lbl.Length;
            return new string('═', padLeft) + lbl + new string('═', padRight);
        }

        // ── GAME BUFFERS ─────────────────────────────────────────────────────

        private void EnsureGameBuffers()
        {
            int h = GameDispH, w = InnerW;
            if (h == _lastGameDispH && w == _lastInnerW) return;

            _p1 = new char[h, w];
            _p2 = new char[h, w];
            _bg = new char[h, w];
            _lastGameDispH = h;
            _lastInnerW    = w;

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    _bg[y, x] = (x + y) % 8 == 0 ? BgDot : ' ';
        }

        private void BuildGameContent(char[,] buf, GameState gs, int gdh, int iw)
        {
            Buffer.BlockCopy(_bg, 0, buf, 0, gdh * iw * sizeof(char));

            foreach (var pipe in gs.Pipes)
            {
                int cx = ScaleX(pipe.X, iw);
                if (cx < 0 || cx >= iw) continue;

                int topH   = ScaleY(pipe.TopHeight, gdh);
                int botCap = ScaleY(GameState.GameHeight - pipe.BottomHeight - 1, gdh);

                for (int y = 0; y < topH && y < gdh; y++)      DrawPipeRow(buf, y, cx, iw);
                if (topH < gdh)                                 DrawCapRow(buf, topH, cx, '▀', iw);
                if (botCap > topH && botCap < gdh)              DrawCapRow(buf, botCap, cx, '▄', iw);
                for (int y = botCap + 1; y < gdh; y++)         DrawPipeRow(buf, y, cx, iw);
            }

            int bx = ScaleX(GameState.BirdX, iw);
            int by = ScaleY(gs.BirdY, gdh);
            if ((uint)bx < iw && (uint)by < gdh)
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

        private static void DrawPipeRow(char[,] buf, int y, int cx, int iw)
        {
            if (cx - 1 >= 0)  buf[y, cx - 1] = PipeChar;
            buf[y, cx]        = PipeChar;
            if (cx + 1 < iw)  buf[y, cx + 1] = PipeChar;
        }

        private static void DrawCapRow(char[,] buf, int y, int cx, char cap, int iw)
        {
            for (int dx = -2; dx <= 2; dx++)
            {
                int x = cx + dx;
                if ((uint)x < iw) buf[y, x] = cap;
            }
        }

        private void FlushContent(char[,] buf, int xOff, int gdh, int iw)
        {
            var pc = GameSettings.Instance.PrimaryColor;
            for (int y = 0; y < gdh; y++)
            {
                _buf.WriteToBuffer(xOff,               y + 1, '║', pc);
                for (int x = 0; x < iw; x++)
                    _buf.WriteToBuffer(xOff + 1 + x,   y + 1, buf[y, x], CharColor(buf[y, x]));
                _buf.WriteToBuffer(xOff + PanelW - 1,  y + 1, '║', pc);
            }
        }

        // ── FOOTER HELPERS ───────────────────────────────────────────────────

        private void WriteFooterBorderRow(int row, char left, char right, ConsoleColor pc)
        {
            int w = ConsoleW;
            _buf.WriteToBuffer(0, row, left, pc);
            for (int x = 1; x < w - 1; x++) _buf.WriteToBuffer(x, row, '═', pc);
            _buf.WriteToBuffer(w - 1, row, right, pc);
        }

        private void WriteFooterContentRow(int row, string text, ConsoleColor fg, ConsoleColor pc)
        {
            int w = ConsoleW;
            _buf.WriteToBuffer(0, row, '║', pc);
            for (int x = 1; x < w - 1; x++)
            {
                int ti = x - 1;
                _buf.WriteToBuffer(x, row,
                    ti < text.Length ? text[ti] : ' ',
                    ti < text.Length ? fg : ConsoleColor.DarkGray);
            }
            _buf.WriteToBuffer(w - 1, row, '║', pc);
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
            int iw  = InnerW;
            int gdh = GameDispH;
            int startY = 1 + (gdh - rows.Length) / 2;
            int startX = panelXOff + 1 + (iw - rows[0].Length) / 2;
            for (int r = 0; r < rows.Length; r++)
                for (int c = 0; c < rows[r].Length; c++)
                    if (rows[r][c] != ' ')
                        _buf.WriteToBuffer(startX + c, startY + r, rows[r][c], color);
        }

        // ── HELPERS ──────────────────────────────────────────────────────────

        private static int ScaleX(int gx, int iw)  => (int)(gx * iw  / (float)GameState.GameWidth);
        private static int ScaleY(int gy, int gdh) => (int)(gy * gdh / (float)GameState.GameHeight);

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
