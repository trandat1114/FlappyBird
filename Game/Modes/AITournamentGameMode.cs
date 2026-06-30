using System;
using System.Collections.Generic;
using FlappyBird.Models;

namespace FlappyBird.Game.Modes
{
    /// <summary>
    /// Chế độ giải đấu AI với nhiều thuật toán
    /// </summary>
    public class AITournamentGameMode : GameModeBase
    {
        private List<TournamentAI> participants = new();
        private int currentMatch = 0;
        private bool tournamentStarted = false;
        
        public class TournamentAI(string name, string strategy)
        {
            public string Name { get; set; } = name;
            public string Strategy { get; set; } = strategy;
            public int Wins { get; set; } = 0;
            public int TotalScore { get; set; } = 0;
            public GameState State { get; set; } = new GameState();
        }

        public override void Initialize()
        {
            participants = new List<TournamentAI>
            {
                new("Conservative AI", "Conservative"),
                new("Aggressive AI", "Aggressive"),
                new("Balanced AI", "Balanced"),
                new("Learning AI", "Learning")
            };
            
            currentMatch = 0;
            tournamentStarted = false;
            
            foreach (var ai in participants)
            {
                ai.State.Reset();
                ai.State.GodMode = true;
            }
        }
        
        public override void Update()
        {
            if (!tournamentStarted) return;
            
            // Tournament logic will be implemented later
            // For now, just placeholder
        }
        
        public override void Render()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("*** AI TOURNAMENT MODE ***");
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        GIẢI ĐẤU AI                             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🚧 ĐANG PHÁT TRIỂN - Các tính năng sắp có:");
            Console.WriteLine("   • Round-robin tournament");
            Console.WriteLine("   • Multiple AI strategies");
            Console.WriteLine("   • Detailed statistics");
            Console.WriteLine("   • Bracket visualization");
            Console.WriteLine();
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Participants:");
            foreach (var ai in participants)
            {
                Console.WriteLine($"   • {ai.Name} ({ai.Strategy})");
            }
            
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("ESC: Quay về menu");
            Console.ResetColor();
        }
        
        public override void HandleInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Escape:
                    shouldExit = true;
                    break;
            }
        }
        
        public override bool IsGameOver()
        {
            return shouldExit;
        }
    }
}
