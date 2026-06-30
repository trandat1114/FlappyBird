using System.Text.Json;
using FlappyBird.Models;

namespace FlappyBird.Utils;

/// <summary>
/// Persists the last 1000 play sessions to %AppData%\FlappyBird\scores.json.
/// Entries are stored newest-first; retrieval can be sorted by caller.
/// </summary>
public static class ScoreRepository
{
    private const int MaxEntries = 1000;

    private static readonly JsonSerializerOptions _opts = new() { WriteIndented = false };

    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FlappyBird", "scores.json");

    private static List<ScoreEntry>? _cache;

    public static IReadOnlyList<ScoreEntry> GetAll()
    {
        _cache ??= LoadFromDisk();
        return _cache;
    }

    public static void Add(ScoreEntry entry)
    {
        _cache ??= LoadFromDisk();
        _cache.Insert(0, entry);
        if (_cache.Count > MaxEntries)
            _cache.RemoveRange(MaxEntries, _cache.Count - MaxEntries);
        SaveToDisk(_cache);
    }

    private static List<ScoreEntry> LoadFromDisk()
    {
        if (!File.Exists(FilePath)) return [];
        try
        {
            var list = JsonSerializer.Deserialize<List<ScoreEntry>>(
                File.ReadAllText(FilePath), _opts);
            return list ?? [];
        }
        catch { return []; }
    }

    private static void SaveToDisk(List<ScoreEntry> list)
    {
        try
        {
            string dir = Path.GetDirectoryName(FilePath)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(list, _opts));
        }
        catch { }
    }
}
