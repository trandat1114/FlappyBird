namespace FlappyBird.Localization;

/// <summary>
/// Static localization manager. Switch <see cref="Current"/> to change language at runtime.
/// Future: call <c>LoadFromJson(path)</c> to replace inline dictionaries with JSON files.
/// </summary>
public static class L
{
    // ── LANGUAGE ──────────────────────────────────────────────────────────
    public static Language Current { get; set; } = Language.English;

    /// <summary>Returns the localized string for <paramref name="key"/>, falling back to English.</summary>
    public static string Get(string key)
    {
        if (_strings.TryGetValue(Current, out var dict) && dict.TryGetValue(key, out var val))
            return val;
        return _strings[Language.English].GetValueOrDefault(key, key);
    }

    // ── KEYS ──────────────────────────────────────────────────────────────
    // Menu
    public const string MENU_TITLE          = nameof(MENU_TITLE);
    public const string MENU_SECTION_HUMAN  = nameof(MENU_SECTION_HUMAN);
    public const string MENU_SINGLE_PLAYER  = nameof(MENU_SINGLE_PLAYER);
    public const string MENU_TWO_PLAYER     = nameof(MENU_TWO_PLAYER);
    public const string MENU_SECTION_AI     = nameof(MENU_SECTION_AI);
    public const string MENU_QUIT           = nameof(MENU_QUIT);
    public const string MENU_CTRL_LABEL     = nameof(MENU_CTRL_LABEL);
    public const string MENU_CTRL_MOVE      = nameof(MENU_CTRL_MOVE);
    public const string MENU_CTRL_SELECT    = nameof(MENU_CTRL_SELECT);
    public const string MENU_CTRL_EXIT      = nameof(MENU_CTRL_EXIT);

    // Menu descriptions
    public const string DESC_SECTION_HUMAN  = nameof(DESC_SECTION_HUMAN);
    public const string DESC_SINGLE_PLAYER  = nameof(DESC_SINGLE_PLAYER);
    public const string DESC_TWO_PLAYER     = nameof(DESC_TWO_PLAYER);
    public const string DESC_SECTION_AI     = nameof(DESC_SECTION_AI);
    public const string DESC_DUAL_AI        = nameof(DESC_DUAL_AI);
    public const string DESC_SPLIT_AI       = nameof(DESC_SPLIT_AI);
    public const string DESC_TOURNAMENT     = nameof(DESC_TOURNAMENT);
    public const string DESC_QUIT           = nameof(DESC_QUIT);
    public const string DESC_DEFAULT        = nameof(DESC_DEFAULT);

    // Gameplay status
    public const string STATUS_READY        = nameof(STATUS_READY);
    public const string STATUS_SCORE_FMT    = nameof(STATUS_SCORE_FMT);

    // Controls
    public const string CTRL_MANUAL         = nameof(CTRL_MANUAL);
    public const string CTRL_JUMP           = nameof(CTRL_JUMP);
    public const string CTRL_START          = nameof(CTRL_START);
    public const string CTRL_EXIT           = nameof(CTRL_EXIT);
    public const string CTRL_PLAY_AGAIN     = nameof(CTRL_PLAY_AGAIN);

    // Game Over
    public const string GO_PLAY_AGAIN       = nameof(GO_PLAY_AGAIN);
    public const string GO_MAIN_MENU        = nameof(GO_MAIN_MENU);
    public const string GO_POINTS           = nameof(GO_POINTS);
    public const string GO_CONTROLS         = nameof(GO_CONTROLS);

    // Two Player
    public const string TP_JUMP_P1          = nameof(TP_JUMP_P1);
    public const string TP_JUMP_P2          = nameof(TP_JUMP_P2);
    public const string TP_START_RESTART    = nameof(TP_START_RESTART);
    public const string TP_WINNER           = nameof(TP_WINNER);
    public const string TP_TIE              = nameof(TP_TIE);
    public const string TP_TIE_EXCLAIM      = nameof(TP_TIE_EXCLAIM);
    public const string TP_PLAYING          = nameof(TP_PLAYING);
    public const string TP_OUT              = nameof(TP_OUT);
    public const string TP_GO_CONTROLS      = nameof(TP_GO_CONTROLS);

    // AI modes
    public const string AI_ROUND_TITLE      = nameof(AI_ROUND_TITLE);
    public const string AI_CONSERVATIVE     = nameof(AI_CONSERVATIVE);
    public const string AI_AGGRESSIVE       = nameof(AI_AGGRESSIVE);
    public const string AI_SCORE_LINE       = nameof(AI_SCORE_LINE);
    public const string AI_STATUS_PLAYING   = nameof(AI_STATUS_PLAYING);
    public const string AI_STATUS_LOST      = nameof(AI_STATUS_LOST);
    public const string AI_WINNER_FMT       = nameof(AI_WINNER_FMT);
    public const string AI_TIE              = nameof(AI_TIE);
    public const string AI_SUMMARY_TITLE    = nameof(AI_SUMMARY_TITLE);
    public const string AI_CONS_WINS        = nameof(AI_CONS_WINS);
    public const string AI_AGG_WINS         = nameof(AI_AGG_WINS);
    public const string AI_TIES             = nameof(AI_TIES);
    public const string AI_START_HINT       = nameof(AI_START_HINT);
    public const string AI_RUNNING          = nameof(AI_RUNNING);
    public const string AI_FINAL_WINNER     = nameof(AI_FINAL_WINNER);

