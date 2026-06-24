using System.Text.Json;
using System.Text.Json.Serialization;
using FlappyBird.Localization;
using FlappyBird.UI.Border;

namespace FlappyBird.Settings;

/// <summary>
/// Saves / loads <see cref="GameSettings"/> to JSON.
/// %AppData%\FlappyBird\settings.json (Windows) / ~/.config/FlappyBird/settings.json (other).
/// </summary>
public static class SettingsSerializer
{
    private static readonly JsonSerializerOptions _opts = new()
    {
        WriteIndented = true,
        Converters    = { new JsonStringEnumConverter() },
    };

    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FlappyBird", "settings.json");

    private sealed class Dto
    {
        public string Language      { get; set; } = "English";
        public bool   MusicEnabled  { get; set; } = true;
        public int    MusicVolume   { get; set; } = 80;
        public int    EffectsVolume { get; set; } = 60;
        public string BorderStyle   { get; set; } = "Double";
        public string PrimaryColor  { get; set; } = "Cyan";
        public string AccentColor   { get; set; } = "Yellow";
        public string FontFaceName  { get; set; } = "";
        public int    FontSize      { get; set; } = 16;
    }

    public static void Save(GameSettings s)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            var dto = new Dto
            {
                Language      = s.Language.ToString(),
                MusicEnabled  = s.MusicEnabled,
                MusicVolume   = s.MusicVolume,
                EffectsVolume = s.EffectsVolume,
                BorderStyle   = s.BorderStyle.ToString(),
                PrimaryColor  = s.PrimaryColor.ToString(),
                AccentColor   = s.AccentColor.ToString(),
                FontFaceName  = s.FontFaceName,
                FontSize      = s.FontSize,
            };
            File.WriteAllText(FilePath, JsonSerializer.Serialize(dto, _opts));
        }
        catch { }
    }

    public static void Load(GameSettings s)
    {
        if (!File.Exists(FilePath)) return;
        try
        {
            var dto = JsonSerializer.Deserialize<Dto>(File.ReadAllText(FilePath), _opts);
            if (dto is null) return;

            if (System.Enum.TryParse<Language>(dto.Language, out var lang))        s.Language     = lang;
            if (System.Enum.TryParse<BorderStyle>(dto.BorderStyle, out var bs))    s.BorderStyle  = bs;
            if (System.Enum.TryParse<ConsoleColor>(dto.PrimaryColor, out var pc))  s.PrimaryColor = pc;
            if (System.Enum.TryParse<ConsoleColor>(dto.AccentColor,  out var ac))  s.AccentColor  = ac;
            s.MusicEnabled  = dto.MusicEnabled;
            s.MusicVolume   = Math.Clamp(dto.MusicVolume,   0, 100);
            s.EffectsVolume = Math.Clamp(dto.EffectsVolume, 0, 100);
            s.FontFaceName  = dto.FontFaceName;
            s.FontSize      = Math.Clamp(dto.FontSize, 8, 72);
            s.Apply();
        }
        catch { }
    }
}
