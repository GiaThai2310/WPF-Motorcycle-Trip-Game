using System.Windows;

namespace WPF_Motorcycle_Trip_Game.Services;
public static class CollisionService
{
    public static bool HasCollision(
        Rect bikeBounds,
        IEnumerable<Rect> obstacleBounds)
    {
        return obstacleBounds.Any(bikeBounds.IntersectsWith);
    }
}