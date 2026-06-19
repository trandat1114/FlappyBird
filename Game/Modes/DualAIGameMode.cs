using FlappyBird.AI;
using FlappyBird.Localization;
using FlappyBird.Models;
using FlappyBird.Settings;

namespace FlappyBird.Game.Modes
{
    public class DualAIGameMode : GameModeBase
    {
        private readonly GameState ai1State = new();
        private readonly GameState ai2State = new();
        private bool gameStarted  = false;
        private readonly int testRounds = 10;
        private int currentRound = 1;
        private int ai1Wins = 0;
        private int ai2Wins = 0;

        private bool _renderInitialized = false;
        private int  _lastAi1Score = -1, _lastAi2Score = -1;
        private bool _lastAi1Over,       _lastAi2Over;
        private int  _lastRound   = -1;
        private bool _lastStarted;

        public override void Initialize()
        {
            _renderInitialized = false;
            ResetRound();
        }

        private void ResetRound()
        {
            ai1State.Reset(); ai2State.Reset();
            ai1State.GodMode = ai2State.GodMode = true;
            ai1State.Pipes.Add(new Pipe(GameState.GameWidth - 1, GameState.BaseGapSize, GameState.GameHeight, Random));
            ai2State.Pipes.Add(new Pipe(GameState.GameWidth - 1, GameState.BaseGapSize, GameState.GameHeight, Random));
            gameStarted = false;
            _lastAi1Score = _lastAi2Score = -1;
        }

        public override void Update()
        {
            if (!gameStarted) return;
            UpdateAI(ai1State, conservative: true);
            UpdateAI(ai2State, conservative: false);
            if (ai1State.GameOver && ai2State.GameOver) CompleteRound();
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
                gameStarted = ai1State.GameStarted = ai2State.GameStarted = true;
            }
        }

        public override void Render()
        {
            bool changed =
                ai1State.Score   != _lastAi1Score || ai2State.Score   != _lastAi2Score ||
                ai1State.GameOver != _lastAi1Over  || ai2State.GameOver != _lastAi2Over  ||
                currentRound     != _lastRound     || gameStarted      != _lastStarted;

            if (!_renderInitialized)
            {
                Console.Clear();
                Console.CursorVisible = false;
                DrawStatic();
                _renderInitialized = true;
                changed = true;
            }

            if (changed)
            {
                UpdateDynamic();
                _lastAi1Score = ai1State.Score;  _lastAi2Score = ai2State.Score;
                _lastAi1Over  = ai1State.GameOver; _lastAi2Over  = ai2State.GameOver;
                _lastRound    = currentRound;      _lastStarted  = gameStarted;
            }
        }

        private void DrawStatic()
        {
            var panel = GameSettings.Instance.CreatePanel(66);
            panel.PrintTop();
            panel.PrintTitle("AI COMPARISON");
            panel.PrintSep();
            Console.WriteLine(RoundLine());          // row 3
            panel.PrintSep();
            Console.WriteLine(AiStatLine(ai1State, L.Get(L.AI_CONSERVATIVE)));  // row 5
            Console.WriteLine(AiStatLine(ai2State, L.Get(L.AI_AGGRESSIVE)));    // row 6
            panel.PrintBottom();
            Console.WriteLine();

            // Summary
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(L.Get(L.AI_SUMMARY_TITLE));
            int ties0 = Math.Max(0, currentRound - 1 - ai1Wins - ai2Wins);
            Console.WriteLine(WinsLine(L.Get(L.AI_CONS_WINS), ai1Wins));
            Console.WriteLine(WinsLine(L.Get(L.AI_AGG_WINS),  ai2Wins));
            Console.WriteLine(WinsLine(L.Get(L.AI_TIES),      ties0));
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(L.Get(L.AI_START_HINT));
            Console.ResetColor();
        }

        private void UpdateDynamic()
        {
            // Row 3: round line
            Console.SetCursorPosition(0, 3);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(RoundLine());

            // Rows 5-6: AI stat lines
            Console.SetCursorPosition(0, 5);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(AiStatLine(ai1State, L.Get(L.AI_CONSERVATIVE)));
            Console.SetCursorPosition(0, 6);
            Console.Write(AiStatLine(ai2State, L.Get(L.AI_AGGRESSIVE)));

            // Rows 10-12: win counters
            int ties = Math.Max(0, currentRound - 1 - ai1Wins - ai2Wins);
            Console.SetCursorPosition(0, 10);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(WinsLine(L.Get(L.AI_CONS_WINS), ai1Wins).PadRight(44));
            Console.SetCursorPosition(0, 11);
            Console.Write(WinsLine(L.Get(L.AI_AGG_WINS),  ai2Wins).PadRight(44));
            Console.SetCursorPosition(0, 12);
            Console.Write(WinsLine(L.Get(L.AI_TIES),      ties).PadRight(44));

            // Row 14: bottom hint
            Console.SetCursorPosition(0, 14);
            if (currentRound > testRounds)
            {
                string winner = ai1Wins > ai2Wins ? L.Get(L.AI_CONSERVATIVE) :
                                ai2Wins > ai1Wins ? L.Get(L.AI_AGGRESSIVE)   : L.Get(L.AI_TIE);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(($"*** {L.Get(L.AI_FINAL_WINNER)}: {winner} ***  R: Restart  ESC: Menu").PadRight(60));
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

        // ── Line builders — always 66 chars via UIPanel.BuildRow ─────────────

        private string RoundLine()
        {
            var panel = GameSettings.Instance.CreatePanel(66);
            string content = $"  {L.Get(L.AI_ROUND_TITLE)} {currentRound,2}/{testRounds}  -  {L.Get(L.AI_CONSERVATIVE)}  vs  {L.Get(L.AI_AGGRESSIVE)}";
            return panel.BuildRow(content);
        }

        private string AiStatLine(GameState st, string name)
        {
            var panel   = GameSettings.Instance.CreatePanel(66);
            string pts    = L.Get(L.AI_SCORE_LINE);
            string status = st.GameOver ? L.Get(L.AI_STATUS_LOST) : L.Get(L.AI_STATUS_PLAYING);
            return panel.BuildRow($"  {name}: {st.Score,3} {pts}  |  Status: {status}");
        }

        private static string WinsLine(string label, int count) => $"   {label}: {count,2}";

        public override void HandleInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Spacebar:
                    if (!gameStarted && currentRound <= testRounds)
                        gameStarted = ai1State.GameStarted = ai2State.GameStarted = true;
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
