using FlappyBird.Localization;
using FlappyBird.Models;
using FlappyBird.Settings;
using FlappyBird.UI;

namespace FlappyBird.Game.Modes.SinglePlayer
{
    public class SinglePlayerGameOverMenu(SinglePlayerRenderer renderer)
    {
        private const int GAME_AREA_TOP = 0;
        private const int FOOTER_TOP    = GAME_AREA_TOP + GameState.GameHeight; // = 20

        private bool     _show              = false;
        private int      _selectedIndex     = 0;
        private DateTime _startTime         = DateTime.MinValue;

        private static string[] GameOverOptions => [L.Get(L.GO_PLAY_AGAIN), L.Get(L.GO_MAIN_MENU)];

        private readonly SinglePlayerRenderer _renderer = renderer;

        public bool     ShowGameOverMenu => _show;
        public DateTime GameOverTime     => _startTime;

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

        public bool ShouldShowMenu() =>
            _show && DateTime.Now - _startTime > TimeSpan.FromMilliseconds(800);

        public void RenderGameOverMenu(GameState gs)
        {
            _renderer.Draw(gs);

            var panel = GameSettings.Instance.CreatePanel(GameState.GameWidth);
            panel.BorderColor = ConsoleColor.Red;
            var bs   = panel.Borders;
            var opts = GameOverOptions;

            Console.SetCursorPosition(0, FOOTER_TOP);

            // Row 0: top border
            Console.ForegroundColor = panel.BorderColor;
            Console.WriteLine(panel.BuildTop());

            // Row 1: score info (GAME OVER overlay in game area is sufficient)
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(panel.BuildRow(
                $"  Score: {gs.Score,3}  │  Level: {gs.DifficultyLevel,2}  │  Speed: {gs.PipeSpeed}  │  Gap: {gs.GetCurrentGapSize(),2}"));

            // Row 2: options with per-option highlight (32 + │ + 31 = 64 inner chars)
            Console.ForegroundColor = panel.BorderColor;
            Console.Write(bs.Vert);

            bool sel0 = _selectedIndex == 0;
            if (sel0) { Console.ForegroundColor = ConsoleColor.Black; Console.BackgroundColor = ConsoleColor.Yellow; }
            else Console.ForegroundColor = ConsoleColor.White;
            Console.Write(((sel0 ? "  ► " : "    ") + opts[0]).PadRight(32));
            Console.ResetColor();

            Console.ForegroundColor = panel.BorderColor;
            Console.Write('│');

            bool sel1 = _selectedIndex == 1;
            if (sel1) { Console.ForegroundColor = ConsoleColor.Black; Console.BackgroundColor = ConsoleColor.Yellow; }
            else Console.ForegroundColor = ConsoleColor.White;
            Console.Write(((sel1 ? "  ► " : "    ") + opts[1]).PadRight(31));
            Console.ResetColor();

            Console.ForegroundColor = panel.BorderColor;
            Console.WriteLine(bs.Vert);

            // Row 3: bottom border — Write (not WriteLine) to prevent scroll at last terminal row
            Console.ForegroundColor = panel.BorderColor;
            Console.Write(panel.BuildBottom());
            Console.ResetColor();
        }

        public SinglePlayerGameOverMenuInputResult HandleGameOverMenuInput(ConsoleKeyInfo keyInfo)
        {
            var result = new SinglePlayerGameOverMenuInputResult();
            var opts   = GameOverOptions;

            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.LeftArrow:
                    _selectedIndex = _selectedIndex > 0 ? _selectedIndex - 1 : opts.Length - 1;
                    break;

                case ConsoleKey.DownArrow:
                case ConsoleKey.RightArrow:
                    _selectedIndex = _selectedIndex < opts.Length - 1 ? _selectedIndex + 1 : 0;
                    break;

                case ConsoleKey.Enter:
                    result.ShouldRestart = _selectedIndex == 0;
                    result.ShouldExit    = _selectedIndex != 0;
                    break;

                case ConsoleKey.Spacebar:
                    result.ShouldRestart = true;
                    break;

                case ConsoleKey.Escape:
                    result.ShouldExit = true;
                    break;
            }

            return result;
        }
    }

    public class SinglePlayerGameOverMenuInputResult
    {
        public bool ShouldRestart { get; set; } = false;
        public bool ShouldExit    { get; set; } = false;
    }
}
