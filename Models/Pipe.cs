namespace FlappyBird.Models
{
    /// <summary>
    /// Class đại diện cho một ống (pipe) trong game
    /// </summary>
    public class Pipe
    {
        public int  X           { get; set; }
        public int  TopHeight   { get; set; }
        public int  BottomHeight { get; set; }
        public int  GapSize     { get; set; }
        /// <summary>True once the bird has passed this pipe's right edge — prevents double-scoring.</summary>
        public bool Scored      { get; set; } = false;
        
        public Pipe(int x, int gapSize, int gameHeight, Random random)
        {
            X = x;
            GapSize = gapSize;
            
            // Tối ưu vị trí ống cho khung height - tạo trải nghiệm cân bằng
            // Đảm bảo gap luôn ở vùng có thể chơi được (tránh quá gần viền)
            int minTopHeight = Math.Max(3, gameHeight / 5); // Ít nhất 3 pixel từ viền trên
            int maxTopHeight = Math.Min(gameHeight - GapSize - 4, (gameHeight * 3) / 4); // Ít nhất 4 pixel từ viền dưới
            
            TopHeight = random.Next(minTopHeight, maxTopHeight);
            BottomHeight = gameHeight - TopHeight - GapSize;
        }
    }
}
