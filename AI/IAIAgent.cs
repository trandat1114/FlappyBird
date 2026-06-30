using FlappyBird.Models;

namespace FlappyBird.AI;

/// <summary>
/// Common interface for all AI agents (rule-based or trained).
/// </summary>
public interface IAIAgent
{
    string Name { get; }

    /// <summary>Called once per frame. Agent observes state and decides whether to jump.</summary>
    bool ShouldJump(GameState gs);

    /// <summary>Called after each frame with the reward signal for that step.</summary>
    void OnFrameReward(float reward);

    /// <summary>Called when an episode ends (game over).</summary>
    void OnEpisodeEnd(bool died, int score, int survivalFrames);

    void Save(string path);
    void Load(string path);
}
