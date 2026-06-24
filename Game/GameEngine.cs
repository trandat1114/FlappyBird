using System.Diagnostics;
using FlappyBird.Enum;
using FlappyBird.Settings;

namespace FlappyBird.Game
{
    public static class GameEngine
    {
        private static IGameMode? currentGameMode;
        private static Thread? gameThread;
        private static Thread? inputThread;
        private static volatile bool isRunning = false;

        public static void StartGame(GameMode gameMode)
        {
            Console.Clear();
            Console.ResetColor();
            Console.SetCursorPosition(0, 0);

            currentGameMode = GameModeFactory.CreateGameMode(gameMode);
            currentGameMode.Initialize();

            isRunning = true;
            gameThread = new Thread(GameLoop) { IsBackground = true };
            inputThread = new Thread(InputLoop) { IsBackground = true };

            gameThread.Start();
            inputThread.Start();

            gameThread.Join();
            inputThread.Join();

            currentGameMode.Cleanup();
        }

        public static void StopGame() => isRunning = false;

        /// <summary>
        /// Game loop với delta time chuẩn xác.
        /// Dùng Stopwatch độ phân giải cao, hybrid sleep/spinwait để đạt đúng 60fps
        /// mà không chiếm quá nhiều CPU.
        /// </summary>
        private static void GameLoop()
        {
            int  fps              = GameSettings.Instance.TargetFps;
            var  sw               = Stopwatch.StartNew();
            long lastTicks        = sw.ElapsedTicks;
            long targetTicks      = Stopwatch.Frequency / fps;
            // At 120fps the frame time (8.33ms) is shorter than Sleep(1) resolution (~15ms),
            // so we skip sleeping entirely and always use SpinWait for precise timing.
            long spinThresholdTicks = fps >= 120
                ? targetTicks               // never sleep at 120fps
                : Stopwatch.Frequency / 400; // ~2.5ms sleep threshold at 60fps

            while (isRunning && currentGameMode != null && !currentGameMode.IsGameOver())
            {
                long now = sw.ElapsedTicks;
                long elapsed = now - lastTicks;

                if (elapsed >= targetTicks)
                {
                    // Tính deltaTime (giây), giới hạn ở 50ms để tránh spiral-of-death
                    float dt = Math.Min((float)elapsed / Stopwatch.Frequency, 0.05f);
                    GameTiming.DeltaTime = dt;

                    currentGameMode.Update();
                    currentGameMode.Render();
                    lastTicks = now;
                }
                else
                {
                    long remaining = targetTicks - (sw.ElapsedTicks - lastTicks);
                    if (remaining > spinThresholdTicks)
                        Thread.Sleep(1); // yield CPU khi còn thời gian đáng kể
                    else
                        Thread.SpinWait(50); // busy-wait ngắn cho độ chính xác cao
                }
            }

            isRunning = false;
        }

        /// <summary>
        /// Input loop tách riêng – 5ms sleep giảm latency từ 10ms xuống còn ~5ms
        /// mà vẫn không block game loop.
        /// </summary>
        private static void InputLoop()
        {
            while (isRunning && currentGameMode != null && !currentGameMode.IsGameOver())
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    currentGameMode.HandleInput(key);
                }
                Thread.Sleep(5);
            }
        }
    }
}
