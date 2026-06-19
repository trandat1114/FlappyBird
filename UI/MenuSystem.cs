using FlappyBird.Enum;
using FlappyBird.Localization;
using FlappyBird.Settings;
using FlappyBird.UI.Border;

namespace FlappyBird.UI
{
    public static class SimpleMenuSystem
    {
        private static int  _selected      = 0;
        private static int  _prevSelected  = -1;
        private static bool _isFirstRender = true;
        private static bool _forceRedraw   = false;

        private static string[] menuItems => [
            L.Get(L.MENU_SECTION_HUMAN),    // 0  header
            L.Get(L.MENU_SINGLE_PLAYER),    // 1
            L.Get(L.MENU_TWO_PLAYER),       // 2
            "",                              // 3  spacer
            L.Get(L.MENU_SECTION_AI),       // 4  header
            "       Dual AI Comparison",     // 5
            "       Split Screen Real-time", // 6
            "       AI Tournament",          // 7
            "",                              // 8  spacer
            L.Get(L.MENU_SETTINGS),         // 9
            "",                              // 10 spacer
            L.Get(L.MENU_QUIT),             // 11
        ];

        private static readonly MenuAction[] menuActions = [
            MenuAction.None,
            MenuAction.SinglePlayer,
            MenuAction.TwoPlayer,
            MenuAction.None,
            MenuAction.None,
            MenuAction.DualAI,
            MenuAction.SplitScreenAI,
            MenuAction.AITournament,
            MenuAction.None,
            MenuAction.Settings,
            MenuAction.None,
            MenuAction.Exit,
        ];

        private static readonly bool[] selectable = [
            false, true, true, false, false, true, true, true, false, true, false, true
        ];

        // ── Public entry point ────────────────────────────────────────────────

        public static MenuAction ShowMenu()
        {
            _selected = GetFirstSelectable();

            if (_isFirstRender || _forceRedraw)
            {
                DrawFull();
                _isFirstRender = false;
                _forceRedraw   = false;
            }

            while (true)
            {
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        _prevSelected = _selected;
                        MovePrev();
                        if (_prevSelected != _selected) UpdateSelection();
                        break;

                    case ConsoleKey.DownArrow:
                        _prevSelected = _selected;
                        MoveNext();
                        if (_prevSelected != _selected) UpdateSelection();
                        break;

                    case ConsoleKey.Enter:
                        if (selectable[_selected])
                        {
                            _forceRedraw = true;
                            return menuActions[_selected];
                        }
                        break;

                    case ConsoleKey.Escape:
                        return MenuAction.Exit;
                }
            }
        }

        // ── Full redraw ───────────────────────────────────────────────────────

        private static void DrawFull()
        {
            var items = menuItems;
            var panel = GameSettings.Instance.CreatePanel(66);

            Console.Clear();
            Console.CursorVisible = false;

            // Logo box (rows 0-16)
            Console.ForegroundColor = panel.BorderColor;
            Console.WriteLine(panel.BuildTop());
            Console.WriteLine(panel.BuildEmptyRow());
            Console.WriteLine(panel.BuildRow("       ███████╗██╗      █████╗ ██████╗ ██████╗ ██╗   ██╗        "));
            Console.WriteLine(panel.BuildRow("       ██╔════╝██║     ██╔══██╗██╔══██╗██╔══██╗╚██╗ ██╔╝        "));
            Console.WriteLine(panel.BuildRow("       █████╗  ██║     ███████║██████╔╝██████╔╝ ╚████╔╝         "));
            Console.WriteLine(panel.BuildRow("       ██╔══╝  ██║     ██╔══██║██╔═══╝ ██╔═══╝   ╚██╔╝          "));
            Console.WriteLine(panel.BuildRow("       ██║     ███████╗██║  ██║██║     ██║        ██║           "));
            Console.WriteLine(panel.BuildRow("       ╚═╝     ╚══════╝╚═╝  ╚═╝╚═╝     ╚═╝        ╚═╝           "));
            Console.WriteLine(panel.BuildEmptyRow());
            Console.WriteLine(panel.BuildRow("                  ██████╗ ██╗██████╗ ██████╗                    "));
            Console.WriteLine(panel.BuildRow("                  ██╔══██╗██║██╔══██╗██╔══██╗                   "));
            Console.WriteLine(panel.BuildRow("                  ██████╔╝██║██████╔╝██║  ██║                   "));
            Console.WriteLine(panel.BuildRow("                  ██╔══██╗██║██╔══██╗██║  ██║                   "));
            Console.WriteLine(panel.BuildRow("                  ██████╔╝██║██║  ██║██████╔╝                   "));
            Console.WriteLine(panel.BuildRow("                  ╚═════╝ ╚═╝╚═╝  ╚═╝╚═════╝                    "));
            Console.WriteLine(panel.BuildEmptyRow());
            Console.WriteLine(panel.BuildBottom());
            Console.ResetColor();

            // Info description (row 17=blank, row 18=INFO)
            Console.WriteLine();
            Console.ForegroundColor = panel.BorderColor;
            Console.WriteLine("INFO: " + GetDescription(_selected));
            Console.ResetColor();
            Console.WriteLine();

            // Menu box (╔ at row 20, title row 21, ╠ row 22, items rows 23+)
            string centeredTitle = L.Get(L.MENU_TITLE)
                .PadLeft((64 + L.Get(L.MENU_TITLE).Length) / 2).PadRight(64);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(panel.BuildTop());
            Console.WriteLine(panel.BuildRow(centeredTitle));
            Console.WriteLine(panel.BuildSep());
            for (int i = 0; i < items.Length; i++)
            {
                PrintItemRow(panel, items, i);
                Console.WriteLine();
            }
            Console.WriteLine(panel.BuildBottom());
            Console.ResetColor();

            // Controls box (always Single border, always Gray)
            var ctrl = new UIPanel(66, BorderStyle.Single)
            {
                BorderColor  = ConsoleColor.Gray,
                ContentColor = ConsoleColor.Gray,
            };
            Console.WriteLine();
            Console.ForegroundColor = ctrl.BorderColor;
            Console.WriteLine(ctrl.BuildTop());
            Console.WriteLine(ctrl.BuildRow(L.Get(L.MENU_CTRL_LABEL)));
            Console.WriteLine(ctrl.BuildRow(L.Get(L.MENU_CTRL_MOVE)));
            Console.WriteLine(ctrl.BuildRow(L.Get(L.MENU_CTRL_SELECT)));
            Console.WriteLine(ctrl.BuildRow(L.Get(L.MENU_CTRL_EXIT)));
            Console.WriteLine(ctrl.BuildBottom());
            Console.ResetColor();
        }

