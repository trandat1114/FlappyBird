using FlappyBird.Audio;
using FlappyBird.Audio.Song;
using FlappyBird.Enum;
using FlappyBird.Game;
using FlappyBird.Rendering;
using FlappyBird.Settings;
using FlappyBird.UI;
using FlappyBird.Models;

namespace FlappyBird;

class FlappyBirdGame
{
    static void Main()
    {
        try { Console.CursorVisible = false; } catch { }

        // Enforce consistent responsive constraints: min 24×78, max 24×80 (via ConsoleLayout)
        // This ensures both single player and two player modes are compatible.
        if (OperatingSystem.IsWindows())
        {
            try
            {
                int minW = ConsoleLayout.MIN_WIDTH;
                int minH = ConsoleLayout.MIN_HEIGHT;
                if (Console.WindowWidth < minW || Console.WindowHeight < minH)
                {
                    Console.SetWindowSize(Math.Max(Console.WindowWidth, minW),
                                         Math.Max(Console.WindowHeight, minH));
                }
            }
            catch { /* terminal may not support resize — proceed anyway */ }
        }

        // Snapshot current dimensions so HasResized() doesn't trigger on first frame
        ConsoleLayout.Snapshot();

        // Register MP3 effect files
        AudioManager.SetAudioDir(Path.Combine(AppContext.BaseDirectory, "Resources", "Audio"));

        // Scan font resources before loading settings so Apply() can work immediately
        string fontsDir = Path.Combine(AppContext.BaseDirectory, "Resources", "Fonts");
        FontManager.Scan(fontsDir);

        // Load persisted settings; always write defaults file on first run
        SettingsSerializer.Load(GameSettings.Instance);
        SettingsSerializer.Save(GameSettings.Instance);

        // Load audio offset calibration (per-sound start-offset trim)
        AudioManager.LoadAudioOffset();
        var gs = GameSettings.Instance;
        if (!string.IsNullOrEmpty(gs.FontFaceName))
            FontManager.Apply(gs.FontFaceName, gs.FontSize);

        if (gs.MusicEnabled)
            AudioManager.StartBackgroundMusic(HarryPotter.Melody);

        // Main menu loop
        while (true)
        {
            var menuAction = SimpleMenuSystem.ShowMenu();

            switch (menuAction)
            {
                case MenuAction.SinglePlayer:
                    GameEngine.StartGame(GameModeFactory.MenuActionToGameMode(menuAction));
                    break;

                case MenuAction.TwoPlayer:
                    GameEngine.StartGame(GameModeFactory.MenuActionToGameMode(menuAction));
                    break;

                case MenuAction.DualAI:
                    GameEngine.StartGame(GameModeFactory.MenuActionToGameMode(menuAction));
                    break;

                case MenuAction.SplitScreenAI:
                    GameEngine.StartGame(GameModeFactory.MenuActionToGameMode(menuAction));
                    break;

                case MenuAction.AITournament:
                    GameEngine.StartGame(GameModeFactory.MenuActionToGameMode(menuAction));
                    break;

                case MenuAction.HighScores:
                    HighScoreMenu.Show();
                    break;

                case MenuAction.CustomGame:
                    CustomGameConfig? cfg = CustomGameMenu.Show();
                    if (cfg != null)
                    {
                        GameSettings.Instance.LastCustomGame = cfg;
                        SettingsSerializer.Save(GameSettings.Instance);
                        GameEngine.StartGame(GameMode.SinglePlayer, cfg);
                    }
                    break;

                case MenuAction.Settings:
                    SettingsMenu.Show();
                    GameSettings.Instance.Apply();
                    SettingsSerializer.Save(GameSettings.Instance);
                    break;

                case MenuAction.Exit:
                    SettingsSerializer.Save(GameSettings.Instance);
                    FontManager.Cleanup();
                    AudioManager.StopAllSounds();
                    Console.ResetColor();
                    Console.Clear();
                    return;
            }
        }
    }
}
