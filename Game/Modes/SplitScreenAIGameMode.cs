using FlappyBird.AI;
using FlappyBird.Localization;
using FlappyBird.Models;
using FlappyBird.Settings;

namespace FlappyBird.Game.Modes
{
    public class SplitScreenAIGameMode : GameModeBase
    {
        private readonly GameState ai1State = new();
        private readonly GameState ai2State = new();
        private bool gameStarted = false;

        private bool _renderInitialized = false;
        private int  _lastScore1 = -1, _lastScore2 = -1;
        private bool _lastOver1,       _lastOver2;

        public override void Initialize()
        {
            ai1State.Reset(); ai2State.Reset();
            ai1State.GodMode = ai2State.GodMode = true;
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
                ai1State.Score    != _lastScore1 || ai2State.Score    != _lastScore2 ||
                ai1State.GameOver != _lastOver1  || ai2State.GameOver != _lastOver2;

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
                UpdateScoreLines();
                _lastScore1 = ai1State.Score;  _lastScore2 = ai2State.Score;
                _lastOver1  = ai1State.GameOver; _lastOver2  = ai2State.GameOver;
            }
        }

        private void DrawStatic()
        {
            var panel = GameSettings.Instance.CreatePanel(66);
            panel.PrintTop();
            panel.PrintTitle(L.Get(L.SPLIT_TITLE));
            panel.PrintSep();
            Console.WriteLine(ScoreLine(ai1State, L.Get(L.AI_CONSERVATIVE))); // row 3
            Console.WriteLine(ScoreLine(ai2State, L.Get(L.AI_AGGRESSIVE)));   // row 4
            panel.PrintSep();
            Console.WriteLine(panel.BuildEmptyRow());                          // row 6
            panel.PrintBottom();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(L.Get(L.SPLIT_START_HINT));                     // row 9
            Console.ResetColor();
        }

        private void UpdateScoreLines()
        {
            var panel = GameSettings.Instance.CreatePanel(66);

            Console.SetCursorPosition(0, 3);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(ScoreLine(ai1State, L.Get(L.AI_CONSERVATIVE)));

            Console.SetCursorPosition(0, 4);
            Console.Write(ScoreLine(ai2State, L.Get(L.AI_AGGRESSIVE)));

            Console.SetCursorPosition(0, 6);
            if (ai1State.GameOver && ai2State.GameOver)
            {
                string winner = ai1State.Score > ai2State.Score ? L.Get(L.AI_CONSERVATIVE) :
                                ai2State.Score > ai1State.Score ? L.Get(L.AI_AGGRESSIVE)   : L.Get(L.AI_TIE);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(panel.BuildRow($"  *** {L.Get(L.AI_FINAL_WINNER)}: {winner} ***"));
                Console.SetCursorPosition(0, 9);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(L.Get(L.SPLIT_RESTART_HINT).PadRight(60));
            }
            else if (!gameStarted)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(panel.BuildRow($"  {L.Get(L.AI_START_HINT)}"));
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(panel.BuildRow($"  {L.Get(L.AI_RUNNING)}"));
            }

            Console.ResetColor();
        }

        private string ScoreLine(GameState st, string name)
        {
            var    panel  = GameSettings.Instance.CreatePanel(66);
            string pts    = L.Get(L.AI_SCORE_LINE);
            string status = st.GameOver ? $"({L.Get(L.AI_STATUS_LOST)})" : "";
            return panel.BuildRow($"  {name}: {st.Score,3} {pts} {status}");
        }

        public override void HandleInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Spacebar:
                    if (!gameStarted)
                        gameStarted = ai1State.GameStarted = ai2State.GameStarted = true;
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
