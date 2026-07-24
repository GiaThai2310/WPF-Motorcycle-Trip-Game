using System.Windows;
using System.Windows.Controls;
using WPF_Motorcycle_Trip_Game.Core;

namespace WPF_Motorcycle_Trip_Game.Managers;

// Owned by: Quan
// Manages score UI updates, end-state overlay text, and infinite road scrolling visuals.
public sealed class VisualManager
{
    private readonly TextBlock _scoreText;
    private readonly TextBlock _messageText;
    private readonly Button _restartButton;
    private readonly Image? _roadImage1;
    private readonly Image? _roadImage2;

    private double _road1X = 0;
    private double _road2X = GameConstants.WindowWidth;

    public VisualManager(
        TextBlock scoreText,
        TextBlock messageText,
        Button restartButton,
        Image? roadImage1 = null,
        Image? roadImage2 = null)
    {
        _scoreText = scoreText;
        _messageText = messageText;
        _restartButton = restartButton;
        _roadImage1 = roadImage1;
        _roadImage2 = roadImage2;
    }

    // Moves dual road tiles left continuously and loops off-screen tiles to the right.
    public void UpdateRoad(double deltaTime, double speed)
    {
        if (_roadImage1 == null || _roadImage2 == null)
        {
            return;
        }

        double deltaX = speed * deltaTime;
        _road1X -= deltaX;
        _road2X -= deltaX;

        if (_road1X <= -GameConstants.WindowWidth)
        {
            _road1X += GameConstants.WindowWidth * 2;
        }

        if (_road2X <= -GameConstants.WindowWidth)
        {
            _road2X += GameConstants.WindowWidth * 2;
        }

        Canvas.SetLeft(_road1ImageOrElement(_roadImage1), _road1X);
        Canvas.SetLeft(_road1ImageOrElement(_roadImage2), _road2X);
    }

    private static Image _road1ImageOrElement(Image img) => img;

    // Resets road tile positions back to their starting arrangement.
    public void ResetRoad()
    {
        _road1X = 0;
        _road2X = GameConstants.WindowWidth;

        if (_roadImage1 != null)
        {
            Canvas.SetLeft(_roadImage1, 0);
        }

        if (_roadImage2 != null)
        {
            Canvas.SetLeft(_roadImage2, GameConstants.WindowWidth);
        }
    }

    // Displays initial prompt overlay before gameplay starts.
    public void ShowWaiting()
    {
        _messageText.Text = "Press Space or ↑ to start";
        _messageText.Visibility = Visibility.Visible;
        _restartButton.Visibility = Visibility.Collapsed;
        ResetRoad();
    }

    // Hides message overlays while active gameplay is running.
    public void ShowRunning()
    {
        _messageText.Visibility = Visibility.Collapsed;
        _restartButton.Visibility = Visibility.Collapsed;
    }

    // Displays Game Over outcome message and final score.
    public void ShowGameOver(int score)
    {
        _messageText.Text = $"Game Over — Score: {score}\nPress Space or ↑ to restart";
        _messageText.Visibility = Visibility.Visible;
        _restartButton.Visibility = Visibility.Collapsed;
    }

    // Displays Victory outcome message upon reaching target score.
    public void ShowVictory()
    {
        _messageText.Text = "Victory!\nPress Space or ↑ to restart";
        _messageText.Visibility = Visibility.Visible;
        _restartButton.Visibility = Visibility.Collapsed;
    }

    // Updates the score HUD display text.
    public void UpdateScore(int score) => _scoreText.Text = $"Score: {score}";
}
