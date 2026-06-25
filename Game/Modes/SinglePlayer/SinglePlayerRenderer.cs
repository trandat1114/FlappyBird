using FlappyBird.Localization;
using FlappyBird.Models;
using FlappyBird.Rendering;
using FlappyBird.Settings;
using FlappyBird.UI;

namespace FlappyBird.Game.Modes.SinglePlayer
{
    public class SinglePlayerRenderer
    {
        // === LAYOUT CONSTANTS ===
        private const int GAME_AREA_TOP = 0;
        private const int FOOTER_ROWS   = 4; // top border + score + controls + bottom border
        private const int FOOTER_TOP    = GAME_AREA_TOP + GameState.GameHeight;

        // Center the game in the terminal when the window is larger.
        public static int OriginX => Math.Max(0, (Console.WindowWidth  - GameState.GameWidth)           / 2);
        public static int OriginY => Math.Max(0, (Console.WindowHeight - (GameState.GameHeight + FOOTER_ROWS)) / 2);

        // === ASCII ART CHARACTERS ===
        private const char BirdChar = '♦';
        private const char PipeChar = '█';
        private const char BorderH = '═';
        private const char BorderV = '║';
        private const char CornerTL = '╔';
        private const char CornerTR = '╗';
        private const char CornerBL = '╚';
        private const char CornerBR = '╝';
        private const char BgDot = '·';

        // === PRE-ALLOCATED BUFFERS (không new mỗi frame) ===
        private readonly char[,] _screenBuf = new char[GameState.GameHeight, GameState.GameWidth];
        private readonly char[,] _bgBuf = new char[GameState.GameHeight, GameState.GameWidth];

        public SinglePlayerRenderer()
        {
            // Pre-compute nền một lần – Buffer.BlockCopy thay nested loop mỗi frame
            for (int y = 0; y < GameState.GameHeight; y++)
                for (int x = 0; x < GameState.GameWidth; x++)
                    _bgBuf[y, x] = ((x + y) % 8 == 0 && y > 0 && y < GameState.GameHeight - 1
                                    && x > 0 && x < GameState.GameWidth - 1)
                                   ? BgDot : ' ';
        }

        // ── PUBLIC API ──────────────────────────────────────────────────────

        public void Draw(GameState gs)
        {
            // 1. Copy nền đã pre-compute (nhanh hơn nested loop ~5x)
            Buffer.BlockCopy(_bgBuf, 0, _screenBuf, 0,
                GameState.GameHeight * GameState.GameWidth * sizeof(char));

            // 2. Vẽ từng lớp vào buffer
            DrawBorders(_screenBuf);
            DrawPipes(_screenBuf, gs);
            DrawBird(_screenBuf, gs);

            // 3. Diff & flush chỉ những cell thay đổi (không alloc List, không Sort)
            FlushDiff(_screenBuf, gs);

            // 4. Cập nhật UI dòng footer chỉ khi có thay đổi
            if (IsUiChanged(gs))
            {
                UpdateFooterLine(gs);
                gs.LastScore = gs.Score;
                gs.LastDifficultyLevel = gs.DifficultyLevel;
                gs.LastGameStarted = gs.GameStarted;
                gs.LastGodMode = gs.GodMode;
                gs.ForceFullRedraw = false;
            }
        }

        /// <summary>Gọi khi cần redraw toàn màn hình (game mới, restart)</summary>
        public void RenderWithConsistentDesign(GameState gs)
        {
            Console.Clear();
            // After Clear() the console is all spaces.
            // Sync PreviousScreen so FlushDiff re-writes every visible cell.
            for (int y = 0; y < GameState.GameHeight; y++)
                for (int x = 0; x < GameState.GameWidth; x++)
                    gs.PreviousScreen[y, x] = ' ';
            Draw(gs);
            RenderFooter(gs);
        }

        public void InitializeScreen(GameState gs)
        {
            for (int y = 0; y < GameState.GameHeight; y++)
                for (int x = 0; x < GameState.GameWidth; x++)
                    gs.PreviousScreen[y, x] = '\0'; // Force full redraw lần đầu

            Console.Clear();
        }

        /// <summary>
        /// Resets diff-tracking to ' ' so FlushDiff redraws all cells on the next frame.
        /// Call this immediately after Console.Clear() on terminal resize.
        /// </summary>
        public static void InvalidateScreen(GameState gs)
        {
            for (int y = 0; y < GameState.GameHeight; y++)
                for (int x = 0; x < GameState.GameWidth; x++)
                    gs.PreviousScreen[y, x] = ' ';
        }

        public void RenderGameOverOverlay()
        {
            Console.SetCursorPosition(OriginX + GameState.GameWidth / 2 - 6,
                                      OriginY + GAME_AREA_TOP + GameState.GameHeight / 2);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(" GAME OVER! ");
            Console.ResetColor();
        }

        // ── PRIVATE DRAWING ─────────────────────────────────────────────────

        private static void DrawBorders(char[,] buf)
        {
            int w = GameState.GameWidth, h = GameState.GameHeight;
            buf[0, 0] = CornerTL; buf[0, w - 1] = CornerTR;
            buf[h - 1, 0] = CornerBL; buf[h - 1, w - 1] = CornerBR;
            for (int x = 1; x < w - 1; x++) { buf[0, x] = BorderH; buf[h - 1, x] = BorderH; }
            for (int y = 1; y < h - 1; y++) { buf[y, 0] = BorderV; buf[y, w - 1] = BorderV; }
        }

