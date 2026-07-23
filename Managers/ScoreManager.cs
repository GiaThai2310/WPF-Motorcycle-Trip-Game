using WPF_Motorcycle_Trip_Game.Core;

namespace WPF_Motorcycle_Trip_Game.Managers;

// Integration scaffold for the member-owned score implementation.
public sealed class ScoreManager
{
    private double _scoreAccumulator;

    public int Score { get; private set; }
    public bool HasReachedTarget => Score >= GameConstants.TargetScore;

    public void Update(double deltaTime)
    {
        _scoreAccumulator += deltaTime * GameConstants.ScorePerSecond;
        Score = Math.Min(GameConstants.TargetScore, (int)_scoreAccumulator);
    }

    public void Reset()
    {
        _scoreAccumulator = 0;
        Score = 0;
    }
}