        // ── In-place selection update ─────────────────────────────────────────

        private static void UpdateSelection()
        {
            var items = menuItems;
            var panel = GameSettings.Instance.CreatePanel(66);

            // Update description (row 18, col 6 = after "INFO: ")
            Console.SetCursorPosition(6, 18);
            Console.ForegroundColor = panel.BorderColor;
            Console.Write(GetDescription(_selected).PadRight(58));
            Console.ResetColor();

            // Redraw previous and new item rows
            if (_prevSelected >= 0 && selectable[_prevSelected])
            {
                Console.SetCursorPosition(0, MenuRow(_prevSelected));
                PrintItemRow(panel, items, _prevSelected);
            }
            if (selectable[_selected])
            {
                Console.SetCursorPosition(0, MenuRow(_selected));
                PrintItemRow(panel, items, _selected);
            }
        }

        // ── Row printer (writes without trailing newline) ─────────────────────

        private static void PrintItemRow(UIPanel panel, string[] items, int i)
        {
            if (string.IsNullOrEmpty(items[i]))
            {
                Console.ForegroundColor = panel.BorderColor;
                Console.Write(panel.BuildEmptyRow());
                Console.ResetColor();
                return;
            }

            bool isSelected = i == _selected && selectable[i];

            Console.ForegroundColor = panel.BorderColor;
            Console.Write(panel.Borders.Vert);

            if (isSelected)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.Write(("  ► " + items[i]).PadRight(panel.InnerWidth));
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ItemColor(i);
                Console.Write(("    " + items[i]).PadRight(panel.InnerWidth));
                Console.ResetColor();
            }

            Console.ForegroundColor = panel.BorderColor;
            Console.Write(panel.Borders.Vert);
            Console.ResetColor();
        }

        // ── Layout constants ──────────────────────────────────────────────────

        // Logo box: 17 rows (0-16). Row 17 blank. Row 18 INFO. Row 19 blank.
        // Menu: ╔ row 20, title row 21, ╠ row 22, items start row 23.
        private const int MENU_ITEMS_START_ROW = 23;

        private static int MenuRow(int index) => MENU_ITEMS_START_ROW + index;

        // ── Navigation ────────────────────────────────────────────────────────

        private static int GetFirstSelectable()
        {
            for (int i = 0; i < selectable.Length; i++)
                if (selectable[i]) return i;
            return 0;
        }

        private static void MovePrev()
        {
            int cur = _selected;
            do _selected = _selected > 0 ? _selected - 1 : selectable.Length - 1;
            while (!selectable[_selected] && _selected != cur);
        }

        private static void MoveNext()
        {
            int cur = _selected;
            do _selected = _selected < selectable.Length - 1 ? _selected + 1 : 0;
            while (!selectable[_selected] && _selected != cur);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static ConsoleColor ItemColor(int i)
        {
            if (!selectable[i])
                return (i == 0 || i == 4) ? ConsoleColor.Green : ConsoleColor.DarkGray;

            return i switch
            {
                1 or 2      => ConsoleColor.Cyan,
                5 or 6 or 7 => ConsoleColor.Magenta,
                11          => ConsoleColor.Red,
                _           => ConsoleColor.White,
            };
        }

        private static string GetDescription(int i) => i switch
        {
            0  => L.Get(L.DESC_SECTION_HUMAN),
            1  => L.Get(L.DESC_SINGLE_PLAYER),
            2  => L.Get(L.DESC_TWO_PLAYER),
            4  => L.Get(L.DESC_SECTION_AI),
            5  => L.Get(L.DESC_DUAL_AI),
            6  => L.Get(L.DESC_SPLIT_AI),
            7  => L.Get(L.DESC_TOURNAMENT),
            9  => L.Get(L.DESC_SETTINGS),
            11 => L.Get(L.DESC_QUIT),
            _  => L.Get(L.DESC_DEFAULT),
        };
    }
}
