using System.Text.Json;
using FlappyBird.Models;

namespace FlappyBird.AI.Training;

/// <summary>
/// Tabular Q-learning agent.
///
/// State: 6000 discrete states (birdY×10, velocity×6, pipeDistX×10, gapCenterY×10).
/// Actions: 0 = no jump, 1 = jump.
/// Update: Q(s,a) += α * (r + γ * max Q(s',·) − Q(s,a))
///
/// Persists Q-table to JSON between sessions.
/// </summary>
public class QLearningAgent : IAIAgent
{
    // ── Hyperparameters ──────────────────────────────────────────────────────
    private float _alpha   = 0.1f;    // learning rate
    private float _gamma   = 0.95f;   // discount
    private float _epsilon = 1.0f;    // exploration probability

    private const float EpsilonDecay = 0.995f;
    private const float EpsilonMin   = 0.01f;

    // ── Q-table ───────────────────────────────────────────────────────────────
    private readonly Dictionary<int, float[]> _q = new();

    // ── Episode state ─────────────────────────────────────────────────────────
    private int   _lastState  = -1;
    private int   _lastAction = -1;
    private float _frameReward;
    private float _episodeTotalReward;
    private int   _episode;

    // ── Metrics history ───────────────────────────────────────────────────────
    private readonly List<EpisodeMetrics> _history = new();

    public string Name    => "Q-Learning";
    public float  Epsilon => _epsilon;
    public int    Episode => _episode;
    public IReadOnlyList<EpisodeMetrics> History => _history;

    // ── IAIAgent ──────────────────────────────────────────────────────────────

    public bool ShouldJump(GameState gs)
    {
        int state = StateKey(gs);

        // TD update for previous step (not on first frame of episode)
        if (_lastState >= 0)
        {
            float[] qNext = GetQ(state);
            float   maxQ  = Math.Max(qNext[0], qNext[1]);
            float[] qLast = GetQ(_lastState);
            qLast[_lastAction] += _alpha * (_frameReward + _gamma * maxQ - qLast[_lastAction]);
            _episodeTotalReward += _frameReward;
            _frameReward = 0f;
        }

        // ε-greedy action selection
        int action;
        if (Random.Shared.NextDouble() < _epsilon)
        {
            action = Random.Shared.Next(2);
        }
        else
        {
            float[] q = GetQ(state);
            action = q[1] > q[0] ? 1 : 0;
        }

        _lastState  = state;
        _lastAction = action;
        return action == 1;
    }

    public void OnFrameReward(float reward) => _frameReward += reward;

    public void OnEpisodeEnd(bool died, int score, int survivalFrames)
    {
        // Final TD update with terminal reward
        if (_lastState >= 0)
        {
            float terminalR = died ? RewardCalculator.Died : 0f;
            float[] qLast = GetQ(_lastState);
            qLast[_lastAction] += _alpha * (terminalR - qLast[_lastAction]);
            _episodeTotalReward += terminalR;
        }

        _epsilon = Math.Max(EpsilonMin, _epsilon * EpsilonDecay);
        _episode++;

        var metrics = new EpisodeMetrics(
            Episode:         _episode,
            Score:           score,
            SurvivalFrames:  survivalFrames,
            PipesPassed:     score,
            DeathCause:      "unknown",
            TotalReward:     _episodeTotalReward,
            Epsilon:         _epsilon,
            QTableSize:      _q.Count,
            Timestamp:       DateTime.Now
        );
        _history.Add(metrics);
        AITrainingLogger.Append(Name, metrics);

        // Reset episode state
        _lastState  = -1;
        _lastAction = -1;
        _frameReward        = 0f;
        _episodeTotalReward = 0f;
    }

    // ── Persistence ───────────────────────────────────────────────────────────

    public void Save(string path)
    {
        try
        {
            var data = new QTableSave
            {
                Epsilon  = _epsilon,
                Episode  = _episode,
                QTable   = _q.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
            };
            File.WriteAllText(path, JsonSerializer.Serialize(data,
                new JsonSerializerOptions { WriteIndented = false }));
        }
        catch { /* never crash */ }
    }

    public void Load(string path)
    {
        try
        {
            if (!File.Exists(path)) return;
            var data = JsonSerializer.Deserialize<QTableSave>(File.ReadAllText(path));
            if (data == null) return;
            _epsilon = Math.Max(EpsilonMin, data.Epsilon);
            _episode = data.Episode;
            _q.Clear();
            foreach (var kv in data.QTable)
                if (int.TryParse(kv.Key, out int k))
                    _q[k] = kv.Value;
        }
        catch { /* corrupt file — start fresh */ }
    }

    // ── State discretisation ─────────────────────────────────────────────────

    private static int StateKey(GameState gs)
    {
        // Bird Y: 10 buckets over [0, GameHeight)
        int birdB = Math.Clamp(gs.BirdY * 10 / GameState.GameHeight, 0, 9);

        // Velocity: 6 buckets over [-1, +1]
        int velB  = Math.Clamp((int)((gs.BirdVelocity + 1.0f) * 3.0f), 0, 5);

        // Next pipe ahead of bird
        var pipe = gs.Pipes
            .Where(p => p.X >= GameState.BirdX - 2)
            .OrderBy(p => p.X)
            .FirstOrDefault();

        int distB, gapB;
        if (pipe != null)
        {
            int dist = Math.Max(0, pipe.X - GameState.BirdX);
            distB = Math.Clamp(dist * 10 / GameState.GameWidth, 0, 9);
            int gapCenter = pipe.TopHeight + pipe.GapSize / 2;
            gapB  = Math.Clamp(gapCenter * 10 / GameState.GameHeight, 0, 9);
        }
        else
        {
            distB = 9;
            gapB  = 5;
        }

        // Key range: [0, 6000)
        return birdB * 600 + velB * 100 + distB * 10 + gapB;
    }

    private float[] GetQ(int state)
    {
        if (!_q.TryGetValue(state, out var q))
            _q[state] = q = new float[2];
        return q;
    }

    private sealed class QTableSave
    {
        public float   Epsilon { get; set; }
        public int     Episode { get; set; }
        public Dictionary<string, float[]> QTable { get; set; } = new();
    }
}
