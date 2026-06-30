using FlappyBird.Models;
using FlappyBird.Settings;

namespace FlappyBird.UI;

/// <summary>
/// Configuration screen for Custom Game.
/// Two mutually-exclusive modes:
///   By Level  — choose 1–10 starting level; Speed + Gap auto-derived; difficulty progresses.
///   By Manual — choose Speed (1–4) and Gap (5–10) directly; difficulty is FIXED.
/// Navigation: ↑↓ between rows, ←→ to adjust, Enter to start, ESC to cancel.
/// </summary>
public static class CustomGameMenu
{
    private const int ROW_MODE  = 0;
    private const int ROW_LEVEL = 1;
    private const int ROW_SPEED = 2;
    private const int ROW_GAP   = 3;
    private const int ROW_COUNT = 4;

    private static int              _row = 0;
    private static CustomGameConfig _cfg = new();

    // ── Public entry point ────────────────────────────────────────────────────

    public static CustomGameConfig? Show()
    {
        Console.CursorVisible = false;
        _cfg = new CustomGameConfig();
        _row = 0;
        Draw();

        while (true)
        {
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.UpArrow:
                    MoveUp(); Draw(); break;

                case ConsoleKey.DownArrow:
                    MoveDown(); Draw(); break;

                case ConsoleKey.LeftArrow:
                    Adjust(-1); Draw(); break;

                case ConsoleKey.RightArrow:
                    Adjust(+1); Draw(); break;

                case ConsoleKey.Enter:
                case ConsoleKey.Spacebar:
                    // On Mode row Enter toggles, anywhere else starts the game
                    if (_row == ROW_MODE) { Adjust(+1); Draw(); }
                    else return _cfg;
                    break;

                case ConsoleKey.Escape:
                    return null;
            }
        }
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    private static bool IsLocked(int row) => row switch
    {
        ROW_LEVEL => _cfg.ManualMode,
        ROW_SPEED => !_cfg.ManualMode,
        ROW_GAP   => !_cfg.ManualMode,
        _         => false,
    };

    private static void MoveUp()
    {
        int start = _row;
        do _row = (_row - 1 + ROW_COUNT) % ROW_COUNT;
        while (IsLocked(_row) && _row != start);
    }

    private static void MoveDown()
    {
        int start = _row;
        do _row = (_row + 1) % ROW_COUNT;
        while (IsLocked(_row) && _row != start);
    }

    // ── Adjustment ───────────────────────────────────────────────────────────

    private static void Adjust(int d)
    {
        switch (_row)
        {
            case ROW_MODE:
                _cfg.ManualMode = !_cfg.ManualMode;
                // If current row just became locked, move to first unlocked
                while (IsLocked(_row)) _row = (_row + 1) % ROW_COUNT;
                break;

            case ROW_LEVEL when !_cfg.ManualMode:
                _cfg.StartLevel = Math.Clamp(_cfg.StartLevel + d, 1, 10);
                break;

            // Higher ManualSpeed value = slower; ← (d=-1) makes faster, → (d=+1) makes slower
            case ROW_SPEED when _cfg.ManualMode:
                _cfg.ManualSpeed = Math.Clamp(_cfg.ManualSpeed + d, 1, 4);
                break;

            case ROW_GAP when _cfg.ManualMode:
                _cfg.ManualGap = Math.Clamp(_cfg.ManualGap + d, GameState.MinGapSize, GameState.BaseGapSize + 3);
                break;
        }
    }

    // ── Rendering ─────────────────────────────────────────────────────────────

