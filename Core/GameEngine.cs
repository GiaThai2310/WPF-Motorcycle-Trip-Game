using System.Diagnostics;
using System.Windows.Threading;
using WPF_Motorcycle_Trip_Game.Controllers;
using WPF_Motorcycle_Trip_Game.Managers;
using WPF_Motorcycle_Trip_Game.Services;

namespace WPF_Motorcycle_Trip_Game.Core;

// Central game engine loop orchestrating timing, state transitions, physics updates,
// collision detection, score management, and visual presentation.
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

        // Configure high-priority render timer running at approximately 60 FPS (16ms interval).
        _gameTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _gameTimer.Tick += OnGameTick;

        State = GameState.Waiting;
        _visualManager.UpdateScore(0);
        _visualManager.ShowWaiting();
    }

    // Current state of the game loop (Waiting, Running, GameOver, Victory).
    public GameState State { get; private set; }

    // Starts the game loop from the Waiting state.
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

    // Handles the jump control key press; automatically restarts if called from terminal states.
    public void Jump()
    {
        ThrowIfDisposed();

        // Allow Space or Up to restart the game when it's over or won.
        if (State == GameState.GameOver || State == GameState.Victory)
        {
            Restart();
        }

        if (State == GameState.Waiting)
        {
            Start();
        }

        if (State == GameState.Running)
        {
            _bikeController.Jump();
        }
    }

    // Resets all subsystems and returns the engine state to Waiting.
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

    // Stops and detaches the game loop timer upon application shutdown.
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

    // Per-frame game loop handler calculating delta time and evaluating updates in strict order:
    // 1. Motorcycle physics and obstacle movement
    // 2. Scrolling background elements
    // 3. Collision detection (triggers GameOver before score accumulation)
    // 4. Score accumulation and Victory condition evaluation
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

        double currentSpeed = GameConstants.BaseObstacleSpeed;
        if (_scoreManager.Score >= 700) currentSpeed *= 1.4;
        else if (_scoreManager.Score >= 400) currentSpeed *= 1.2;

        _obstacleManager.Update(deltaTime, currentSpeed);
        _visualManager.UpdateRoad(deltaTime, currentSpeed);

        // Immediate collision check to stop gameplay on impact.
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
