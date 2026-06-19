using FlappyBird.Enum;
using FlappyBird.Localization;

namespace FlappyBird.UI
{
    public static class SimpleMenuSystem
    {
        private static int selectedIndex = 0;
        private static int previousSelectedIndex = -1;
        private static bool isFirstRender = true;
        private static bool forceFullRedraw = false;

        // Computed property – always returns current-language strings
        private static string[] menuItems => [
            L.Get(L.MENU_SECTION_HUMAN),
            L.Get(L.MENU_SINGLE_PLAYER),
            L.Get(L.MENU_TWO_PLAYER),
            "",
            L.Get(L.MENU_SECTION_AI),
            "       Dual AI Comparison",
            "       Split Screen Real-time",
            "       AI Tournament",
            "",
            L.Get(L.MENU_QUIT)
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
            MenuAction.Exit
        ];

        private static readonly bool[] selectableItems = [
            false, true, true, false, false, true, true, true, false, true
        ];

        public static MenuAction ShowMenu()
        {
            selectedIndex = GetFirstSelectableIndex();
            ConsoleKeyInfo keyInfo;

            if (isFirstRender || forceFullRedraw)
            {
                DrawMenu();
                isFirstRender = false;
                forceFullRedraw = false;
            }

            do
            {
                keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        previousSelectedIndex = selectedIndex;
                        MoveToPreviousSelectableItem();
                        if (previousSelectedIndex != selectedIndex)
                            UpdateMenuSelection();
                        break;
                    case ConsoleKey.DownArrow:
                        previousSelectedIndex = selectedIndex;
                        MoveToNextSelectableItem();
                        if (previousSelectedIndex != selectedIndex)
                            UpdateMenuSelection();
                        break;
                    case ConsoleKey.Enter:
                        if (selectableItems[selectedIndex])
                        {
                            forceFullRedraw = true;
                            return menuActions[selectedIndex];
                        }
                        break;
                    case ConsoleKey.Escape:
                        return MenuAction.Exit;
                }
            } while (true);
        }

