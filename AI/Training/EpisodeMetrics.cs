namespace FlappyBird.AI.Training;

/// <summary>One episode's training outcome, serialised to JSONL by AITrainingLogger.</summary>
public record EpisodeMetrics(
    int    Episode,
    int    Score,
    int    SurvivalFrames,
    int    PipesPassed,
    string DeathCause,     // "top_pipe" | "bottom_pipe" | "floor" | "ceiling"
    float  TotalReward,
    float  Epsilon,        // exploration rate at episode end (Q-agents only; 0 for rule-based)
    int    QTableSize,     // unique states visited (Q-agents only; 0 for rule-based)
    DateTime Timestamp
);
