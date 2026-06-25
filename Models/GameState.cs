namespace FlappyBird.Models
{
    /// <summary>
    /// All runtime game state. BirdState and DifficultyState are embedded here as
    /// bridge properties (Phase 3 migration) — callers can start using them directly
    /// and GameState flat properties will be removed once all modes are migrated.
    /// </summary>
    public class GameState
    {
        // Phase 3 bridge: structured sub-objects available now; flat properties kept for compat
        public BirdState       Bird       { get; } = new();
        public DifficultyState Difficulty { get; } = new();

        // === GAME DIMENSIONS - PHÙ HỢP VỚI MENU DESIGN ===
        public const int GameWidth = 78;  // Khớp với menu border width (panel 78, inner 76)
        public const int GameHeight = 20; // 20 game rows + 4 footer = 24 rows (no header)
        public const int BirdX = 8;       // Điều chỉnh vị trí chim phù hợp với width mới

        // === GAME STATE ===
        public int BirdY { get; set; } = 10; // Vị trí khởi đầu ở giữa màn hình (20/2 = 10)
        public float BirdVelocity { get; set; } = 0f;
        public int Score { get; set; } = 0;
        public bool GameOver { get; set; } = false;
        public bool GameStarted { get; set; } = false;
        public int FrameCounter { get; set; } = 0;
        
        // === PHYSICS - CÂN BẰNG CHO KHUNG NHỎ 20x80 ===
        public const float Gravity = 0.06f; // Cân bằng: đủ nhanh để cảm thấy tự nhiên, đủ chậm để kiểm soát
        public const float JumpStrength = -0.7f; // Nhẹ hơn: kiểm soát tốt hơn, phù hợp gap tối thiểu 5
        public const float MaxFallSpeed = 1.0f; // Cân bằng: nhanh nhưng vẫn kiểm soát được
        
        // === PIPES AND DIFFICULTY - TỐI ƯU CHO WIDTH MỚI ===
        public List<Pipe> Pipes { get; set; } = new List<Pipe>();
        public const int PipeSpacing = 35; // Tối đa 2 ống trên màn hình (spawn khi lastPipe.X < GameWidth-35)
        public int LastPipeX { get; set; } = GameWidth;
        public int DifficultyLevel { get; set; } = 1; // Bắt đầu từ level 1
        public int PipeSpeed { get; set; } = 4; // Chậm hơn ban đầu để học
        
        // === GAP SIZE ĐỘNG - TỐI ƯU CHO HEIGHT MỚI ===
        public const int BaseGapSize = 7; // Gap phù hợp cho height 20
        public const int MinGapSize = 5;  // Gap tối thiểu vẫn chơi được

        // === PHYSICS PRECISE POSITION ===
        // BirdYf giữ vị trí float cho vật lý delta time; BirdY là int để render/collision
        public float BirdYf { get; set; } = 10f;

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
            Bird.Reset();
            Difficulty.Reset();

            // Sync flat properties from sub-objects for backward compat
            BirdY = Bird.Y;
            BirdYf = Bird.Yf;
            BirdVelocity = Bird.Velocity;
            BirdAnimationFrame = Bird.AnimationFrame;
            DifficultyLevel = Difficulty.Level;
            PipeSpeed = Difficulty.PipeSpeed;

            PipeTimeAccumulator = 0f;
            Score = 0;
            GameOver = false;
            GameStarted = false;
            FrameCounter = 0;

            // Reset render tracking
            LastScore = 0;
            LastDifficultyLevel = 0;
            LastGameStarted = false;
            ForceFullRedraw = true;

            Pipes.Clear();

            for (int y = 0; y < GameHeight; y++)
                for (int x = 0; x < GameWidth; x++)
                    PreviousScreen[y, x] = ' ';
        }
        
        public void UpdateDifficulty()
        {
            if (Difficulty.Update(Score))
            {
                // Sync flat properties for callers still using them
                DifficultyLevel = Difficulty.Level;
                PipeSpeed       = Difficulty.PipeSpeed;
            }
        }

        public int GetCurrentGapSize() => Difficulty.GapSize;
    }
}
