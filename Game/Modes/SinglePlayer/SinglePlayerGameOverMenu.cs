using FlappyBird.Localization;
using FlappyBird.Models;

namespace FlappyBird.Game.Modes.SinglePlayer
{
    public class SinglePlayerGameOverMenu(SinglePlayerRenderer renderer)
    {
        // Layout phải khớp với SinglePlayerRenderer
        private const int GAME_AREA_TOP = 3;
        private const int FOOTER_TOP = GAME_AREA_TOP + GameState.GameHeight; // = 25

        private bool showGameOverMenu = false;
        private int gameOverSelectedIndex = 0;
        private static string[] GameOverOptions => [L.Get(L.GO_PLAY_AGAIN), L.Get(L.GO_MAIN_MENU)];
        private DateTime gameOverTime = DateTime.MinValue;

        private readonly SinglePlayerRenderer renderer = renderer;

        public bool ShowGameOverMenu => showGameOverMenu;
        public DateTime GameOverTime => gameOverTime;

        public void StartGameOverMenu()
        {
            showGameOverMenu = true;
            gameOverSelectedIndex = 0;
            gameOverTime = DateTime.Now;
        }

        public void ResetGameOverMenu()
        {
            showGameOverMenu = false;
            gameOverSelectedIndex = 0;
            gameOverTime = DateTime.MinValue;
        }

        public bool CanReceiveInput() =>
            showGameOverMenu && DateTime.Now - gameOverTime > TimeSpan.FromMilliseconds(800);

        public bool ShouldShowMenu() =>
            showGameOverMenu && DateTime.Now - gameOverTime > TimeSpan.FromMilliseconds(800);

        public void RenderGameOverMenu(GameState gs)
        {
            renderer.Draw(gs);

            Console.SetCursorPosition(0, FOOTER_TOP);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         GAME  OVER !                          ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║                     Score: {gs.Score,3} {L.Get(L.GO_POINTS),-4}                       ║");
            Console.WriteLine($"║                     Level: {gs.DifficultyLevel,3}                               ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");

            for (int i = 0; i < GameOverOptions.Length; i++)
            {
                if (i == gameOverSelectedIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"║  > {GameOverOptions[i],-58}  ║");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"║    {GameOverOptions[i],-58}  ║");
                }
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
            Console.WriteLine(("║  " + L.Get(L.GO_CONTROLS)).PadRight(GameState.GameWidth - 1) + "║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        public SinglePlayerGameOverMenuInputResult HandleGameOverMenuInput(ConsoleKeyInfo keyInfo)
        {
            var result = new SinglePlayerGameOverMenuInputResult();

            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    gameOverSelectedIndex = gameOverSelectedIndex > 0
                        ? gameOverSelectedIndex - 1 : GameOverOptions.Length - 1;
                    break;

                case ConsoleKey.DownArrow:
                    gameOverSelectedIndex = gameOverSelectedIndex < GameOverOptions.Length - 1
                        ? gameOverSelectedIndex + 1 : 0;
                    break;

                case ConsoleKey.Enter:
                    result.ShouldRestart = gameOverSelectedIndex == 0;
                    result.ShouldExit = gameOverSelectedIndex != 0;
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
        public bool ShouldExit { get; set; } = false;
    }
}
