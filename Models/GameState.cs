namespace FlappyBird.Models
{
    /// <summary>
    /// Class chứa tất cả trạng thái của game
    /// </summary>
    public class GameState
    {
        // === GAME DIMENSIONS - PHÙ HỢP VỚI MENU DESIGN ===
        public const int GameWidth = 66;  // Khớp với menu border width
        public const int GameHeight = 22; // Tăng height để có không gian chơi tốt hơn
        public const int BirdX = 8;       // Điều chỉnh vị trí chim phù hợp với width mới
        
        // === GAME STATE ===
        public int BirdY { get; set; } = 11; // Vị trí khởi đầu ở giữa màn hình (22/2 = 11)
        public float BirdVelocity { get; set; } = 0f;
        public int Score { get; set; } = 0;
        public bool GameOver { get; set; } = false;
        public bool GameStarted { get; set; } = false;
        public int FrameCounter { get; set; } = 0;
        
        // === PHYSICS - CÂN BẰNG CHO KHUNG NHỎ 20x80 ===
        public const float Gravity = 0.06f; // Cân bằng: đủ nhanh để cảm thấy tự nhiên, đủ chậm để kiểm soát
        public const float JumpStrength = -0.8f; // Giảm xuống: vừa đủ để vượt gap mà không bay quá cao
        public const float MaxFallSpeed = 1.0f; // Cân bằng: nhanh nhưng vẫn kiểm soát được
        
        // === PIPES AND DIFFICULTY - TỐI ƯU CHO WIDTH MỚI ===
        public List<Pipe> Pipes { get; set; } = new List<Pipe>();
        public const int PipeSpacing = 35; // Điều chỉnh cho width 66: tối đa 2 ống trên màn hình
        public int LastPipeX { get; set; } = GameWidth;
        public int DifficultyLevel { get; set; } = 1; // Bắt đầu từ level 1
        public int PipeSpeed { get; set; } = 4; // Chậm hơn ban đầu để học
        
        // === GAP SIZE ĐỘNG - TỐI ƯU CHO HEIGHT MỚI ===
        public const int BaseGapSize = 10; // Gap lớn hơn cho height 22
        public const int MinGapSize = 7;   // Gap tối thiểu vẫn chơi được
        
        // === PHYSICS PRECISE POSITION ===
        // BirdYf giữ vị trí float cho vật lý delta time; BirdY là int để render/collision
        public float BirdYf { get; set; } = 11f;

        // Bộ tích lũy thời gian di chuyển ống (giây), thay thế FrameCounter%PipeSpeed
        public float PipeTimeAccumulator { get; set; } = 0f;

        // === ANIMATION & EFFECTS ===
        public int BirdAnimationFrame { get; set; } = 0;
        
        // === GOD MODE ===
        public bool GodMode { get; set; } = false;
        public bool GodModeAutoRestart { get; set; } = false; // Tự động restart khi God mode thua
        public int GodModeAttempts { get; set; } = 0; // Số lần thử của God mode
        public int GodModeBestScore { get; set; } = 0; // Điểm cao nhất của God mode
        
        /// <summary>
        /// Kiểm tra God mode có đang hoạt động không
        /// </summary>
        public bool IsGodModeActive => GodMode && GameStarted;
        
        // === OPTIMIZATION ===
        public bool ForceFullRedraw { get; set; } = false;
        public int LastScore { get; set; } = 0;
        public int LastDifficultyLevel { get; set; } = 0;
        public bool LastGameStarted { get; set; } = false;
        public bool LastGodMode { get; set; } = false;
        
        // === RENDERING BUFFERS ===
        public char[,] CurrentScreen { get; set; } = new char[GameHeight, GameWidth];
        public char[,] PreviousScreen { get; set; } = new char[GameHeight, GameWidth];
        
        /// <summary>
        /// Reset game về trạng thái ban đầu
        /// </summary>
        public void Reset()
        {
            BirdY = 11;
            BirdYf = 11f;
            BirdVelocity = 0f;
            PipeTimeAccumulator = 0f;
            Score = 0;
            GameOver = false;
            GameStarted = false;
            FrameCounter = 0;
            
            // Reset difficulty về level 1
            DifficultyLevel = 1;
            PipeSpeed = 4; // Chậm nhất để bắt đầu
            BirdAnimationFrame = 0;
            
            // Reset tracking variables cho rendering tối ưu
            LastScore = 0;
            LastDifficultyLevel = 0;
            LastGameStarted = false;
            ForceFullRedraw = true;
            
            // Không reset GodMode, GodModeAutoRestart, GodModeAttempts, GodModeBestScore
            // để giữ trạng thái qua các lần chơi
            
            // Clear pipes
            Pipes.Clear();
            
            // Reset màn hình buffer
            for (int y = 0; y < GameHeight; y++)
            {
                for (int x = 0; x < GameWidth; x++)
                {
                    PreviousScreen[y, x] = ' ';
                }
            }
        }
        
        /// <summary>
        /// Cập nhật độ khó dựa trên điểm số
        /// </summary>
        public void UpdateDifficulty()
        {
            // Tăng level mỗi 5 điểm để có thời gian thích nghi
            int newDifficultyLevel = (Score / 5) + 1;
            
            if (newDifficultyLevel != DifficultyLevel)
            {
                DifficultyLevel = newDifficultyLevel;
                
                // Pipe speed tăng dần
                if (DifficultyLevel <= 2) PipeSpeed = 4;
                else if (DifficultyLevel <= 4) PipeSpeed = 3;
                else if (DifficultyLevel <= 6) PipeSpeed = 2;
                else PipeSpeed = 1;
            }
        }
        
        /// <summary>
        /// Tính gap size hiện tại dựa trên difficulty level
        /// </summary>
        public int GetCurrentGapSize()
        {
            // Level 1-3: Gap 9 (học cách chơi)
            // Level 4-6: Gap 8 (trung bình) 
            // Level 7-9: Gap 7 (khó)
            // Level 10+: Gap 6 (chuyên nghiệp)
            int currentGapSize = BaseGapSize - (DifficultyLevel / 3);
            return System.Math.Max(MinGapSize, currentGapSize);
        }
    }
}