    private static void Draw()
    {
        Console.Clear();
        Console.SetCursorPosition(0, 0);

        var panel = GameSettings.Instance.CreatePanel(78);
        int iw    = panel.InnerWidth; // 76

        // Row 0: top border
        Console.ForegroundColor = panel.BorderColor;
        Console.WriteLine(panel.BuildTop());

        // Row 1: title
        string title = "CUSTOM GAME";
        string centred = title.PadLeft((iw + title.Length) / 2).PadRight(iw);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(panel.BuildRow(centred));

        // Row 2: separator
        Console.ForegroundColor = panel.BorderColor;
        Console.WriteLine(panel.BuildSep());

        // Row 3: empty
        Console.WriteLine(panel.BuildEmptyRow());

        // Row 4: MODE section header
        WriteSection(panel, "MODE");

        // Row 5: Mode row
        string modeVal  = _cfg.ManualMode ? "By Manual" : "By Level";
        string modeNote = _cfg.ManualMode
            ? "Speed & Gap set manually, fixed difficulty"
            : "Speed & Gap auto-derived, difficulty progresses";
        WriteParamRow(panel, ROW_MODE, "Mode", modeVal, locked: false, note: modeNote, isMode: true);

        // Row 6: empty
        Console.WriteLine(panel.BuildEmptyRow());

        // Row 7: PARAMETERS section header
        WriteSection(panel, "PARAMETERS");

        // Row 8: Level
        if (_cfg.ManualMode)
            WriteParamRow(panel, ROW_LEVEL, "Level", "[–]", locked: true,
                note: "N/A in By Manual mode");
        else
            WriteParamRow(panel, ROW_LEVEL, "Level", _cfg.StartLevel.ToString(), locked: false,
                note: $"1–10  ·  auto → Speed {CustomGameConfig.LevelToSpeed(_cfg.StartLevel)}" +
                      $" ({CustomGameConfig.SpeedName(CustomGameConfig.LevelToSpeed(_cfg.StartLevel))})" +
                      $", Gap {CustomGameConfig.LevelToGap(_cfg.StartLevel)} rows");

        // Row 9: Speed
        int autoSpd = CustomGameConfig.LevelToSpeed(_cfg.StartLevel);
        if (!_cfg.ManualMode)
            WriteParamRow(panel, ROW_SPEED, "Speed", autoSpd.ToString(), locked: true,
                note: $"({CustomGameConfig.SpeedName(autoSpd)})  set by Level");
        else
            WriteParamRow(panel, ROW_SPEED, "Speed", _cfg.ManualSpeed.ToString(), locked: false,
                note: $"1=Fastest · 2=Fast · 3=Normal · 4=Slow  " +
                      $"(current: {CustomGameConfig.SpeedName(_cfg.ManualSpeed)})");

        // Row 10: Gap
        int autoGap = CustomGameConfig.LevelToGap(_cfg.StartLevel);
        if (!_cfg.ManualMode)
            WriteParamRow(panel, ROW_GAP, "Gap", autoGap.ToString(), locked: true,
                note: $"({autoGap} rows)  set by Level");
        else
            WriteParamRow(panel, ROW_GAP, "Gap", _cfg.ManualGap.ToString(), locked: false,
                note: $"{_cfg.ManualGap} rows  (5=narrow · 10=wide)");

        // Row 11: empty
        Console.WriteLine(panel.BuildEmptyRow());

        // Row 12: SUMMARY section header
        WriteSection(panel, "SUMMARY");

        // Row 13: summary content
        string summary = BuildSummary();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(panel.BuildRow($"   {summary}"));

        // Rows 14-20: empty padding (7 rows)
        for (int i = 0; i < 7; i++) Console.WriteLine(panel.BuildEmptyRow());

        // Row 21: separator
        Console.ForegroundColor = panel.BorderColor;
        Console.WriteLine(panel.BuildSep());

        // Row 22: controls hint
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(panel.BuildRow("  ↑↓: Select   ←→: Adjust   Enter: Start Game   ESC: Back"));

        // Row 23: bottom border — Write (not WriteLine) to avoid scroll on last row
        Console.ForegroundColor = panel.BorderColor;
        Console.Write(panel.BuildBottom());
        Console.ResetColor();
    }

    // ── Row writers ───────────────────────────────────────────────────────────

    private static void WriteSection(UIPanel panel, string title)
    {
        Console.ForegroundColor = panel.BorderColor;
        Console.Write(panel.Borders.Vert);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"  {title}".PadRight(panel.InnerWidth));
        Console.ForegroundColor = panel.BorderColor;
        Console.WriteLine(panel.Borders.Vert);
        Console.ResetColor();
    }

    /// <param name="isMode">When true, always show ◄ ► (mode is never truly locked).</param>
    private static void WriteParamRow(UIPanel panel, int rowIdx, string label,
        string rawValue, bool locked, string note, bool isMode = false)
    {
        bool sel = !locked && rowIdx == _row;

        string indicator = sel ? "► " : "  ";

        // Value display: show arrows when editable, bare value when locked
        string valDisplay;
        if (isMode)
            valDisplay = $"◄ {rawValue,-12} ►"; // always arrows for Mode
        else if (locked)
            valDisplay = rawValue.PadRight(16);
        else
            valDisplay = $"◄ {rawValue,-3} ►".PadRight(16);

        // Assemble: 2+2+9+valDisplay+2+note — truncate note if needed
        string left  = $"  {indicator}{label,-9}{valDisplay}  ";
        int    avail = panel.InnerWidth - left.Length;
        string noteStr = avail > 0 && note.Length > 0
            ? (note.Length > avail ? note[..avail] : note)
            : "";
        string content = (left + noteStr).PadRight(panel.InnerWidth);

        ConsoleColor fg = locked   ? ConsoleColor.DarkGray
                        : sel      ? ConsoleColor.Yellow
                        :            ConsoleColor.White;

        Console.ForegroundColor = panel.BorderColor;
        Console.Write(panel.Borders.Vert);
        Console.ForegroundColor = fg;
        Console.Write(content);
        Console.ForegroundColor = panel.BorderColor;
        Console.WriteLine(panel.Borders.Vert);
        Console.ResetColor();
    }

    // ── Summary line ──────────────────────────────────────────────────────────

    private static string BuildSummary()
    {
        int spd = _cfg.EffectiveSpeed;
        int gap = _cfg.EffectiveGap;

        if (_cfg.ManualMode)
            return $"Manual · Speed: {spd} ({CustomGameConfig.SpeedName(spd)}) · Gap: {gap} rows · Fixed difficulty";

        int lvl = _cfg.StartLevel;
        return $"Level {lvl} · Speed: {spd} ({CustomGameConfig.SpeedName(spd)}) · Gap: {gap} rows · {CustomGameConfig.DiffName(lvl)}";
    }
}
