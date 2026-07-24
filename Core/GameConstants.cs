namespace WPF_Motorcycle_Trip_Game.Core;

// Global configuration parameters and constants for layout, physics, and gameplay rules.
public static class GameConstants
{
    public const double WindowWidth = 1000;
    public const double WindowHeight = 450;
    public const double GroundY = 330;
    public const double BikeStartX = 120;
    public const double BikeWidth = 120;
    public const double BikeHeight = 80;
    public const double JumpVelocity = -650;
    public const double Gravity = 1800;
    public const double BaseObstacleSpeed = 360;
    public const double RoadHeight = WindowHeight - GroundY;
    public const int TargetScore = 1000;
    public const double ScorePerSecond = 20;
    public const double MinimumSpawnGap = 400;
    public const double MaximumSpawnGap = 700;
}
