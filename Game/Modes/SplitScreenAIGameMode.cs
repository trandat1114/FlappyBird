using FlappyBird.AI;
using FlappyBird.Localization;
using FlappyBird.Models;

namespace FlappyBird.Game.Modes
{
    public class SplitScreenAIGameMode : GameModeBase
    {
        private readonly GameState ai1State = new();
        private readonly GameState ai2State = new();
        private bool gameStarted = false;

        private bool _renderInitialized = false;
        private int _lastScore1 = -1, _lastScore2 = -1;
        private bool _lastOver1, _lastOver2;

        public override void Initialize()
        {
            ai1State.Reset(); ai2State.Reset();
            ai1State.GodMode = true; ai2State.GodMode = true;
            ai1State.Pipes.Add(new Pipe(GameState.GameWidth - 1, GameState.BaseGapSize, GameState.GameHeight, Random));
            ai2State.Pipes.Add(new Pipe(GameState.GameWidth - 1, GameState.BaseGapSize, GameState.GameHeight, Random));
            gameStarted = false;
            _renderInitialized = false;
            _lastScore1 = _lastScore2 = -1;
        }

        public override void Update()
        {
            if (!gameStarted) return;

            if (!ai1State.GameOver)
            {
                ai1State.FrameCounter++;
                ai1State.UpdateDifficulty();
                GodModeAI.AutoControlBird(ai1State);
                UpdateBirdPhysics(ai1State);
                UpdatePipes(ai1State);
                CheckCollision(ai1State);
            }

            if (!ai2State.GameOver)
            {
                ai2State.FrameCounter++;
                ai2State.UpdateDifficulty();
                GodModeAI.AutoControlBirdAggressive(ai2State);
                UpdateBirdPhysics(ai2State);
                UpdatePipes(ai2State);
                CheckCollision(ai2State);
            }
        }

        public override void Render()
        {
            bool changed =
                ai1State.Score != _lastScore1 || ai2State.Score != _lastScore2 ||
                ai1State.GameOver != _lastOver1 || ai2State.GameOver != _lastOver2;

            if (!_renderInitialized)
            {
                Console.Clear();
                Console.CursorVisible = false;
                DrawStaticFrame();
                _renderInitialized = true;
                changed = true;
            }

            if (changed)
            {
                UpdateScoreLines();
                _lastScore1 = ai1State.Score;
                _lastScore2 = ai2State.Score;
                _lastOver1 = ai1State.GameOver;
                _lastOver2 = ai2State.GameOver;
            }
        }

        private void DrawStaticFrame()
        {
            // Title – centered, always 66 chars
            string title = L.Get(L.SPLIT_TITLE);
            string centeredTitle = title.PadLeft((64 + title.Length) / 2).PadRight(64);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║{centeredTitle}║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
            Console.WriteLine(BuildAIScoreLine(ai1State, L.Get(L.AI_CONSERVATIVE)));  // row 3
            Console.WriteLine(BuildAIScoreLine(ai2State, L.Get(L.AI_AGGRESSIVE)));    // row 4
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║                                                                ║"); // row 6
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(L.Get(L.SPLIT_START_HINT));  // row 9
            Console.ResetColor();
        }

        private void UpdateScoreLines()
        {
            string pts = L.Get(L.AI_SCORE_LINE);
            string gameOver = L.Get(L.AI_STATUS_LOST);

            Console.SetCursorPosition(0, 3);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(BuildAIScoreLine(ai1State, L.Get(L.AI_CONSERVATIVE)));

            Console.SetCursorPosition(0, 4);
            Console.Write(BuildAIScoreLine(ai2State, L.Get(L.AI_AGGRESSIVE)));

            Console.SetCursorPosition(0, 6);
            if (ai1State.GameOver && ai2State.GameOver)
            {
                string winner = ai1State.Score > ai2State.Score ? L.Get(L.AI_CONSERVATIVE) :
                                ai2State.Score > ai1State.Score ? L.Get(L.AI_AGGRESSIVE)   : L.Get(L.AI_TIE);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(("║  *** " + L.Get(L.AI_FINAL_WINNER) + ": " + winner + " ***").PadRight(65) + "║");
                Console.SetCursorPosition(0, 9);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(L.Get(L.SPLIT_RESTART_HINT).PadRight(60));
            }
            else if (!gameStarted)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(("║  " + L.Get(L.AI_START_HINT)).PadRight(65) + "║");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(("║  " + L.Get(L.AI_RUNNING)).PadRight(65) + "║");
            }

            Console.ResetColor();
        }

        // 66-char border line: ║ + 64 content + ║
        private string BuildAIScoreLine(GameState st, string name)
        {
            string pts    = L.Get(L.AI_SCORE_LINE);
            string status = st.GameOver ? $"({L.Get(L.AI_STATUS_LOST)})" : "";
            string content = $"  {name}: {st.Score,3} {pts} {status}";
            return ("║" + content).PadRight(65) + "║";
        }

        public override void HandleInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Spacebar:
                    if (!gameStarted)
                    {
                        gameStarted = true;
                        ai1State.GameStarted = true;
                        ai2State.GameStarted = true;
                    }
                    break;
                case ConsoleKey.R:
                    if (ai1State.GameOver && ai2State.GameOver)
                        Initialize();
                    break;
                case ConsoleKey.Escape:
                    shouldExit = true;
                    break;
            }
        }

        public override bool IsGameOver() => shouldExit;
    }
}
