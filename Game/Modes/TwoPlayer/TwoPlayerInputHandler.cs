using System;
using FlappyBird.Models;

namespace FlappyBird.Game.Modes.TwoPlayer
{
    /// <summary>
    /// Xử lý input cho TwoPlayerGameMode
    /// </summary>
    public class TwoPlayerInputHandler(TwoPlayerGameOverMenu gameOverMenu, TwoPlayerCountdown countdown)
    {
        private readonly TwoPlayerGameOverMenu gameOverMenu = gameOverMenu;
        private readonly TwoPlayerCountdown countdown = countdown;

        /// <summary>
        /// Xử lý input chính cho two player mode
        /// </summary>
        public TwoPlayerInputResult HandleInput(ConsoleKeyInfo keyInfo, bool gameStarted, 
            GameState player1State, GameState player2State)
        {
            // Xử lý input cho game over menu (chỉ sau khi delay) - như SinglePlayerGameMode
            if (gameOverMenu.ShowGameOverMenu)
            {
                // Chỉ cho phép input sau khi delay để người chơi thấy kết quả
                if (DateTime.Now - gameOverMenu.GameOverTime > TimeSpan.FromMilliseconds(800))
                {
                    var menuAction = gameOverMenu.HandleGameOverMenuInput(keyInfo);
                    return new TwoPlayerInputResult 
                    { 
                        ShouldExit = menuAction == GameOverMenuAction.Exit,
                        ShouldRestart = menuAction == GameOverMenuAction.Restart
                    };
                }
                return new TwoPlayerInputResult();
            }

            // Nếu đang đếm ngược thì không nhận input khác
            if (countdown.IsCountingDown)
                return new TwoPlayerInputResult();

            var result = new TwoPlayerInputResult();

            switch (keyInfo.Key)
            {
                case ConsoleKey.Spacebar:
                    if (!gameStarted && !countdown.IsCountingDown)
                    {
                        result.ShouldStartCountdown = true;
                    }
                    break;
                case ConsoleKey.W:
                    if (gameStarted && !player1State.GameOver)
                    {
                        result.Player1Jump = true;
                    }
                    break;
                case ConsoleKey.UpArrow:
                    if (gameStarted && !player2State.GameOver)
                    {
                        result.Player2Jump = true;
                    }
                    break;
                case ConsoleKey.Escape:
                    result.ShouldExit = true;
                    break;
            }

            return result;
        }
    }

    /// <summary>
    /// Kết quả xử lý input
    /// </summary>
    public class TwoPlayerInputResult
    {
        public bool ShouldExit { get; set; } = false;
        public bool ShouldRestart { get; set; } = false;
        public bool ShouldStartCountdown { get; set; } = false;
        public bool Player1Jump { get; set; } = false;
        public bool Player2Jump { get; set; } = false;
    }
}
