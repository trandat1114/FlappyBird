namespace FlappyBird.UI.Border;

/// <summary>
/// All box-drawing characters for a single border style.
/// TeeRight/TeeLeft = ╠ ╣ (horizontal divider connectors).
/// </summary>
public readonly record struct BorderSet(
    char TopLeft,  char TopRight,
    char BotLeft,  char BotRight,
    char Horiz,    char Vert,
    char TeeRight, char TeeLeft
);
