using System.Text.Json;

namespace FlappyBird.Audio;

/// <summary>
/// Saves and loads <see cref="AudioOffsetConfig"/> to
/// %AppData%\FlappyBird\audio_offset.json (Windows) or
/// ~/.config/FlappyBird/audio_offset.json (other platforms).
/// All exceptions are silently swallowed; the caller gets a default config on failure.
/// </summary>
public static class AudioOffsetSerializer
{
    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FlappyBird", "audio_offset.json");

    private static readonly JsonSerializerOptions _opts = new() { WriteIndented = true };

    public static void Save(AudioOffsetConfig cfg)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(cfg, _opts));
        }
        catch { }
    }

    public static AudioOffsetConfig Load()
    {
        if (!File.Exists(FilePath)) return new();
        try
        {
            return JsonSerializer.Deserialize<AudioOffsetConfig>(
                       File.ReadAllText(FilePath), _opts) ?? new();
        }
        catch { return new(); }
    }
}
