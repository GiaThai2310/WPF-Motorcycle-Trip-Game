using System.Windows;

namespace WPF_Motorcycle_Trip_Game.Services;

// Provides static bounding-box intersection evaluation between motorcycle and active obstacles.
public static class CollisionService
{
    // Evaluates whether the motorcycle bounding box intersects with any active obstacle bounding box.
    public static bool HasCollision(
        Rect bikeBounds,
        IEnumerable<Rect> obstacleBounds)
    {
        return obstacleBounds.Any(bikeBounds.IntersectsWith);
    }
}
