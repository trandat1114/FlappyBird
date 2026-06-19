using FlappyBird.AI;
using FlappyBird.Localization;
using FlappyBird.Models;

namespace FlappyBird.Game.Modes
{
    public class DualAIGameMode : GameModeBase
    {
        private readonly GameState ai1State = new();
        private readonly GameState ai2State = new();
        private bool gameStarted = false;
        private readonly int testRounds = 10;
        private int currentRound = 1;
        private int ai1Wins = 0;
        private int ai2Wins = 0;

        // Render state – tránh Console.Clear mỗi frame
        private bool _renderInitialized = false;
        private int _lastAi1Score = -1, _lastAi2Score = -1;
        private bool _lastAi1Over, _lastAi2Over;
        private int _lastRound = -1;
        private bool _lastStarted;

        public override void Initialize()
        {
            _renderInitialized = false; // force redraw khi vào mode
            ResetRound();
        }

        private void ResetRound()
        {
            ai1State.Reset();
            ai2State.Reset();
            ai1State.GodMode = true;
            ai2State.GodMode = true;
            ai1State.Pipes.Add(new Pipe(GameState.GameWidth - 1, GameState.BaseGapSize, GameState.GameHeight, Random));
            ai2State.Pipes.Add(new Pipe(GameState.GameWidth - 1, GameState.BaseGapSize, GameState.GameHeight, Random));
            gameStarted = false;
            // Reset render tracker
            _lastAi1Score = -1; _lastAi2Score = -1;
        }

        public override void Update()
        {
            if (!gameStarted) return;
            UpdateAI(ai1State, conservative: true);
            UpdateAI(ai2State, conservative: false);
            if (ai1State.GameOver && ai2State.GameOver)
                CompleteRound();
        }

        private void UpdateAI(GameState st, bool conservative)
        {
            if (st.GameOver) return;
            st.FrameCounter++;
            st.UpdateDifficulty();
            if (conservative) GodModeAI.AutoControlBird(st);
            else              GodModeAI.AutoControlBirdAggressive(st);
            UpdateBirdPhysics(st);
            UpdatePipes(st);
            CheckCollision(st);
        }

        private void CompleteRound()
        {
            if (ai1State.Score > ai2State.Score) ai1Wins++;
            else if (ai2State.Score > ai1State.Score) ai2Wins++;
            currentRound++;
            if (currentRound <= testRounds)
            {
                Thread.Sleep(1000);
                ResetRound();
                gameStarted = true;
                ai1State.GameStarted = true;
                ai2State.GameStarted = true;
            }
        }

        public override void Render()
        {
            bool statsChanged =
                ai1State.Score != _lastAi1Score || ai2State.Score != _lastAi2Score ||
                ai1State.GameOver != _lastAi1Over || ai2State.GameOver != _lastAi2Over ||
                currentRound != _lastRound || gameStarted != _lastStarted;

            if (!_renderInitialized)
            {
                // Frame tĩnh – vẽ một lần
                Console.Clear();
                Console.CursorVisible = false;
                DrawStaticFrame();
                _renderInitialized = true;
                statsChanged = true; // force update dòng số liệu
            }

            if (statsChanged)
            {
                UpdateDynamicLines();
                _lastAi1Score = ai1State.Score;
                _lastAi2Score = ai2State.Score;
                _lastAi1Over = ai1State.GameOver;
                _lastAi2Over = ai2State.GameOver;
                _lastRound = currentRound;
                _lastStarted = gameStarted;
            }
        }

