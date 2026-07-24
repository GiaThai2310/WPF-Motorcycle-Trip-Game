using WPF_Motorcycle_Trip_Game.Core;

namespace WPF_Motorcycle_Trip_Game.Managers;

// Owned by: Khang
// Manages real-time score accumulation, capping at target score, and state reset.
public sealed class ScoreManager
{
    private double _scoreAccumulator;

    // Gets current integer score value.
    public int Score { get; private set; }

    // Evaluates whether current score reached or surpassed the victory target.
    public bool HasReachedTarget => Score >= GameConstants.TargetScore;

    // Accumulates score over delta time and updates integer Score property capped at target limit.
    public void Update(double deltaTime)
    {
        _scoreAccumulator += deltaTime * GameConstants.ScorePerSecond;
        Score = Math.Min(GameConstants.TargetScore, (int)_scoreAccumulator);
    }

    // Resets score accumulator and integer score to zero.
    public void Reset()
    {
        _scoreAccumulator = 0;
        Score = 0;
    }
}
