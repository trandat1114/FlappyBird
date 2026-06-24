using FlappyBird.Audio;
using FlappyBird.Audio.Enum;
using FlappyBird.Settings;

namespace FlappyBird.UI;

/// <summary>
/// Terminal UI for adjusting per-sound start offsets (leading-silence trim).
///
/// Layout (66-char UIPanel):
///   Title / Sound selector / Offset value + bar / Metronome + beat bar / Help
///
/// Input is polled non-blocking every 80 ms so the beat-progress bar animates
/// smoothly without blocking on Console.ReadKey().
///
/// Working config:
///   All edits go to a working copy. ESC reverts; S commits to AudioManager and disk.
/// </summary>
public static class AudioCalibrationMenu
{
    private const int BAR_WIDTH = 44;

    public static void Show()
    {
        Console.CursorVisible = false;

        // Snapshot original values so ESC can revert
        var original = AudioManager.AudioOffset.Clone();
        // Working copy: engine writes to this; AudioManager.AudioOffset is updated on Save
        var working  = AudioManager.AudioOffset.Clone();

        using var engine = new AudioCalibrationEngine(working);

        bool saved      = false;
        bool beatDirty  = false;           // beat fired → redraw only beat bar row
        engine.BeatFired += () => beatDirty = true;

        Console.Clear();
        DrawFull(engine);

        while (true)
        {
            // Refresh beat-bar row on metronome tick, or full refresh if needed
            if (beatDirty)
            {
                beatDirty = false;
                RefreshBeatRow(engine);
            }
            else if (engine.MetronomeRunning)
            {
                RefreshBeatRow(engine);
            }

            if (Console.KeyAvailable)
            {
                var key   = Console.ReadKey(true);
                bool shift = (key.Modifiers & ConsoleModifiers.Shift) != 0;

                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        // Revert live config to original
                        AudioManager.AudioOffset.JumpStartOffsetMs  = original.JumpStartOffsetMs;
                        AudioManager.AudioOffset.ScoreStartOffsetMs = original.ScoreStartOffsetMs;
                        goto exit;

                    case ConsoleKey.S:
                        // Commit working copy → live config → file
                        AudioManager.AudioOffset.JumpStartOffsetMs  = working.JumpStartOffsetMs;
                        AudioManager.AudioOffset.ScoreStartOffsetMs = working.ScoreStartOffsetMs;
                        AudioOffsetSerializer.Save(AudioManager.AudioOffset);
                        saved = true;
                        Console.Clear();
                        DrawFull(engine, savedMsg: true);
                        Thread.Sleep(900);
                        goto exit;

                    case ConsoleKey.LeftArrow:
                        engine.AdjustOffset(shift ? -1 : -10);
                        Console.Clear();
                        DrawFull(engine);
                        break;

                    case ConsoleKey.RightArrow:
                        engine.AdjustOffset(shift ? 1 : 10);
                        Console.Clear();
                        DrawFull(engine);
                        break;

                    case ConsoleKey.Tab:
                        engine.SwitchSound();
                        Console.Clear();
                        DrawFull(engine);
                        break;

                    case ConsoleKey.Spacebar:
                        engine.PlayOnce();
                        break;

                    case ConsoleKey.M:
                        engine.ToggleMetronome();
                        Console.Clear();
                        DrawFull(engine);
                        break;

                    case ConsoleKey.OemPlus:
                    case ConsoleKey.Add:
                        engine.AdjustBpm(5);
                        Console.Clear();
                        DrawFull(engine);
                        break;

                    case ConsoleKey.OemMinus:
                    case ConsoleKey.Subtract:
                        engine.AdjustBpm(-5);
                        Console.Clear();
                        DrawFull(engine);
                        break;

                    case ConsoleKey.R:
                        working.Reset();
                        Console.Clear();
                        DrawFull(engine);
                        break;
                }
            }

            Thread.Sleep(80); // ~12 FPS — smooth beat bar without burning CPU
        }

