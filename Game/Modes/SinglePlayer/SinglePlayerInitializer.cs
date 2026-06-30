using System;
using FlappyBird.Models;

namespace FlappyBird.Game.Modes.SinglePlayer
{
    /// <summary>
    /// Quản lý việc khởi tạo game cho Single Player Mode
    /// </summary>
    public class SinglePlayerInitializer
    {
        private readonly SinglePlayerValidator validator;

        public SinglePlayerInitializer()
        {
            validator = new SinglePlayerValidator();
        }

        /// <summary>
        /// Khởi tạo game với border hoàn toàn nhất quán với menu design
        /// </summary>
        public void InitializeGameWithMenuConsistentBorders(GameState gameState, Random random)
        {
            // Validate trước khi khởi tạo
            validator.ValidateMenuConsistency();

            gameState.Pipes.Clear();

            // Tạo pipe đầu tiên với spacing phù hợp với border width
            int initialPipeX = GameState.GameWidth - 1;
            gameState.Pipes.Add(new Pipe(initialPipeX, GameState.BaseGapSize, GameState.GameHeight, random));

            // Set last pipe position để spacing đều đặn
            gameState.LastPipeX = initialPipeX;
        }
    }
}
