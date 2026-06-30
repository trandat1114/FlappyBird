namespace FlappyBird.Models;

/// <summary>
/// Configuration for a Custom Game session. Either "By Level" (auto-derives Speed+Gap
/// from a starting level and lets difficulty progress) or "By Manual" (fixed Speed and
/// Gap that never change during play).
/// </summary>
public class CustomGameConfig
{
    public bool ManualMode  { get; set; } = false;
    public int  StartLevel  { get; set; } = 1;  // 1–10 (Level mode)
    public int  ManualSpeed { get; set; } = 3;  // 1–4  (Manual mode; lower = faster)
    public int  ManualGap   { get; set; } = 7;  // 5–10 (Manual mode)

    // Resolved values regardless of mode
    public int EffectiveSpeed => ManualMode ? ManualSpeed : LevelToSpeed(StartLevel);
    public int EffectiveGap   => ManualMode ? ManualGap   : LevelToGap(StartLevel);

    public static int LevelToSpeed(int lvl) => lvl switch { <= 2 => 3, <= 4 => 2, _ => 1 };
    public static int LevelToGap(int lvl)   => Math.Max(GameState.MinGapSize, GameState.BaseGapSize - lvl / 3);
    public static string SpeedName(int spd) => spd switch { 1 => "Fastest", 2 => "Fast", 3 => "Normal", _ => "Slow" };
    public static string DiffName(int lvl)  => lvl switch { <= 2 => "Easy", <= 4 => "Normal", <= 6 => "Hard", _ => "Extreme" };
}
