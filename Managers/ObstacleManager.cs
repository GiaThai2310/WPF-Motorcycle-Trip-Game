using System.Windows;
using System.Windows.Controls;

namespace WPF_Motorcycle_Trip_Game.Managers;

// Integration scaffold for the member-owned obstacle implementation.
public sealed class ObstacleManager
{
    public ObstacleManager(Canvas gameCanvas) { }

    public IReadOnlyList<Rect> ObstacleBounds { get; } = Array.Empty<Rect>();

    public void Update(double deltaTime, double speed) { }

    public void Reset() { }
}