        private void DrawStaticFrame()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        AI COMPARISON                          ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
            Console.WriteLine(BuildRoundLine());
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
            Console.WriteLine(BuildAIStatLine(ai1State, L.Get(L.AI_CONSERVATIVE)));
            Console.WriteLine(BuildAIStatLine(ai2State, L.Get(L.AI_AGGRESSIVE)));
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(L.Get(L.AI_SUMMARY_TITLE));
            int ties0 = Math.Max(0, currentRound - 1 - ai1Wins - ai2Wins);
            Console.WriteLine(BuildWinsLine(L.Get(L.AI_CONS_WINS), ai1Wins));
            Console.WriteLine(BuildWinsLine(L.Get(L.AI_AGG_WINS), ai2Wins));
            Console.WriteLine(BuildWinsLine(L.Get(L.AI_TIES), ties0));
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(L.Get(L.AI_START_HINT));
            Console.ResetColor();
        }

        private void UpdateDynamicLines()
        {
            // Row 3: round title
            Console.SetCursorPosition(0, 3);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(BuildRoundLine());

            // Rows 5-6: AI stats
            Console.SetCursorPosition(0, 5);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(BuildAIStatLine(ai1State, L.Get(L.AI_CONSERVATIVE)));
            Console.SetCursorPosition(0, 6);
            Console.Write(BuildAIStatLine(ai2State, L.Get(L.AI_AGGRESSIVE)));

            // Rows 10-12: win counters
            int ties = Math.Max(0, currentRound - 1 - ai1Wins - ai2Wins);
            Console.SetCursorPosition(0, 10);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(BuildWinsLine(L.Get(L.AI_CONS_WINS), ai1Wins).PadRight(44));
            Console.SetCursorPosition(0, 11);
            Console.Write(BuildWinsLine(L.Get(L.AI_AGG_WINS), ai2Wins).PadRight(44));
            Console.SetCursorPosition(0, 12);
            Console.Write(BuildWinsLine(L.Get(L.AI_TIES), ties).PadRight(44));

            // Row 14: bottom hint – PadRight clears leftover chars from previous state
            Console.SetCursorPosition(0, 14);
            if (currentRound > testRounds)
            {
                string aiWinner = ai1Wins > ai2Wins ? L.Get(L.AI_CONSERVATIVE) :
                                  ai2Wins > ai1Wins ? L.Get(L.AI_AGGRESSIVE)   : L.Get(L.AI_TIE);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(($"*** {L.Get(L.AI_FINAL_WINNER)}: {aiWinner} ***  R: Restart  ESC: Menu").PadRight(60));
            }
            else if (!gameStarted)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(L.Get(L.AI_START_HINT).PadRight(60));
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(L.Get(L.AI_RUNNING).PadRight(60));
            }

            Console.ResetColor();
        }

        // ── LINE BUILDERS – chiều rộng cố định 66 char để không vỡ layout ──
        private string BuildRoundLine()
        {
            string con = L.Get(L.AI_CONSERVATIVE);
            string agg = L.Get(L.AI_AGGRESSIVE);
            string content = $"  {L.Get(L.AI_ROUND_TITLE)} {currentRound,2}/{testRounds}  -  {con}  vs  {agg}";
            return ("║" + content).PadRight(65) + "║";
        }

        private string BuildAIStatLine(GameState st, string name)
        {
            string pts    = L.Get(L.AI_SCORE_LINE);
            string status = st.GameOver ? L.Get(L.AI_STATUS_LOST) : L.Get(L.AI_STATUS_PLAYING);
            string content = $"  {name}: {st.Score,3} {pts}  |  Status: {status}";
            return ("║" + content).PadRight(65) + "║";
        }

        private static string BuildWinsLine(string label, int count) =>
            $"   {label}: {count,2}";

        public override void HandleInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Spacebar:
                    if (!gameStarted && currentRound <= testRounds)
                    {
                        gameStarted = true;
                        ai1State.GameStarted = true;
                        ai2State.GameStarted = true;
                    }
                    break;
                case ConsoleKey.R:
                    if (currentRound > testRounds)
                    {
                        currentRound = 1; ai1Wins = 0; ai2Wins = 0;
                        _renderInitialized = false;
                        Initialize();
                    }
                    break;
                case ConsoleKey.Escape:
                    shouldExit = true;
                    break;
            }
        }

        public override bool IsGameOver() => shouldExit;
    }
}
