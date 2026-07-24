using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WPF_Motorcycle_Trip_Game.Core;
using WPF_Motorcycle_Trip_Game.Models;

namespace WPF_Motorcycle_Trip_Game.Managers;

// Owned by: Vinh
// Manages distance-based spawning, leftward translation, and cleanup of active obstacle entities.
public sealed class ObstacleManager
{
    private readonly Canvas _gameCanvas;
    private readonly List<Obstacle> _obstacles = new();
    private readonly Random _random = new();
    private double _spawnCountdownDistance;

    public ObstacleManager(Canvas gameCanvas)
    {
        _gameCanvas = gameCanvas ?? throw new ArgumentNullException(nameof(gameCanvas));
        Reset();
    }

    // Returns collection of bounding rectangles for all active obstacles on screen.
    public IReadOnlyList<Rect> ObstacleBounds => _obstacles.Select(o => o.Bounds).ToList();

    public void Update(double deltaTime, double speed)
    {
        if (deltaTime <= 0) return;

        // 1. Distance-based obstacle spawning
        _spawnCountdownDistance -= speed * deltaTime;
        if (_spawnCountdownDistance <= 0)
        {
            SpawnObstacle();
            double gap = _random.NextDouble() * (GameConstants.MaximumSpawnGap - GameConstants.MinimumSpawnGap) + GameConstants.MinimumSpawnGap;
            _spawnCountdownDistance = gap;
        }

        // 2. Move active obstacles from right to left
        for (int i = _obstacles.Count - 1; i >= 0; i--)
        {
            var obstacle = _obstacles[i];
            double newX = obstacle.X - (speed * deltaTime);

            // 3. Remove obstacles after leaving the screen
            if (newX + obstacle.Width < 0)
            {
                _gameCanvas.Children.Remove(obstacle.VisualElement);
                _obstacles.RemoveAt(i);
            }
            else
            {
                obstacle.UpdatePosition(newX);
            }
        }
    }

    public void Reset()
    {
        // Remove every active obstacle visual from canvas
        foreach (var obstacle in _obstacles)
        {
            _gameCanvas.Children.Remove(obstacle.VisualElement);
        }

        _obstacles.Clear();

        // Initialize spawn countdown with a random gap so first obstacle spawns after initial distance
        _spawnCountdownDistance = _random.NextDouble() * (GameConstants.MaximumSpawnGap - GameConstants.MinimumSpawnGap) + GameConstants.MinimumSpawnGap;
    }

    private void SpawnObstacle()
    {
        // Randomly pick one of the 3 obstacle types: Pedestrian, Pothole, Rock
        Array types = Enum.GetValues(typeof(ObstacleType));
        ObstacleType randomType = (ObstacleType)types.GetValue(_random.Next(types.Length))!;

        double spawnX = _gameCanvas.ActualWidth > 0 ? _gameCanvas.ActualWidth : GameConstants.WindowWidth;
        Obstacle obstacle = new Obstacle(randomType, spawnX);

        _obstacles.Add(obstacle);
        _gameCanvas.Children.Add(obstacle.VisualElement);
    }
}
