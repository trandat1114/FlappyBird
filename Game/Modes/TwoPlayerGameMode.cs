using FlappyBird.Models;
using FlappyBird.Game.Modes.TwoPlayer;

namespace FlappyBird.Game.Modes
{
    /// <summary>
    /// Chế độ chơi 2 người - Dual Screen Layout với thiết kế nhất quán và tối ưu rendering
    /// </summary>
    public class TwoPlayerGameMode : GameModeBase
    {
        private readonly GameState player1State = new();
        private readonly GameState player2State = new();
        private bool gameStarted = false;

        // === MENU CONSISTENCY CONSTANTS ===
        private const int MENU_BORDER_WIDTH = 66;  // Khớp chính xác với menu border

        // === COMPONENTS ===
        private readonly TwoPlayerBuffer buffer;
        private readonly TwoPlayerRenderer renderer;
        private readonly TwoPlayerGameOverMenu gameOverMenu;
        private readonly TwoPlayerCountdown countdown;
        private readonly TwoPlayerInputHandler inputHandler;
        
        private bool firstRender = true; // Track first render to clear screen

        // === CONSTRUCTOR ===
        public TwoPlayerGameMode()
        {
            buffer = new TwoPlayerBuffer();
            buffer.InitializeBuffers();
            
            renderer = new TwoPlayerRenderer(buffer);
            gameOverMenu = new TwoPlayerGameOverMenu(buffer);
            countdown = new TwoPlayerCountdown();
            inputHandler = new TwoPlayerInputHandler(gameOverMenu, countdown);
        }

        public override void Initialize()
        {
            // Validate menu consistency như SinglePlayerGameMode
            ValidateMenuConsistency();

            player1State.Reset();
            player2State.Reset();

            // Reset components
            gameOverMenu.ResetGameOverMenu();
            countdown.ResetCountdown();

            // Initialize pipes cho cả hai players như SinglePlayerGameMode
            InitializeGameForPlayer(player1State);
            InitializeGameForPlayer(player2State);

            gameStarted = false;
            firstRender = true; // Reset first render flag
        }

        /// <summary>
        /// Validate rằng game dimensions khớp hoàn toàn với menu - tương tự SinglePlayerGameMode
        /// </summary>
        private void ValidateMenuConsistency()
        {
            if (GameState.GameWidth != MENU_BORDER_WIDTH)
            {
                throw new InvalidOperationException($"Game width ({GameState.GameWidth}) does not match menu border width ({MENU_BORDER_WIDTH})");
            }
        }

        /// <summary>
        /// Initialize game cho một player - dùng logic từ SinglePlayerGameMode
        /// </summary>
        private void InitializeGameForPlayer(GameState playerState)
        {
            playerState.Pipes.Clear();

            // Tạo pipe đầu tiên với spacing phù hợp với border width
            int initialPipeX = GameState.GameWidth - 1;
            playerState.Pipes.Add(new Pipe(initialPipeX, GameState.BaseGapSize, GameState.GameHeight, Random));

            // Set last pipe position để spacing đều đặn
            playerState.LastPipeX = initialPipeX;
        }

        public override void Update()
        {
            // Xử lý countdown trước khi bắt đầu game
            if (countdown.IsCountingDown)
            {
                bool countdownFinished = countdown.UpdateCountdown();
                if (countdownFinished)
                {
                    gameStarted = true;
                    player1State.GameStarted = true;
                    player2State.GameStarted = true;
                }
                return;
            }
            
            if (!gameStarted) return;

            // Update both players
            UpdatePlayer(player1State);
            UpdatePlayer(player2State);

            // Kiểm tra game over và hiển thị menu thay vì thoát ngay - như SinglePlayerGameMode
            if (AreBothPlayersGameOver() && !gameOverMenu.ShowGameOverMenu)
            {
                gameOverMenu.StartGameOverMenu();
            }
        }

