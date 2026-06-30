using System.Text.Json;

namespace FlappyBird.AI.Training;

/// <summary>
/// Appends one JSON object per line (JSONL) to logs/training_{agent}.jsonl.
/// Each line is one episode. File grows indefinitely — truncate manually to reset.
/// </summary>
public static class AITrainingLogger
{
    private static readonly string LogDir = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "logs");

    private static string LogPath(string agentName) =>
        Path.Combine(LogDir, $"training_{agentName}.jsonl");

    private static readonly JsonSerializerOptions _opts = new()
    {
        WriteIndented = false,
    };

    public static void Append(string agentName, EpisodeMetrics m)
    {
        try
        {
            Directory.CreateDirectory(LogDir);
            File.AppendAllText(LogPath(agentName),
                JsonSerializer.Serialize(m, _opts) + "\n");
        }
        catch { /* never break game loop */ }
    }

    public static List<EpisodeMetrics> LoadAll(string agentName)
    {
        var results = new List<EpisodeMetrics>();
        try
        {
            string path = LogPath(agentName);
            if (!File.Exists(path)) return results;
            foreach (var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var m = JsonSerializer.Deserialize<EpisodeMetrics>(line, _opts);
                if (m != null) results.Add(m);
            }
        }
        catch { /* return partial results */ }
        return results;
    }

    /// <summary>Compute summary stats from the last N episodes.</summary>
    public static (float avgScore, float avgReward, int bestScore) Summary(
        List<EpisodeMetrics> history, int window = 20)
    {
        if (history.Count == 0) return (0, 0, 0);
        var tail = history.TakeLast(window).ToList();
        float avg   = (float)tail.Average(m => m.Score);
        float avgR  = (float)tail.Average(m => m.TotalReward);
        int   best  = history.Max(m => m.Score);
        return (avg, avgR, best);
    }
}
