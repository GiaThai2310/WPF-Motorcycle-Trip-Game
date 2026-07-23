using System.Windows;

namespace WPF_Motorcycle_Trip_Game.Services;

// Integration scaffold for the member-owned collision implementation.
public static class CollisionService
{
    public static bool HasCollision(
        Rect bikeBounds,
        IEnumerable<Rect> obstacleBounds)
    {
        return obstacleBounds.Any(bikeBounds.IntersectsWith);
    }
}
