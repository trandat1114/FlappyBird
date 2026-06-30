using FlappyBird.Models;

namespace FlappyBird.Game
{
    /// <summary>
    /// Interface cho các chế độ chơi khác nhau
    /// </summary>
    public interface IGameMode
    {
        /// <summary>
        /// Khởi tạo chế độ chơi
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Cập nhật logic game
        /// </summary>
        void Update();
        
        /// <summary>
        /// Render game
        /// </summary>
        void Render();
        
        /// <summary>
        /// Xử lý input
        /// </summary>
        void HandleInput(ConsoleKeyInfo keyInfo);
        
        /// <summary>
        /// Kiểm tra game có kết thúc không
        /// </summary>
        bool IsGameOver();
        
        /// <summary>
        /// Cleanup khi thoát
        /// </summary>
        void Cleanup();
    }
}
