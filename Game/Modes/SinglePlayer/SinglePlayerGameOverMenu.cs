using FlappyBird.Localization;
using FlappyBird.Models;
using FlappyBird.Settings;
using FlappyBird.UI;

namespace FlappyBird.Game.Modes.SinglePlayer
{
    public class SinglePlayerGameOverMenu(SinglePlayerRenderer renderer)
    {
        private const int GAME_AREA_TOP = 3;
        private const int FOOTER_TOP    = GAME_AREA_TOP + GameState.GameHeight; // = 25

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

            var opts = GameOverOptions;

            Console.SetCursorPosition(0, FOOTER_TOP);
            panel.PrintTop();
            panel.PrintTitle(L.Get(L.GO_GAME_OVER), ConsoleColor.Red);
            panel.PrintSep();
            panel.PrintRow($"  Score: {gs.Score,3} {L.Get(L.GO_POINTS)}", ConsoleColor.Yellow);
            panel.PrintRow($"  Level: {gs.DifficultyLevel}", ConsoleColor.Yellow);
            panel.PrintSep();

            for (int i = 0; i < opts.Length; i++)
            {
                bool selected = i == _selectedIndex;
                string prefix  = selected ? "  > " : "    ";
                panel.PrintRow(
                    $"{prefix}{opts[i]}",
                    fg:        selected ? ConsoleColor.Black : ConsoleColor.White,
                    contentBg: selected ? ConsoleColor.Yellow : null);
            }

            panel.PrintSep();
            panel.PrintRow($"  {L.Get(L.GO_CONTROLS)}", ConsoleColor.Gray);
            panel.PrintBottom();
        }

        public SinglePlayerGameOverMenuInputResult HandleGameOverMenuInput(ConsoleKeyInfo keyInfo)
        {
            var result = new SinglePlayerGameOverMenuInputResult();
            var opts   = GameOverOptions;

            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    _selectedIndex = _selectedIndex > 0 ? _selectedIndex - 1 : opts.Length - 1;
                    break;

                case ConsoleKey.DownArrow:
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
