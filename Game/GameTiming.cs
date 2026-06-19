namespace FlappyBird.Game;

/// <summary>
/// Cung cấp thông tin thời gian cho game loop - tránh vật lý phụ thuộc frame rate
/// </summary>
public static class GameTiming
{
    public const float TargetFrameTime = 1.0f / 60.0f; // 16.67ms

    /// <summary>
    /// Thời gian của frame hiện tại (giây), được set bởi GameEngine mỗi frame
    /// </summary>
    public static float DeltaTime { get; set; } = TargetFrameTime;

    /// <summary>
    /// Hệ số scale so với 60fps: 1.0 tại 60fps, 2.0 tại 30fps
    /// Dùng để scale vật lý cho ổn định trên mọi tốc độ máy
    /// </summary>
    public static float Scale => DeltaTime / TargetFrameTime;
}
