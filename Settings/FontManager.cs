using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using FlappyBird.Utils;

namespace FlappyBird.Settings;

/// <summary>
/// Scans the Resources/Fonts directory for valid TTF/OTF fonts and applies
/// them to the console via Win32 API (Windows only; graceful no-op elsewhere).
/// Each call to <see cref="Apply"/> registers the font with <c>AddFontResourceEx</c>
/// so it is visible to conhost.exe even without a system-level install.
/// </summary>
public static class FontManager
{
    public record FontEntry(string DisplayName, string FilePath, string FaceName);

    // ── Font list ─────────────────────────────────────────────────────────────

    public static FontEntry[] Available { get; private set; } = [];

    /// <summary>
    /// True when the process is hosted inside Windows Terminal.
    /// SetCurrentConsoleFontEx has no visible effect in WT — it only works in conhost.exe.
    /// </summary>
    public static bool IsWindowsTerminal =>
        Environment.GetEnvironmentVariable("WT_SESSION") is not null;

    /// <summary>
    /// Scans <paramref name="fontsDir"/> (one sub-folder per font family).
    /// Skips files that are not TTF/OTF, exceed 30 MB, or have unreadable name tables.
    /// Saves the current console font so RestoreDefault() can revert it later.
    /// </summary>
    public static void Scan(string fontsDir)
    {
        if (OperatingSystem.IsWindows() && !_originalSaved) SaveOriginalFont();

        if (!Directory.Exists(fontsDir)) { Available = []; return; }

        var list = new List<FontEntry>();
        foreach (var dir in Directory.GetDirectories(fontsDir).OrderBy(d => d))
        {
            var entry = ScanFolder(dir);
            if (entry is not null) list.Add(entry);
        }
        Available = [.. list];
    }

    // ── Apply / restore ───────────────────────────────────────────────────────

    /// <summary>Applies a discovered font entry. Returns false on failure.</summary>
    public static bool Apply(FontEntry entry, int fontSize)
    {
        if (!OperatingSystem.IsWindows()) return false;
        return ApplyWindows(entry.FaceName, (short)fontSize, entry.FilePath);
    }

    /// <summary>Applies a font by face name only (e.g. from saved settings).</summary>
    public static bool Apply(string faceName, int fontSize)
    {
        if (!OperatingSystem.IsWindows()) return false;
        // Try to find a matching file path so we can register it if needed
        var entry = Array.Find(Available, e => e.FaceName == faceName);
        return ApplyWindows(faceName, (short)fontSize, entry?.FilePath);
    }

    /// <summary>Restores the console font saved at startup.</summary>
    public static bool RestoreDefault()
    {
        if (!OperatingSystem.IsWindows() || !_originalSaved) return false;
        return RestoreOriginalWindows();
    }

    /// <summary>Unregisters all fonts loaded by this session.</summary>
    public static void Cleanup()
    {
        if (OperatingSystem.IsWindows()) CleanupWindows();
    }

    // ── Folder scanning ───────────────────────────────────────────────────────

    private static readonly string[] FontExts = [".ttf", ".otf"];
    private const long MaxFontBytes = 30L * 1024 * 1024;

    private static FontEntry? ScanFolder(string dir)
    {
        var files = Directory.EnumerateFiles(dir)
            .Where(f => FontExts.Contains(Path.GetExtension(f).ToLowerInvariant())
                     && new FileInfo(f).Length <= MaxFontBytes)
            .OrderBy(PreferenceRank)      // best variant first
            .ToArray();

        foreach (var file in files)
        {
            string? face = TtfReader.ReadFamilyName(file);
            if (face is null) continue;

            string display = Path.GetFileName(dir).Replace('_', ' ').Replace('-', ' ').Trim();
            return new FontEntry(display, file, face);
        }
        return null;
    }

    // Lower number = higher preference for console use
    private static int PreferenceRank(string path)
    {
        string name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
        if (name.Contains("italic"))  return 90;
        if (name.Contains("mono")  && name.Contains("regular")) return 0;
        if (name.Contains("regular") && !name.Contains("bold"))  return 1;
        if (name.Contains("mono"))    return 2;
        if (name.Contains("bold"))    return 50;
        return 10;
    }

    // ── Win32 P/Invoke ────────────────────────────────────────────────────────

    [StructLayout(LayoutKind.Sequential)]
    private struct COORD { public short X; public short Y; }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CONSOLE_FONT_INFOEX
    {
        public uint  cbSize;
        public uint  nFont;
        public COORD dwFontSize;
        public int   FontFamily;
        public int   FontWeight;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string FaceName;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int h);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetCurrentConsoleFontEx(
        IntPtr h, bool bMax, ref CONSOLE_FONT_INFOEX fi);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetCurrentConsoleFontEx(
        IntPtr h, bool bMax, ref CONSOLE_FONT_INFOEX fi);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int AddFontResourceEx(string path, uint fl, IntPtr pdv);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool RemoveFontResourceEx(string path, uint fl, IntPtr pdv);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wp, IntPtr lp);

    private const int  STD_OUTPUT  = -11;
    private const uint FR_NOT_ENUM = 0x20; // register globally but hide from font pickers
    private const uint WM_FONTCHANGE = 0x001D;

    private static CONSOLE_FONT_INFOEX _original;
    private static bool _originalSaved;
    private static readonly List<string> _loaded = [];

    [SupportedOSPlatform("windows")]
    private static void SaveOriginalFont()
    {
        var h    = GetStdHandle(STD_OUTPUT);
        var font = new CONSOLE_FONT_INFOEX { cbSize = (uint)Marshal.SizeOf<CONSOLE_FONT_INFOEX>() };
        if (GetCurrentConsoleFontEx(h, false, ref font))
        {
            _original      = font;
            _originalSaved = true;
        }
    }

    [SupportedOSPlatform("windows")]
    private static bool ApplyWindows(string faceName, short size, string? filePath)
    {
        // Register the font file so conhost.exe can see it (skip if already done)
        if (filePath is not null && !_loaded.Contains(filePath))
        {
            if (AddFontResourceEx(filePath, FR_NOT_ENUM, IntPtr.Zero) > 0)
            {
                _loaded.Add(filePath);
                SendMessage((IntPtr)0xFFFF, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
            }
        }

        var h    = GetStdHandle(STD_OUTPUT);
        var font = new CONSOLE_FONT_INFOEX
        {
            cbSize     = (uint)Marshal.SizeOf<CONSOLE_FONT_INFOEX>(),
            nFont      = 0,
            dwFontSize = new COORD { X = 0, Y = size },
            FontFamily = 54,  // FIXED_PITCH | FF_MODERN | TMPF_TRUETYPE
            FontWeight = 400, // FW_NORMAL
            FaceName   = faceName,
        };
        return SetCurrentConsoleFontEx(h, false, ref font);
    }

    [SupportedOSPlatform("windows")]
    private static bool RestoreOriginalWindows()
    {
        if (_original.cbSize == 0) return false;
        var h    = GetStdHandle(STD_OUTPUT);
        var font = _original;
        return SetCurrentConsoleFontEx(h, false, ref font);
    }

    [SupportedOSPlatform("windows")]
    private static void CleanupWindows()
    {
        foreach (var p in _loaded)
            RemoveFontResourceEx(p, FR_NOT_ENUM, IntPtr.Zero);
        if (_loaded.Count > 0)
            SendMessage((IntPtr)0xFFFF, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
        _loaded.Clear();
    }
}
