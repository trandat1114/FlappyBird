using System;
using FlappyBird.Models;

namespace FlappyBird.Game.Modes.SinglePlayer
{
    /// <summary>
    /// Xử lý input cho Single Player Mode
    /// </summary>
    public class SinglePlayerInputHandler(SinglePlayerGameOverMenu gameOverMenu)
    {
        private readonly SinglePlayerGameOverMenu gameOverMenu = gameOverMenu;

        /// <summary>
        /// Xử lý input chính cho Single Player
        /// </summary>
        public SinglePlayerInputResult HandleInput(ConsoleKeyInfo keyInfo, GameState gameState)
        {
            var result = new SinglePlayerInputResult();

            // Xử lý input cho game over menu (chỉ sau khi delay)
            if (gameOverMenu.ShowGameOverMenu)
            {
                if (gameOverMenu.CanReceiveInput())
                {
                    var menuResult = gameOverMenu.HandleGameOverMenuInput(keyInfo);
                    result.ShouldRestart = menuResult.ShouldRestart;
                    result.ShouldExit = menuResult.ShouldExit;
                }
                return result;
            }

            // Xử lý input trong game
            switch (keyInfo.Key)
            {
                case ConsoleKey.Spacebar:
                    result.ShouldJump = true;
                    break;

                case ConsoleKey.Escape:
                    result.ShouldExit = true;
                    break;
            }

            return result;
        }
    }

    /// <summary>
    /// Kết quả xử lý input của Single Player
    /// </summary>
    public class SinglePlayerInputResult
    {
        public bool ShouldJump { get; set; } = false;
        public bool ShouldRestart { get; set; } = false;
        public bool ShouldExit { get; set; } = false;
    }
}
