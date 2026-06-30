using FlappyBird.Audio;
using FlappyBird.Audio.Enum;
using FlappyBird.Models;

namespace FlappyBird.Game
{
    public abstract class GameModeBase : IGameMode
    {
        protected bool shouldExit = false;
        protected readonly Random Random = new();

        public abstract void Initialize();
        public abstract void Update();
        public abstract void Render();
        public abstract void HandleInput(ConsoleKeyInfo keyInfo);
        public abstract bool IsGameOver();

        public virtual void Cleanup()
        {
            shouldExit = false;
        }

        /// <summary>
        /// Vật lý chim theo delta time – ổn định trên mọi frame rate.
        /// BirdYf là vị trí float; BirdY (int) dùng cho render/collision.
        /// </summary>
        protected void UpdateBirdPhysics(GameState gs)
        {
            float s = GameTiming.Scale; // 1.0 tại 60fps

            gs.BirdVelocity += GameState.Gravity * s;
            if (gs.BirdVelocity > GameState.MaxFallSpeed)
                gs.BirdVelocity = GameState.MaxFallSpeed;

            gs.BirdYf += gs.BirdVelocity * s;

            // Biên trên
            if (gs.BirdYf < 1f)
            {
                gs.BirdYf = 1f;
                gs.BirdVelocity = 0f;
            }

            gs.BirdY = (int)Math.Round(gs.BirdYf);

            // Biên dưới – game over
            if (gs.BirdY >= GameState.GameHeight - 1)
            {
                gs.BirdY = GameState.GameHeight - 1;
                gs.GameOver = true;
            }
        }

        /// <summary>
        /// Di chuyển ống theo bộ tích lũy thời gian – thay thế FrameCounter%PipeSpeed.
        /// Ống di chuyển 1 char mỗi (PipeSpeed/60) giây, bất kể frame rate.
        /// </summary>
        protected void UpdatePipes(GameState gs)
        {
            float pipeInterval = gs.PipeSpeed / 60.0f; // giây giữa mỗi lần dịch 1 char
            gs.PipeTimeAccumulator += GameTiming.DeltaTime;

            while (gs.PipeTimeAccumulator >= pipeInterval)
            {
                gs.PipeTimeAccumulator -= pipeInterval;

                for (int i = gs.Pipes.Count - 1; i >= 0; i--)
                {
                    gs.Pipes[i].X--;

                    // Score the instant the bird passes the pipe's right edge (X+1 < BirdX)
                    if (!gs.Pipes[i].Scored && gs.Pipes[i].X + 1 < GameState.BirdX)
                    {
                        gs.Pipes[i].Scored = true;
                        gs.Score++;
                        AudioManager.PlaySoundEffect(SoundEffect.Score);
                    }

                    if (gs.Pipes[i].X < -2)
                        gs.Pipes.RemoveAt(i);
                }

                // Spawn ống mới nếu cần
                if (gs.Pipes.Count == 0 ||
                    gs.Pipes[gs.Pipes.Count - 1].X < GameState.GameWidth - gs.PipeSpacing)
                {
                    gs.Pipes.Add(new Pipe(GameState.GameWidth - 1,
                        gs.GetCurrentGapSize(), GameState.GameHeight, Random));
                }
            }
        }

        /// <summary>
        /// Kiểm tra va chạm – chỉ kiểm tra ống gần chim (không scan toàn bản đồ mỗi frame)
        /// </summary>
        protected void CheckCollision(GameState gs)
        {
            int bx = GameState.BirdX;
            int by = gs.BirdY;

            foreach (var pipe in gs.Pipes)
            {
                // Bỏ qua ống đã qua
                if (pipe.X < bx - 2) continue;
                // Bỏ qua ống chưa đến gần
                if (pipe.X > bx + 2) break;

                if (by <= pipe.TopHeight || by >= GameState.GameHeight - pipe.BottomHeight - 1)
                {
                    gs.GameOver = true;
                    return;
                }
            }

            if (by <= 0 || by >= GameState.GameHeight - 1)
                gs.GameOver = true;
        }

        protected void Jump(GameState gs)
        {
            if (!gs.GameStarted)
                gs.GameStarted = true;

            gs.BirdVelocity = GameState.JumpStrength;
            // Sync BirdYf để tránh float drift sau jump
            gs.BirdYf = gs.BirdY;
        }
    }
}
