using System.Windows;
using System.Windows.Controls;
using WPF_Motorcycle_Trip_Game.Core;

namespace WPF_Motorcycle_Trip_Game.Controllers;

// Owned by: Khang  (feature/bike-controller)
// Implements full motorcycle jump physics, gravity, ground clamping,
// double-jump prevention, and hitbox insets per the integration spec.
public sealed class BikeController
{
    // The ground rest position for the TOP of the bike element (in canvas coordinates).
    // Canvas Y increases downward, so the bike's top edge at rest = GroundY - BikeHeight.
    private static readonly double RestY =
        GameConstants.GroundY - GameConstants.BikeHeight; // 330 - 80 = 250

    private readonly FrameworkElement _bikeElement;

    // Vertical velocity in px/s. Negative = moving upward (canvas Y decreases).
    private double _velocityY;

    // Current canvas-top Y position of the bike element, tracked in code
    // so we accumulate fractional pixels without floating-point drift.
    private double _currentY;

    // True only when the bike is resting exactly on the road surface.
    // Prevents double-jumping.
    private bool _isGrounded;

    public BikeController(FrameworkElement bikeElement)
    {
        _bikeElement = bikeElement;
        Reset();
    }

    /// <summary>True when the bike is resting on the ground and a jump is allowed.</summary>
    public bool IsGrounded => _isGrounded;

    /// <summary>
    /// Hitbox rectangle in canvas coordinates, with insets per the task sheet:
    ///   Left +10, Top +10, Right -12, Bottom -8
    /// so the hitbox is slightly smaller than the image, making collisions feel fair.
    /// </summary>
    public Rect Bounds => new(
        GameConstants.BikeStartX + 10,
        _currentY + 10,
        GameConstants.BikeWidth - 22,   // 120 - 10 - 12
        GameConstants.BikeHeight - 18); // 80  - 10 -  8

    /// <summary>
    /// Initiates a jump if the bike is currently grounded.
    /// Called by GameEngine when Space or Up is pressed while Running.
    /// </summary>
    public void Jump()
    {
        if (!_isGrounded)
        {
            return; // Silently ignore double-jump attempts.
        }

        _velocityY = GameConstants.JumpVelocity; // -650 px/s (upward)
        _isGrounded = false;
    }

    /// <summary>
    /// Advances the bike physics by one frame.
    /// Called by GameEngine every tick while state is Running.
    /// </summary>
    public void Update(double deltaTime)
    {
        if (_isGrounded)
        {
            // Bike is resting; nothing to integrate.
            return;
        }

        // Apply gravity (positive = accelerates downward).
        _velocityY += GameConstants.Gravity * deltaTime; // 1800 px/s²

        // Integrate position.
        _currentY += _velocityY * deltaTime;

        // Clamp to ground: if the bike has reached or passed the road surface,
        // snap it exactly to the rest position and stop vertical motion.
        if (_currentY >= RestY)
        {
            _currentY = RestY;
            _velocityY = 0;
            _isGrounded = true;
        }

        Canvas.SetTop(_bikeElement, _currentY);
    }

    /// <summary>
    /// Returns the bike to its starting position with all physics state cleared.
    /// Safe to call before the first game starts and on every restart.
    /// </summary>
    public void Reset()
    {
        _currentY = RestY;
        _velocityY = 0;
        _isGrounded = true;

        Canvas.SetLeft(_bikeElement, GameConstants.BikeStartX);
        Canvas.SetTop(_bikeElement, _currentY);
    }
}