        private static void DrawPipes(char[,] buf, GameState gs)
        {
            foreach (var pipe in gs.Pipes)
            {
                int px = pipe.X;
                if (px < 1 || px >= GameState.GameWidth - 1) continue;

                // Thân ống trên (3 cột rộng)
                for (int y = 1; y <= pipe.TopHeight; y++)
                {
                    if (px - 1 >= 1) buf[y, px - 1] = PipeChar;
                    buf[y, px] = PipeChar;
                    if (px + 1 < GameState.GameWidth - 1) buf[y, px + 1] = PipeChar;
                }
                // Mũ ống trên
                if (pipe.TopHeight >= 1 && pipe.TopHeight < GameState.GameHeight - 1)
                    for (int cx = px - 2; cx <= px + 2; cx++)
                        if (cx >= 1 && cx < GameState.GameWidth - 1)
                            buf[pipe.TopHeight, cx] = '▀';

                // Thân ống dưới (3 cột rộng)
                int botStart = GameState.GameHeight - pipe.BottomHeight - 1;
                for (int y = botStart; y < GameState.GameHeight - 1; y++)
                {
                    if (px - 1 >= 1) buf[y, px - 1] = PipeChar;
                    buf[y, px] = PipeChar;
                    if (px + 1 < GameState.GameWidth - 1) buf[y, px + 1] = PipeChar;
                }
                // Mũ ống dưới
                if (botStart > 1 && botStart < GameState.GameHeight - 1)
                    for (int cx = px - 2; cx <= px + 2; cx++)
                        if (cx >= 1 && cx < GameState.GameWidth - 1)
                            buf[botStart, cx] = '▄';
            }
        }

        private static void DrawBird(char[,] buf, GameState gs)
        {
            int bx = GameState.BirdX, by = gs.BirdY;
            if (bx < 1 || bx >= GameState.GameWidth - 1 || by < 1 || by >= GameState.GameHeight - 1)
                return;

            char birdChar = gs.BirdVelocity < -0.12f ? '^'
                          : gs.BirdVelocity > 0.12f ? 'v'
                          : BirdChar;
            buf[by, bx] = birdChar;

            // Cánh chim
            gs.BirdAnimationFrame = (gs.BirdAnimationFrame + 1) % 4;
            if (bx - 1 >= 1)
                buf[by, bx - 1] = gs.BirdAnimationFrame < 2 ? '~' : '_';
        }

        private static void FlushDiff(char[,] newBuf, GameState gs)
            => DiffRenderer.Flush(newBuf, gs.PreviousScreen,
                   originY: GAME_AREA_TOP + OriginY,
                   originX: OriginX,
                   colorFn: CharColor);

        private static ConsoleColor CharColor(char ch) => ch switch
        {
            '╔' or '╗' or '╚' or '╝' or '═' or '║' => GameSettings.Instance.PrimaryColor,
            '█' or '▀' or '▄'                        => ConsoleColor.Green,
            '♦' or '^'  or 'v'                        => ConsoleColor.Yellow,
            '~' or '_'                                 => ConsoleColor.DarkYellow,
            '·'                                        => ConsoleColor.DarkGray,
            _                                          => ConsoleColor.DarkGray,
        };

        // ── FOOTER ──────────────────────────────────────────────────────────

        private static void RenderFooter(GameState gs)
        {
            var panel = GameSettings.Instance.CreatePanel(GameState.GameWidth);
            ConsoleColor ctrlColor = gs.GameStarted ? ConsoleColor.Green : ConsoleColor.Yellow;

            int footerY = OriginY + FOOTER_TOP;

            // Row 0: Top border
            Console.SetCursorPosition(OriginX, footerY);
            Console.ForegroundColor = panel.BorderColor;
            Console.Write(panel.BuildTop());
            Console.ResetColor();

            // Row 1: Status line
            Console.SetCursorPosition(OriginX, footerY + 1);
            string statusContent = gs.GameStarted
                ? $"  Score: {gs.Score,3}  │  Level: {gs.DifficultyLevel,2}  │  Speed: {gs.PipeSpeed}  │  Gap: {gs.GetCurrentGapSize(),2}"
                : $"  {L.Get(L.STATUS_READY)}";
            panel.PrintRow(statusContent, ConsoleColor.Yellow);

            // Row 2: Controls
            string ctrlContent = gs.GameStarted
                ? $" {L.Get(L.CTRL_MANUAL)}  │  {L.Get(L.CTRL_JUMP)}  │  {L.Get(L.CTRL_EXIT)}"
                : $" {L.Get(L.CTRL_START)}  │  {L.Get(L.CTRL_EXIT)}";
            Console.SetCursorPosition(OriginX, footerY + 2);
            panel.PrintRow(ctrlContent, ctrlColor);

            // Row 3: Bottom border (use Write to prevent scroll on last line)
            Console.SetCursorPosition(OriginX, footerY + 3);
            Console.ForegroundColor = panel.BorderColor;
            Console.Write(panel.BuildBottom());
            Console.ResetColor();
        }

        private static void UpdateFooterLine(GameState gs)
        {
            var panel = GameSettings.Instance.CreatePanel(GameState.GameWidth);
            int footerY = OriginY + FOOTER_TOP;
            Console.SetCursorPosition(OriginX, footerY + 1);
            string statusContent = gs.GameStarted
                ? $"  Score: {gs.Score,3}  │  Level: {gs.DifficultyLevel,2}  │  Speed: {gs.PipeSpeed}  │  Gap: {gs.GetCurrentGapSize(),2}"
                : $"  {L.Get(L.STATUS_READY)}";
            panel.PrintRow(statusContent, ConsoleColor.Yellow);
        }

        private static bool IsUiChanged(GameState gs) =>
            gs.Score != gs.LastScore ||
            gs.DifficultyLevel != gs.LastDifficultyLevel ||
            gs.GameStarted != gs.LastGameStarted ||
            gs.GodMode != gs.LastGodMode ||
            gs.ForceFullRedraw;
    }
}
