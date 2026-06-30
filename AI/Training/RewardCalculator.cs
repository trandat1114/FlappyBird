using FlappyBird.Models;

namespace FlappyBird.AI.Training;

/// <summary>
/// Reward signal definitions for reinforcement learning.
///
/// Design: sparse + dense hybrid.
///   Dense rewards (per-frame) give gradient signal for survival and centering.
///   Sparse rewards (events) give strong signal for key outcomes.
/// </summary>
public static class RewardCalculator
{
    public const float SurvivedFrame  =  0.01f;   // per frame alive
    public const float PassedPipe     =  1.00f;   // pipe scored
    public const float Died           = -1.00f;   // any collision
    public const float GapCenterBonus =  0.05f;   // when near gap center and pipe is close

    /// <summary>
    /// Per-frame reward: survival + optional gap-centering bonus.
    /// Call this every frame the bird is alive, BEFORE checking collision.
    /// </summary>
    public static float Frame(GameState gs)
    {
        float r = SurvivedFrame;

        // Centering bonus: only when next pipe is within 12 cols
        var pipe = gs.Pipes
            .Where(p => p.X >= GameState.BirdX - 2)
            .OrderBy(p => p.X)
            .FirstOrDefault();

        if (pipe != null && pipe.X - GameState.BirdX <= 12)
        {
            int gapCenter = pipe.TopHeight + pipe.GapSize / 2;
            if (Math.Abs(gs.BirdY - gapCenter) <= 1)
                r += GapCenterBonus;
        }

        return r;
    }

    /// <summary>Returns "top_pipe", "bottom_pipe", "floor", or "ceiling".</summary>
    public static string DeathCause(GameState gs)
    {
        if (gs.BirdY <= 0) return "ceiling";
        if (gs.BirdY >= GameState.GameHeight - 1) return "floor";

        var pipe = gs.Pipes
            .Where(p => p.X >= GameState.BirdX - 3 && p.X <= GameState.BirdX + 3)
            .OrderBy(p => p.X)
            .FirstOrDefault();

        if (pipe == null) return "floor";
        return gs.BirdY <= pipe.TopHeight ? "top_pipe" : "bottom_pipe";
    }
}