        private static void DrawMenu()
        {
            var items = menuItems; // snapshot for this draw

            Console.Clear();
            Console.CursorVisible = false;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                                ║");
            Console.WriteLine("║       ███████╗██╗      █████╗ ██████╗ ██████╗ ██╗   ██╗        ║");
            Console.WriteLine("║       ██╔════╝██║     ██╔══██╗██╔══██╗██╔══██╗╚██╗ ██╔╝        ║");
            Console.WriteLine("║       █████╗  ██║     ███████║██████╔╝██████╔╝ ╚████╔╝         ║");
            Console.WriteLine("║       ██╔══╝  ██║     ██╔══██║██╔═══╝ ██╔═══╝   ╚██╔╝          ║");
            Console.WriteLine("║       ██║     ███████╗██║  ██║██║     ██║        ██║           ║");
            Console.WriteLine("║       ╚═╝     ╚══════╝╚═╝  ╚═╝╚═╝     ╚═╝        ╚═╝           ║");
            Console.WriteLine("║                                                                ║");
            Console.WriteLine("║                  ██████╗ ██╗██████╗ ██████╗                    ║");
            Console.WriteLine("║                  ██╔══██╗██║██╔══██╗██╔══██╗                   ║");
            Console.WriteLine("║                  ██████╔╝██║██████╔╝██║  ██║                   ║");
            Console.WriteLine("║                  ██╔══██╗██║██╔══██╗██║  ██║                   ║");
            Console.WriteLine("║                  ██████╔╝██║██║  ██║██████╔╝                   ║");
            Console.WriteLine("║                  ╚═════╝ ╚═╝╚═╝  ╚═╝╚═════╝                    ║");
            Console.WriteLine("║                                                                ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("INFO: " + GetMenuDescription(selectedIndex));
            Console.ResetColor();
            Console.WriteLine();

            // Menu title – centered
            string title = L.Get(L.MENU_TITLE);
            string centeredTitle = title.PadLeft((64 + title.Length) / 2).PadRight(64);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║{centeredTitle}║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");

            for (int i = 0; i < items.Length; i++)
            {
                if (string.IsNullOrEmpty(items[i]))
                {
                    Console.WriteLine("║                                                                ║");
                    continue;
                }

                string prefix = "║  ";
                if (i == selectedIndex && selectableItems[i])
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.Write(prefix + "► " + items[i]);
                    Console.ResetColor();
                    Console.WriteLine(new string(' ', Math.Max(0, 60 - items[i].Length)) + "║");
                }
                else
                {
                    Console.ForegroundColor = GetMenuItemColor(i);
                    Console.Write(prefix + "  " + items[i]);
                    Console.ResetColor();
                    Console.WriteLine(new string(' ', Math.Max(0, 60 - items[i].Length)) + "║");
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("┌────────────────────────────────────────────────────────────────┐");
            Console.WriteLine(L.Get(L.MENU_CTRL_LABEL));
            Console.WriteLine(L.Get(L.MENU_CTRL_MOVE));
            Console.WriteLine(L.Get(L.MENU_CTRL_SELECT));
            Console.WriteLine(L.Get(L.MENU_CTRL_EXIT));
            Console.WriteLine("└────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
        }

        static int GetMenuLinePosition(int menuIndex)
        {
            int linePos = 23;
            for (int i = 0; i < menuIndex; i++) linePos++;
            return linePos;
        }

        private static void UpdateMenuSelection()
        {
            var items = menuItems;
            Console.CursorVisible = false;

            // Update description
            Console.SetCursorPosition(6, 18);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(GetMenuDescription(selectedIndex).PadRight(58));
            Console.ResetColor();

            // Deselect previous
            if (previousSelectedIndex >= 0 && selectableItems[previousSelectedIndex])
            {
                Console.SetCursorPosition(0, GetMenuLinePosition(previousSelectedIndex));
                Console.ForegroundColor = GetMenuItemColor(previousSelectedIndex);
                Console.Write("║  " + "  " + items[previousSelectedIndex]);
                Console.ResetColor();
                Console.Write(new string(' ', Math.Max(0, 60 - items[previousSelectedIndex].Length)) + "║");
            }

            // Highlight new selection
            if (selectableItems[selectedIndex])
            {
                Console.SetCursorPosition(0, GetMenuLinePosition(selectedIndex));
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.Write("║  " + "► " + items[selectedIndex]);
                Console.ResetColor();
                Console.Write(new string(' ', Math.Max(0, 60 - items[selectedIndex].Length)) + "║");
            }
        }

        private static ConsoleColor GetMenuItemColor(int index)
        {
            if (!selectableItems[index])
                return (index == 0 || index == 4) ? ConsoleColor.Green : ConsoleColor.DarkGray;

            return index switch
            {
                1 or 2 => ConsoleColor.Cyan,
                5 or 6 or 7 => ConsoleColor.Magenta,
                9 => ConsoleColor.Red,
                _ => ConsoleColor.White
            };
        }

        private static string GetMenuDescription(int index) => index switch
        {
            0 => L.Get(L.DESC_SECTION_HUMAN),
            1 => L.Get(L.DESC_SINGLE_PLAYER),
            2 => L.Get(L.DESC_TWO_PLAYER),
            4 => L.Get(L.DESC_SECTION_AI),
            5 => L.Get(L.DESC_DUAL_AI),
            6 => L.Get(L.DESC_SPLIT_AI),
            7 => L.Get(L.DESC_TOURNAMENT),
            9 => L.Get(L.DESC_QUIT),
            _ => L.Get(L.DESC_DEFAULT)
        };

        private static int GetFirstSelectableIndex()
        {
            for (int i = 0; i < selectableItems.Length; i++)
                if (selectableItems[i]) return i;
            return 0;
        }

        private static void MoveToPreviousSelectableItem()
        {
            int current = selectedIndex;
            do
                selectedIndex = selectedIndex > 0 ? selectedIndex - 1 : selectableItems.Length - 1;
            while (!selectableItems[selectedIndex] && selectedIndex != current);
        }

        private static void MoveToNextSelectableItem()
        {
            int current = selectedIndex;
            do
                selectedIndex = selectedIndex < selectableItems.Length - 1 ? selectedIndex + 1 : 0;
            while (!selectableItems[selectedIndex] && selectedIndex != current);
        }
    }
}