    public const string SPLIT_TITLE         = nameof(SPLIT_TITLE);
    public const string SPLIT_START_HINT    = nameof(SPLIT_START_HINT);
    public const string SPLIT_RESTART_HINT  = nameof(SPLIT_RESTART_HINT);

    // ── DICTIONARIES ──────────────────────────────────────────────────────
    // Future: replace with public static void LoadFromJson(string langCode, string path)
    // which populates _strings[lang] from a JSON file (keys match constants above).
    private static readonly Dictionary<Language, Dictionary<string, string>> _strings = new()
    {
        [Language.English] = new()
        {
            [MENU_TITLE]         = "MAIN MENU",
            [MENU_SECTION_HUMAN] = "   HUMAN PLAYERS",
            [MENU_SINGLE_PLAYER] = "       Single Player",
            [MENU_TWO_PLAYER]    = "       Two Players",
            [MENU_SECTION_AI]    = "   AI TRAINING",
            [MENU_QUIT]          = "   Quit",
            [MENU_CTRL_LABEL]    = "│    Controls     :                                              │",
            [MENU_CTRL_MOVE]     = "│    ↑ ↓          : Move selection                               │",
            [MENU_CTRL_SELECT]   = "│    ENTER        : Select                                        │",
            [MENU_CTRL_EXIT]     = "│    ESC          : Exit                                          │",

            [DESC_SECTION_HUMAN] = "Game modes for human players",
            [DESC_SINGLE_PLAYER] = "Conquer the game on your own",
            [DESC_TWO_PLAYER]    = "Two players at once: [W] (Player 1) and [↑] (Player 2)",
            [DESC_SECTION_AI]    = "AI training and testing modes",
            [DESC_DUAL_AI]       = "Compare performance between 2 different AIs",
            [DESC_SPLIT_AI]      = "Watch 2 AIs play simultaneously on a split screen",
            [DESC_TOURNAMENT]    = "AI tournament with multiple algorithms",
            [DESC_QUIT]          = "Exit the game",
            [DESC_DEFAULT]       = "Use arrow keys to navigate",

            [STATUS_READY]       = "Get ready...",
            [STATUS_SCORE_FMT]   = "Score",

            [CTRL_MANUAL]        = "MANUAL CONTROL",
            [CTRL_JUMP]          = "SPACE: Jump",
            [CTRL_START]         = "SPACE: Start",
            [CTRL_EXIT]          = "ESC: Exit",
            [CTRL_PLAY_AGAIN]    = "Space: Play again",

            [GO_PLAY_AGAIN]      = "Play Again",
            [GO_MAIN_MENU]       = "Main Menu",
            [GO_POINTS]          = "pts",
            [GO_CONTROLS]        = "Up/Down: Select    Enter: Confirm    Space: Play Again",

            [TP_JUMP_P1]         = "[W] Player 1 Jump",
            [TP_JUMP_P2]         = "[↑] Player 2 Jump",
            [TP_START_RESTART]   = "[SPACE] Start / Play Again",
            [TP_WINNER]          = "WINNER",
            [TP_TIE]             = "TIE",
            [TP_TIE_EXCLAIM]     = "*** TIE! ***",
            [TP_PLAYING]         = "PLAYING",
            [TP_OUT]             = "OUT",
            // max 62 chars (64 inner - 2 prefix spaces in WriteRow)
            [TP_GO_CONTROLS]     = "[↑↓] Select  [Enter] OK  [Space] Play Again  [ESC] Menu",

            [AI_ROUND_TITLE]     = "Round",
            [AI_CONSERVATIVE]    = "Conservative AI",
            [AI_AGGRESSIVE]      = "Aggressive AI",
            [AI_SCORE_LINE]      = "pts",
            [AI_STATUS_PLAYING]  = "PLAYING",
            [AI_STATUS_LOST]     = "LOST",
            [AI_WINNER_FMT]      = "WINNER",
            [AI_TIE]             = "TIE",
            [AI_SUMMARY_TITLE]   = "OVERALL STATS:",
            [AI_CONS_WINS]       = "Conservative AI Wins",
            [AI_AGG_WINS]        = "Aggressive AI Wins  ",
            [AI_TIES]            = "Ties                ",
            [AI_START_HINT]      = "SPACE: Start  |  R: Restart  |  ESC: Menu",
            [AI_RUNNING]         = "Running...",
            [AI_FINAL_WINNER]    = "WINNER",

            [SPLIT_TITLE]        = "SPLIT SCREEN AI  -  In Development",
            [SPLIT_START_HINT]   = "SPACE: Start  |  R: Restart  |  ESC: Menu",
            [SPLIT_RESTART_HINT] = "R: Restart  |  ESC: Menu",
        },
        [Language.Vietnamese] = new()
        {
            [MENU_TITLE]         = "MENU CHÍNH",
            [MENU_SECTION_HUMAN] = "   NGƯỜI CHƠI",
            [MENU_SINGLE_PLAYER] = "       Chơi đơn",
            [MENU_TWO_PLAYER]    = "       Chơi đôi",
            [MENU_SECTION_AI]    = "   LUYỆN AI",
            [MENU_QUIT]          = "   Thoát",
            [MENU_CTRL_LABEL]    = "│    Điều khiển   :                                              │",
            [MENU_CTRL_MOVE]     = "│    ↑ ↓          : Di chuyển lựa chọn                           │",
            [MENU_CTRL_SELECT]   = "│    ENTER        : Chọn                                          │",
            [MENU_CTRL_EXIT]     = "│    ESC          : Thoát                                         │",

            [DESC_SECTION_HUMAN] = "Chế độ dành cho người chơi thật",
            [DESC_SINGLE_PLAYER] = "Chinh phục trò chơi một mình",
            [DESC_TWO_PLAYER]    = "Hai người chơi cùng lúc: [W] (Player 1) và [↑] (Player 2)",
            [DESC_SECTION_AI]    = "Các chế độ huấn luyện và thử nghiệm AI",
            [DESC_DUAL_AI]       = "So sánh hiệu suất giữa 2 AI khác nhau",
            [DESC_SPLIT_AI]      = "Xem 2 AI chơi cùng lúc trên màn hình chia đôi",
            [DESC_TOURNAMENT]    = "Giải đấu AI với nhiều thuật toán khác nhau",
            [DESC_QUIT]          = "Thoát khỏi game",
            [DESC_DEFAULT]       = "Sử dụng phím mũi tên để điều hướng",

            [STATUS_READY]       = "Chuẩn bị sẵn sàng...",
            [STATUS_SCORE_FMT]   = "Điểm",

            [CTRL_MANUAL]        = "ĐIỀU KHIỂN",
            [CTRL_JUMP]          = "SPACE: Nhảy",
            [CTRL_START]         = "SPACE: Bắt đầu",
            [CTRL_EXIT]          = "ESC: Thoát",
            [CTRL_PLAY_AGAIN]    = "Space: Chơi lại",

            [GO_PLAY_AGAIN]      = "Chơi lại",
            [GO_MAIN_MENU]       = "Về menu chính",
            [GO_POINTS]          = "điểm",
            [GO_CONTROLS]        = "Trên/Dưới: Chọn    Enter: Xác nhận    Space: Chơi lại",

            [TP_JUMP_P1]         = "[W] P1 Nhảy",
            [TP_JUMP_P2]         = "[↑] P2 Nhảy",
            [TP_START_RESTART]   = "[SPACE] Bắt đầu / Chơi lại",
            [TP_WINNER]          = "THẮNG",
            [TP_TIE]             = "HÒA",
            [TP_TIE_EXCLAIM]     = "*** HÒA! ***",
            [TP_PLAYING]         = "ĐANG CHƠI",
            [TP_OUT]             = "ĐÃ THUA",
            // max 62 chars (64 inner - 2 prefix spaces in WriteRow)
            [TP_GO_CONTROLS]     = "[↑↓] Chọn  [Enter] OK  [Space] Chơi lại  [ESC] Menu",

            [AI_ROUND_TITLE]     = "Vòng",
            [AI_CONSERVATIVE]    = "AI Thận trọng",
            [AI_AGGRESSIVE]      = "AI Tấn công",
            [AI_SCORE_LINE]      = "điểm",
            [AI_STATUS_PLAYING]  = "ĐANG CHƠI",
            [AI_STATUS_LOST]     = "THUA",
            [AI_WINNER_FMT]      = "THẮNG",
            [AI_TIE]             = "HÒA",
            [AI_SUMMARY_TITLE]   = "THỐNG KÊ TỔNG HỢP:",
            [AI_CONS_WINS]       = "AI Thận trọng thắng ",
            [AI_AGG_WINS]        = "AI Tấn công thắng   ",
            [AI_TIES]            = "Hòa                 ",
            [AI_START_HINT]      = "SPACE: Bắt đầu  |  R: Chơi lại  |  ESC: Menu",
            [AI_RUNNING]         = "Đang chạy...",
            [AI_FINAL_WINNER]    = "THẮNG",

            [SPLIT_TITLE]        = "SPLIT SCREEN AI  -  Đang phát triển",
            [SPLIT_START_HINT]   = "SPACE: Bắt đầu  |  R: Chơi lại  |  ESC: Menu",
            [SPLIT_RESTART_HINT] = "R: Chơi lại  |  ESC: Menu",
        }
    };
}
