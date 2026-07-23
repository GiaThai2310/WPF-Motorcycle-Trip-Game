using System.Windows;
using System.Windows.Controls;

namespace WPF_Motorcycle_Trip_Game.Managers;

// Integration scaffold for the member-owned visual implementation.
public sealed class VisualManager
{
    private readonly TextBlock _scoreText;
    private readonly TextBlock _messageText;
    private readonly Button _restartButton;

    public VisualManager(
        TextBlock scoreText,
        TextBlock messageText,
        Button restartButton)
    {
        _scoreText = scoreText;
        _messageText = messageText;
        _restartButton = restartButton;
    }

    public void ShowWaiting()
    {
        _messageText.Text = "Press Space or ↑ to start";
        _messageText.Visibility = Visibility.Visible;
        _restartButton.Visibility = Visibility.Collapsed;
    }

    public void ShowRunning()
    {
        _messageText.Visibility = Visibility.Collapsed;
        _restartButton.Visibility = Visibility.Collapsed;
    }

    public void ShowGameOver(int score)
    {
        _messageText.Text = $"Game Over — Score: {score}";
        _messageText.Visibility = Visibility.Visible;
        _restartButton.Visibility = Visibility.Visible;
    }

    public void ShowVictory()
    {
        _messageText.Text = "Victory!";
        _messageText.Visibility = Visibility.Visible;
        _restartButton.Visibility = Visibility.Visible;
    }

    public void UpdateScore(int score) => _scoreText.Text = $"Score: {score}";
}
