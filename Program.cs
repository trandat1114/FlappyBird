using FlappyBird.Audio;
using FlappyBird.Audio.Song;
using FlappyBird.Enum;
using FlappyBird.Game;
using FlappyBird.Settings;
using FlappyBird.UI;

namespace FlappyBird;

class FlappyBirdGame
{
    static void Main()
    {
        try { Console.CursorVisible = false; } catch { }

        // Chỉ thiết lập kích thước cửa sổ trên Windows
        if (OperatingSystem.IsWindows())
        {
            try
            {
                Console.SetWindowSize(100, 40);
                Console.SetBufferSize(100, 40);
            }
            catch
            {
                // Ignore if we can't set window size
            }
        }

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
