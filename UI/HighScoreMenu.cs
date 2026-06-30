using FlappyBird.Models;
using FlappyBird.Settings;
using FlappyBird.Utils;

namespace FlappyBird.UI;

/// <summary>
/// High Scores viewer.
/// Shows the last 1000 play sessions, sorted by score descending, paginated (10 per page).
/// Controls: ← → to navigate pages, Tab to cycle filter (All / SP / CG), ESC to exit.
/// </summary>
public static class HighScoreMenu
{
    private const int PageSize = 10;

    private enum ScoreFilter { All, SinglePlayer, CustomGame }

    private static ScoreFilter _filter = ScoreFilter.All;
    private static int         _page   = 0;

    // ── Public entry point ────────────────────────────────────────────────────

    public static void Show()
    {
        Console.CursorVisible = false;
        _filter = ScoreFilter.All;
        _page   = 0;
        Draw();

        while (true)
        {
            int totalPages = CalcTotalPages(GetFiltered());
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.LeftArrow:
                    if (_page > 0) { _page--; Draw(); }
                    break;

                case ConsoleKey.RightArrow:
                    if (_page < totalPages - 1) { _page++; Draw(); }
                    break;

                case ConsoleKey.Tab:
                    _filter = (ScoreFilter)(((int)_filter + 1) % 3);
                    _page   = 0;
                    Draw();
                    break;

                case ConsoleKey.Escape:
                    return;
            }
        }
    }

    // ── Data ─────────────────────────────────────────────────────────────────

    private static List<ScoreEntry> GetFiltered()
    {
        var all = ScoreRepository.GetAll();
        IEnumerable<ScoreEntry> q = _filter switch
        {
            ScoreFilter.SinglePlayer => all.Where(e => e.Mode == "SinglePlayer"),
            ScoreFilter.CustomGame   => all.Where(e => e.Mode == "CustomGame"),
            _                        => all,
        };
        return q.OrderByDescending(e => e.Score)
                .ThenByDescending(e => e.PlayedAt)
                .ToList();
    }

    private static int CalcTotalPages(List<ScoreEntry> list) =>
        Math.Max(1, (list.Count + PageSize - 1) / PageSize);

    // ── Rendering ─────────────────────────────────────────────────────────────

    private static void Draw()
    {
        Console.Clear();
        var panel = GameSettings.Instance.CreatePanel(78);
        int iw    = panel.InnerWidth; // 76

        var filtered   = GetFiltered();
        int totalPages = CalcTotalPages(filtered);
        _page          = Math.Clamp(_page, 0, totalPages - 1);
        var page       = filtered.Skip(_page * PageSize).Take(PageSize).ToList();

        // Row 0: top border
        Console.ForegroundColor = panel.BorderColor;
        Console.WriteLine(panel.BuildTop());

        // Row 1: title
        const string TITLE = "HIGH SCORES";
        string centredTitle = TITLE.PadLeft((iw + TITLE.Length) / 2).PadRight(iw);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(panel.BuildRow(centredTitle));

        // Row 2: separator
        Console.ForegroundColor = panel.BorderColor;
        Console.WriteLine(panel.BuildSep());

        // Row 3: filter tabs
        WriteFilterRow(panel, iw);

        // Row 4: separator
        Console.ForegroundColor = panel.BorderColor;
        Console.WriteLine(panel.BuildSep());

        // Row 5: column header
        WriteColumnHeader(panel, iw);

        // Row 6: thin divider
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(panel.BuildRow(new string('─', iw)));

        // Rows 7-16: page entries (always exactly PageSize rows)
        for (int i = 0; i < PageSize; i++)
        {
            if (i < page.Count)
                WriteEntry(panel, iw, _page * PageSize + i + 1, page[i]);
            else
            {
                Console.ForegroundColor = panel.BorderColor;
                Console.WriteLine(panel.BuildEmptyRow());
            }
        }

        // Row 17: separator
        Console.ForegroundColor = panel.BorderColor;
        Console.WriteLine(panel.BuildSep());

        // Row 18: page info + navigation hint
        WriteStatusRow(panel, iw, filtered.Count, totalPages);

        // Rows 19-21: empty padding
        Console.ForegroundColor = panel.BorderColor;
        for (int i = 0; i < 3; i++)
            Console.WriteLine(panel.BuildEmptyRow());

        // Row 22: controls hint
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(panel.BuildRow("  ← →: Page   Tab: Filter   ESC: Back"));

        // Row 23: bottom border — Console.Write (not WriteLine) to avoid scroll
        Console.ForegroundColor = panel.BorderColor;
        Console.Write(panel.BuildBottom());
        Console.ResetColor();
    }

    // ── Row writers ───────────────────────────────────────────────────────────

    private static void WriteFilterRow(UIPanel panel, int iw)
    {
        string allLbl = _filter == ScoreFilter.All          ? "[ALL]"            : " ALL ";
        string spLbl  = _filter == ScoreFilter.SinglePlayer ? "[SINGLE PLAYER]"  : " SINGLE PLAYER ";
        string cgLbl  = _filter == ScoreFilter.CustomGame   ? "[CUSTOM GAME]"    : " CUSTOM GAME ";

        // Widths: ALL=5, SINGLE PLAYER=15, CUSTOM GAME=13 (with brackets same length as spaces)
        int labelsWidth = 2 + allLbl.Length + 2 + spLbl.Length + 2 + cgLbl.Length; // "  " + each + "  " sep
        string hint     = "Tab: switch filter";
        int padLen      = iw - labelsWidth - hint.Length;

        Console.ForegroundColor = panel.BorderColor;
        Console.Write(panel.Borders.Vert);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("  ");
        WriteFilterLabel(allLbl, _filter == ScoreFilter.All);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("  ");
        WriteFilterLabel(spLbl, _filter == ScoreFilter.SinglePlayer);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("  ");
        WriteFilterLabel(cgLbl, _filter == ScoreFilter.CustomGame);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        if (padLen > 0) Console.Write(new string(' ', padLen));
        Console.Write(hint);

        Console.ForegroundColor = panel.BorderColor;
        Console.WriteLine(panel.Borders.Vert);
        Console.ResetColor();
    }

    private static void WriteFilterLabel(string label, bool active)
    {
        if (active)
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.Write(label);
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(label);
        }
    }

    private static void WriteColumnHeader(UIPanel panel, int iw)
    {
        // Aligns with data row format: {rank,4}. {score,6}  {dt,-16}  {mode,-2}  {cfg}
        string header = $" {"No.",-4} {"Score",6}  {"Date/Time",-16}  {"Md",-2}  Config";
        string padded = header.PadRight(iw);

        Console.ForegroundColor = panel.BorderColor;
        Console.Write(panel.Borders.Vert);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(padded);
        Console.ForegroundColor = panel.BorderColor;
        Console.WriteLine(panel.Borders.Vert);
        Console.ResetColor();
    }

    private static void WriteEntry(UIPanel panel, int iw, int rank, ScoreEntry entry)
    {
        string dt   = entry.PlayedAt.ToString("MM-dd HH:mm      ")[..16]; // "06-30 14:32      " → 16 chars
        string mode = entry.Mode == "CustomGame" ? "CG" : "SP";
        string cfg  = BuildConfigStr(entry);

        string line    = $"{rank,4}. {entry.Score,6}  {dt,-16}  {mode,-2}  {cfg}";
        string content = line.Length < iw ? line.PadRight(iw) : line[..iw];

        ConsoleColor fg = entry.Mode == "CustomGame" ? ConsoleColor.Cyan : ConsoleColor.White;

        Console.ForegroundColor = panel.BorderColor;
        Console.Write(panel.Borders.Vert);
        Console.ForegroundColor = fg;
        Console.Write(content);
        Console.ForegroundColor = panel.BorderColor;
        Console.WriteLine(panel.Borders.Vert);
        Console.ResetColor();
    }

    private static void WriteStatusRow(UIPanel panel, int iw, int total, int totalPages)
    {
        string info;
        if (total == 0)
            info = "  No records yet — play a game to see your scores here.";
        else
        {
            string pageStr = totalPages > 1 ? $"  Page {_page + 1} / {totalPages}" : $"  Page 1 / 1";
            string totStr  = $"   Total: {total} record{(total == 1 ? "" : "s")}";
            string navStr  = totalPages > 1 ? "   ◄ ►: navigate" : "";
            info = pageStr + totStr + navStr;
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(panel.BuildRow(info.PadRight(iw)));
        Console.ResetColor();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string BuildConfigStr(ScoreEntry e)
    {
        if (e.Mode != "CustomGame") return "-";
        if (e.ManualMode == true)
            return $"Manual · S{e.ManualSpeed}/G{e.ManualGap}";
        if (e.StartLevel.HasValue)
            return $"Level {e.StartLevel}";
        return "-";
    }
}
