namespace FlappyBird.Models;

public sealed class ScoreEntry
{
    public string   Mode        { get; set; } = "SinglePlayer";
    public int      Score       { get; set; }
    public DateTime PlayedAt    { get; set; }
    // CustomGame session metadata (null = not a Custom Game session)
    public bool?    ManualMode  { get; set; }
    public int?     StartLevel  { get; set; }
    public int?     ManualSpeed { get; set; }
    public int?     ManualGap   { get; set; }
}
