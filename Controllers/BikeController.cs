using System.Windows;
using System.Windows.Controls;
using WPF_Motorcycle_Trip_Game.Core;

namespace WPF_Motorcycle_Trip_Game.Controllers;

// Integration scaffold. Member-owned implementation can replace this class
// without changing GameEngine or MainWindow.
public sealed class BikeController
{
    private readonly FrameworkElement _bikeElement;

    public BikeController(FrameworkElement bikeElement)
    {
        _bikeElement = bikeElement;
        Reset();
    }

    public bool IsGrounded => true;

    public Rect Bounds => new(
        Canvas.GetLeft(_bikeElement),
        Canvas.GetTop(_bikeElement),
        GameConstants.BikeWidth,
        GameConstants.BikeHeight);

    public void Jump() { }

    public void Update(double deltaTime) { }

    public void Reset()
    {
        Canvas.SetLeft(_bikeElement, GameConstants.BikeStartX);
        Canvas.SetTop(_bikeElement, GameConstants.GroundY - GameConstants.BikeHeight);
    }
}
