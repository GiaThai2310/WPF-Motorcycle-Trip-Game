using System.Windows;
using System.Windows.Controls;
using WPF_Motorcycle_Trip_Game.Core;

namespace WPF_Motorcycle_Trip_Game.Controllers;

// Implements full motorcycle jump physics, gravity, ground clamping,
// double-jump prevention, and hitbox insets per the integration spec.
public sealed class BikeController
{
    // Calculating the ground rest position for the TOP of the motorcycle image.
    // Since the Canvas Y coordinate increases downwards, the bike's top edge when resting
    // is simply the ground level minus its height.
    private static readonly double RestY =
        GameConstants.GroundY - GameConstants.BikeHeight;

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
    /// Gets the hitbox rectangle for collision detection.
    /// To make the game feel fair and prevent frustrating deaths, the hitbox is slightly
    /// smaller than the actual image (insets: Left +10, Top +10, Right -12, Bottom -8).
    /// </summary>
    public Rect Bounds => new(
        GameConstants.BikeStartX + 10,
        _currentY + 10,
        GameConstants.BikeWidth - 22,
        GameConstants.BikeHeight - 18);

    /// <summary>
    /// Triggers the jump action. This checks if the motorcycle is already in the air
    /// to prevent double-jumping. If grounded, it applies the initial upward velocity.
    /// </summary>
    public void Jump()
    {
        // Don't allow jumping if already in the air.
        if (!_isGrounded)
        {
            return;
        }

        // Apply the upward force. Negative velocity moves the object UP on the canvas.
        _velocityY = GameConstants.JumpVelocity;
        _isGrounded = false;
    }

    /// <summary>
    /// Updates the motorcycle's physics for the current frame.
    /// Applies gravity over time and updates the vertical position, making sure
    /// the bike lands perfectly on the road.
    /// </summary>
    public void Update(double deltaTime)
    {
        if (_isGrounded)
        {
            // No physics calculations needed if we are just driving on the road.
            return;
        }

        // Gravity constantly pulls the bike downwards.
        _velocityY += GameConstants.Gravity * deltaTime;

        // Update the current Y position based on velocity.
        _currentY += _velocityY * deltaTime;

        // Collision check with the ground. If the bike falls below the ground level,
        // snap it back to the road surface, kill the vertical momentum, and mark as grounded.
        if (_currentY >= RestY)
        {
            _currentY = RestY;
            _velocityY = 0;
            _isGrounded = true;
        }

        // Apply the new position to the UI element.
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
