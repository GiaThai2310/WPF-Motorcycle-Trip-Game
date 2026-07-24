using System;
using System.Windows;
using System.Windows.Controls;
using WPF_Motorcycle_Trip_Game.Core;

namespace WPF_Motorcycle_Trip_Game.Managers;

/// <summary>
/// Managed by: Quan (feature/game-ui)
/// Controls the UI presentation of the game including score display,
/// overlay text messages, restart button visibility, and dual-tile road scrolling.
/// Must NOT create any timers or modify GameState directly.
/// </summary>
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
        _scoreText = scoreText ?? throw new ArgumentNullException(nameof(scoreText));
        _messageText = messageText ?? throw new ArgumentNullException(nameof(messageText));
        _restartButton = restartButton ?? throw new ArgumentNullException(nameof(restartButton));
        _roadImage1 = roadImage1;
        _roadImage2 = roadImage2;
    }

    /// <summary>
    /// Moves dual road tiles left continuously and loops off-screen tiles to the right.
    /// </summary>
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

        Canvas.SetLeft(_roadImage1, _road1X);
        Canvas.SetLeft(_roadImage2, _road2X);
    }

    /// <summary>
    /// Resets road tile positions back to their starting arrangement.
    /// </summary>
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

    /// <summary>
    /// Displays initial prompt overlay before gameplay starts.
    /// </summary>
    public void ShowWaiting()
    {
        _messageText.Text = "Press Space or ↑ to start";
        _messageText.Visibility = Visibility.Visible;
        _restartButton.Visibility = Visibility.Collapsed;
        ResetRoad();
    }

    /// <summary>
    /// Hides message overlays while active gameplay is running.
    /// </summary>
    public void ShowRunning()
    {
        _messageText.Visibility = Visibility.Collapsed;
        _restartButton.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Displays Game Over outcome message and final score, and shows the Restart button.
    /// </summary>
    public void ShowGameOver(int score)
    {
        _messageText.Text = $"Game Over — Score: {score}\nPress Space or ↑ to restart";
        _messageText.Visibility = Visibility.Visible;
        _restartButton.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Displays Victory outcome message upon reaching target score (1000) and shows the Restart button.
    /// </summary>
    public void ShowVictory()
    {
        _messageText.Text = "Victory!\nPress Space or ↑ to restart";
        _messageText.Visibility = Visibility.Visible;
        _restartButton.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Dynamically updates the score HUD display text.
    /// </summary>
    public void UpdateScore(int score)
    {
        _scoreText.Text = $"Score: {score}";
    }
}