        private void UpdatePlayer(GameState playerState)
        {
            if (playerState.GameOver) return;

            playerState.FrameCounter++;
            playerState.UpdateDifficulty();

            UpdateBirdPhysics(playerState);
            UpdatePipes(playerState);
            CheckCollision(playerState);
        }

        public override void Render()
        {
            // Clear screen completely on first render to remove previous menu
            if (firstRender)
            {
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                firstRender = false;
            }
            
            // Initialize buffers if needed
            if (!buffer.BufferInitialized)
            {
                buffer.InitializeBuffers();
            }
            
            // Clear current buffer for new frame
            buffer.ClearCurrentBuffer();
            
            // Nếu đang hiển thị game over menu - tương tự SinglePlayerGameMode
            if (gameOverMenu.ShowGameOverMenu)
            {
                // Luôn render panels để giữ game state hiển thị
                renderer.RenderDualStackedScreensToBuffer(player1State, player2State);

                // Cho phép người chơi nhìn thấy kết quả một chút trước khi hiển thị menu
                if (DateTime.Now - gameOverMenu.GameOverTime > TimeSpan.FromMilliseconds(800))
                {
                    gameOverMenu.RenderGameOverMenuToBuffer(player1State, player2State);
                }
                else
                {
                    renderer.RenderGameOverOverlayToBuffer();
                }
            }
            else if (countdown.IsCountingDown)
            {
                // Render hai màn hình như bình thường
                renderer.RenderDualStackedScreensToBuffer(player1State, player2State);
                // Hiển thị số countdown lớn ở giữa mỗi màn hình player
                renderer.RenderCountdownOverlayToBuffer(countdown.CountdownValue);
                // Render footer
                renderer.RenderDualPlayerFooterToBuffer(player1State, player2State);
            }
            else
            {
                // Render dual stacked screens với thiết kế nhất quán
                renderer.RenderDualStackedScreensToBuffer(player1State, player2State);
                // Render footer với thông tin cả hai player
                renderer.RenderDualPlayerFooterToBuffer(player1State, player2State);
            }
            
            // Flush buffer to console - only changed characters
            buffer.FlushBufferToConsole();
        }

        public override void HandleInput(ConsoleKeyInfo keyInfo)
        {
            var inputResult = inputHandler.HandleInput(keyInfo, gameStarted, player1State, player2State);

            if (inputResult.ShouldExit)
            {
                shouldExit = true;
                return;
            }

            if (inputResult.ShouldRestart)
            {
                RestartGame();
                return;
            }

            if (inputResult.ShouldStartCountdown)
            {
                countdown.StartCountdown();
            }

            if (inputResult.Player1Jump)
            {
                Jump(player1State);
            }

            if (inputResult.Player2Jump)
            {
                Jump(player2State);
            }
        }

        /// <summary>
        /// Restart game với cùng cài đặt - tương tự SinglePlayerGameMode
        /// </summary>
        private void RestartGame()
        {
            gameOverMenu.ResetGameOverMenu();
            player1State.Reset();
            player2State.Reset();

            // Khởi tạo lại game với border dimensions chính xác
            InitializeGameForPlayer(player1State);
            InitializeGameForPlayer(player2State);

            gameStarted = false;
            firstRender = true; // Reset để clear screen khi restart
        }

        /// <summary>
        /// Jump method cho player - tương tự SinglePlayerGameMode
        /// </summary>
        protected new void Jump(GameState playerState)
        {
            if (!playerState.GameStarted)
            {
                playerState.GameStarted = true;
            }

            playerState.BirdVelocity = GameState.JumpStrength;
        }

        public override bool IsGameOver()
        {
            // Game chỉ kết thúc khi người chơi chọn thoát (shouldExit = true) - như SinglePlayerGameMode
            // Không kết thúc khi cả hai player GameOver = true vì lúc đó chúng ta đang hiển thị game over menu
            return shouldExit;
        }

        /// <summary>
        /// Check nếu cả hai player đã game over để trigger menu
        /// </summary>
        private bool AreBothPlayersGameOver()
        {
            return player1State.GameOver && player2State.GameOver;
        }
    }
}
