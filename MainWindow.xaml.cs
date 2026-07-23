using System.Windows;
using System.Windows.Input;
using WPF_Motorcycle_Trip_Game.Controllers;
using WPF_Motorcycle_Trip_Game.Core;
using WPF_Motorcycle_Trip_Game.Managers;

namespace WPF_Motorcycle_Trip_Game;

public partial class MainWindow : Window
{
    private readonly GameEngine _gameEngine;

    public MainWindow()
    {
        InitializeComponent();

        _gameEngine = new GameEngine(
            new BikeController(BikeImage),
            new ObstacleManager(GameCanvas),
            new ScoreManager(),
            new VisualManager(ScoreText, MessageText, RestartButton));
    }

    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Space:
            case Key.Up:
                _gameEngine.Jump();
                e.Handled = true;
                break;

            case Key.R:
                _gameEngine.Restart();
                e.Handled = true;
                break;
        }
    }

    private void OnRestartClick(object sender, RoutedEventArgs e)
    {
        _gameEngine.Restart();
        Keyboard.Focus(this);
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        _gameEngine.Dispose();
    }
}