        exit:
        _ = saved; // suppress unused-variable warning
        Console.CursorVisible = false;
    }

    // ── Full draw ──────────────────────────────────────────────────────────────

    private static void DrawFull(AudioCalibrationEngine engine, bool savedMsg = false)
    {
        var s     = GameSettings.Instance;
        var panel = s.CreatePanel(66);

        Console.SetCursorPosition(0, 0);

        panel.PrintTop();
        panel.PrintTitle("AUDIO OFFSET CALIBRATION");
        panel.PrintSep();

        // ── Sound selector ────────────────────────────────────────────────────
        string jumpMark  = engine.ActiveSound == SoundEffect.Jump  ? "►" : " ";
        string scoreMark = engine.ActiveSound == SoundEffect.Score ? "►" : " ";
        panel.PrintRow(
            $"  Sound  :  {jumpMark} Jump (flap.mp3)      {scoreMark} Score (point.mp3)",
            ConsoleColor.White);

        panel.PrintSep();

        // ── Offset value ──────────────────────────────────────────────────────
        int    offset     = engine.CurrentOffset;
        string offsetStr  = $"  Start Offset  :  ◄  {offset,4} ms  ►";
        panel.PrintRow(offsetStr, ConsoleColor.Yellow);

        // Offset bar: filled = offset / MaxOffsetMs * BAR_WIDTH
        int filled = (int)((double)offset / AudioOffsetConfig.MaxOffsetMs * BAR_WIDTH);
        filled = Math.Clamp(filled, 0, BAR_WIDTH);
        string offsetBar =
            $"  [{new string('█', filled)}{new string('░', BAR_WIDTH - filled)}]  0 – {AudioOffsetConfig.MaxOffsetMs} ms";
        panel.PrintRow(offsetBar, ConsoleColor.Cyan);

        panel.PrintRow("  Trims N ms of leading silence from the start of the file.", ConsoleColor.DarkGray);

        panel.PrintSep();

        // ── Metronome ──────────────────────────────────────────────────────────
        string metState = engine.MetronomeRunning ? "ON " : "OFF";
        ConsoleColor metColor = engine.MetronomeRunning ? ConsoleColor.Green : ConsoleColor.Gray;
        panel.PrintRow(
            $"  Metronome  :  [{metState}]   BPM {engine.MetronomeBpm,3}",
            metColor);

        panel.PrintRow("", ConsoleColor.White);

        // Beat bar — dynamic, refreshed by RefreshBeatRow
        DrawBeatBarRow(engine, panel);

        panel.PrintRow("", ConsoleColor.White);
        panel.PrintSep();

        // ── Help ───────────────────────────────────────────────────────────────
        panel.PrintRow("  ←→: ±10ms   Shift+←→: ±1ms   Tab: Switch sound", ConsoleColor.Gray);
        panel.PrintRow("  Space: Play once   M: Metronome   +/-: BPM ±5   R: Reset", ConsoleColor.Gray);

        if (savedMsg)
            panel.PrintRow("  Saved!", ConsoleColor.Green);
        else
            panel.PrintRow("  S: Save & Apply   ESC: Cancel (revert)", ConsoleColor.Gray);

        panel.PrintBottom();
    }

    // ── Beat bar refresh (called frequently to animate the progress bar) ───────

    // Row index of the beat bar line within the full panel output.
    // Top(1) + Title(1) + Sep(1) + Sound(1) + Sep(1) + Offset(1) + Bar(1) + Desc(1) + Sep(1)
    // + Metro(1) + Blank(1) = 11 rows before the beat bar → cursor row = 11
    private const int BEAT_ROW_Y = 11;

    private static void RefreshBeatRow(AudioCalibrationEngine engine)
    {
        var s     = GameSettings.Instance;
        var panel = s.CreatePanel(66);
        Console.SetCursorPosition(0, BEAT_ROW_Y);
        DrawBeatBarRow(engine, panel);
    }

    private static void DrawBeatBarRow(AudioCalibrationEngine engine, UIPanel panel)
    {
        if (!engine.MetronomeRunning)
        {
            panel.PrintRow("  Beat  :  [" + new string('░', BAR_WIDTH) + "]  ·", ConsoleColor.DarkGray);
            return;
        }

        int filled = (int)(engine.BeatProgress * BAR_WIDTH);
        filled = Math.Clamp(filled, 0, BAR_WIDTH);
        string note  = engine.BeatFlash ? "♩" : "♪";
        string bar   = $"  Beat  :  [{new string('█', filled)}{new string('░', BAR_WIDTH - filled)}]  {note}";
        ConsoleColor color = engine.BeatFlash ? ConsoleColor.Cyan : ConsoleColor.Yellow;
        panel.PrintRow(bar, color);
    }

    // ── Offset bar helper ─────────────────────────────────────────────────────

    private static string OffsetBar(int offsetMs)
    {
        int filled = (int)((double)offsetMs / AudioOffsetConfig.MaxOffsetMs * BAR_WIDTH);
        filled = Math.Clamp(filled, 0, BAR_WIDTH);
        return new string('█', filled) + new string('░', BAR_WIDTH - filled);
    }
}
