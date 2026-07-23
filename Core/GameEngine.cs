using System.Diagnostics;
using System.Windows.Threading;
using WPF_Motorcycle_Trip_Game.Controllers;
using WPF_Motorcycle_Trip_Game.Managers;
using WPF_Motorcycle_Trip_Game.Services;

namespace WPF_Motorcycle_Trip_Game.Core;

public sealed class GameEngine : IDisposable
{
    private const double MaximumDeltaTime = 0.1;
    private readonly DispatcherTimer _gameTimer;
    private readonly Stopwatch _frameClock = new();
    private readonly BikeController _bikeController;
    private readonly ObstacleManager _obstacleManager;
    private readonly ScoreManager _scoreManager;
    private readonly VisualManager _visualManager;
    private bool _disposed;

    public GameEngine(
        BikeController bikeController,
        ObstacleManager obstacleManager,
        ScoreManager scoreManager,
        VisualManager visualManager)
    {
        _bikeController = bikeController;
        _obstacleManager = obstacleManager;
        _scoreManager = scoreManager;
        _visualManager = visualManager;
        _gameTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _gameTimer.Tick += OnGameTick;

        State = GameState.Waiting;
        _visualManager.UpdateScore(0);
        _visualManager.ShowWaiting();
    }

    public GameState State { get; private set; }

    public void Start()
    {
        ThrowIfDisposed();
        if (State != GameState.Waiting)
        {
            return;
        }

        State = GameState.Running;
        _visualManager.ShowRunning();
        _frameClock.Restart();
        _gameTimer.Start();
    }

    public void Jump()
    {
        ThrowIfDisposed();
        if (State == GameState.Waiting)
        {
            Start();
        }

        if (State == GameState.Running)
        {
            _bikeController.Jump();
        }
    }

    public void Restart()
    {
        ThrowIfDisposed();
        _gameTimer.Stop();
        _frameClock.Reset();
        _bikeController.Reset();
        _obstacleManager.Reset();
        _scoreManager.Reset();
        State = GameState.Waiting;
        _visualManager.UpdateScore(0);
        _visualManager.ShowWaiting();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _gameTimer.Stop();
        _gameTimer.Tick -= OnGameTick;
        _frameClock.Stop();
        _disposed = true;
    }

    private void OnGameTick(object? sender, EventArgs e)
    {
        if (State != GameState.Running)
        {
            return;
        }

        double deltaTime = Math.Min(_frameClock.Elapsed.TotalSeconds, MaximumDeltaTime);
        _frameClock.Restart();
        if (deltaTime <= 0)
        {
            return;
        }

        _bikeController.Update(deltaTime);
        _obstacleManager.Update(deltaTime, GameConstants.BaseObstacleSpeed);

        if (CollisionService.HasCollision(
                _bikeController.Bounds,
                _obstacleManager.ObstacleBounds))
        {
            State = GameState.GameOver;
            StopGameplay();
            _visualManager.ShowGameOver(_scoreManager.Score);
            return;
        }

        _scoreManager.Update(deltaTime);
        _visualManager.UpdateScore(_scoreManager.Score);

        if (_scoreManager.HasReachedTarget)
        {
            State = GameState.Victory;
            StopGameplay();
            _visualManager.ShowVictory();
        }
    }

    private void StopGameplay()
    {
        _gameTimer.Stop();
        _frameClock.Stop();
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
