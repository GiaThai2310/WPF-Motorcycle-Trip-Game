# Game-engine integration specification

## Purpose and current status

This document is the shared task sheet and handoff contract for integrating all
member branches into the WPF Motorcycle Road Trip game.

Thai's engine foundation is implemented and the project builds. Final gameplay
integration is **not complete** until every scaffold in the status table has
been replaced and the final checklist has passed.

| Area | Owner | Current status |
| --- | --- | --- |
| Constants and states | Thai | Implemented |
| Shared game loop and transitions | Thai | Implemented |
| Keyboard and restart wiring | Thai | Implemented |
| Motorcycle physics | Khang | Implemented |
| Obstacles and spawning | Vinh | Implemented |
| Collision service | Dang | Implemented |
| Score manager | Khang | Implemented |
| Visual manager and final UI | Quan | Needs fixes: end-state UI, restart-button visibility, and final-score consistency |
| XAML, assets, and styles | Van | Implemented |

## Build and runtime baseline

- Project: `WPF-Motorcycle-Trip-Game.csproj`
- Target: `net10.0-windows`
- UI framework: WPF
- Root namespace: `WPF_Motorcycle_Trip_Game`
- Startup window: `MainWindow.xaml`
- Fixed MVP coordinate system: 1000 x 450

Build command:

```powershell
dotnet build WPF-Motorcycle-Trip-Game.csproj
```

No subsystem may add another `DispatcherTimer`, `System.Timers.Timer`,
`System.Threading.Timer`, render loop, or background gameplay loop.

## Engine lifecycle

`MainWindow` constructs all subsystems and passes them to `GameEngine`.
`GameEngine` is the sole owner of the game timer and `GameState`.

State transitions:

```text
Application opens
    -> Waiting

Waiting + Space/Up
    -> Running
    -> BikeController.Jump()

Running + collision
    -> GameOver
    -> timer stops

Running + score reaches 1000
    -> Victory
    -> timer stops

Any non-disposed state + R/Restart button
    -> reset every subsystem
    -> Waiting

Window closes
    -> GameEngine.Dispose()
    -> timer is detached and stopped
```

The first `Space` or `Up` both starts the game and requests a jump. Calling
`Start()` directly only starts the run.

## Per-frame contract

Only while state is `Running`, `GameEngine` executes this exact order:

```text
1. BikeController.Update(deltaTime)
2. ObstacleManager.Update(deltaTime, GameConstants.BaseObstacleSpeed)
3. CollisionService.HasCollision(bike.Bounds, obstacleBounds)
4. If collision: enter GameOver and return immediately
5. ScoreManager.Update(deltaTime)
6. VisualManager.UpdateScore(score)
7. If target reached: enter Victory
```

`deltaTime` is measured in seconds using a monotonic `Stopwatch`. It is capped
at 0.1 seconds so restoring a paused or stalled UI does not create an extreme
physics step.

## Public contracts

Namespaces are part of the contract.

### `WPF_Motorcycle_Trip_Game.Core.GameState`

```csharp
public enum GameState
{
    Waiting,
    Running,
    GameOver,
    Victory
}
```

### `WPF_Motorcycle_Trip_Game.Controllers.BikeController`

```csharp
public sealed class BikeController
{
    public BikeController(FrameworkElement bikeElement);
    public bool IsGrounded { get; }
    public Rect Bounds { get; }
    public void Jump();
    public void Update(double deltaTime);
    public void Reset();
}
```

Requirements:

- Use `GameConstants.BikeStartX`, `GroundY`, `BikeWidth`, and `BikeHeight`.
- Interpret `GroundY` as the road surface; the bike's visual top at rest is
  `GroundY - BikeHeight`.
- Prevent double-jump internally.
- `Bounds` must return finite canvas coordinates and use the agreed hitbox
  insets.

### `WPF_Motorcycle_Trip_Game.Managers.ObstacleManager`

```csharp
public sealed class ObstacleManager
{
    public ObstacleManager(Canvas gameCanvas);
    public IReadOnlyList<Rect> ObstacleBounds { get; }
    public void Update(double deltaTime, double speed);
    public void Reset();
}
```

Requirements:

- Spawn and remove obstacle visuals only on the supplied canvas.
- Use distance countdown and the shared minimum/maximum spawn gaps.
- `ObstacleBounds` must represent the current frame after `Update`.
- `Reset()` must remove every obstacle visual and reset spawn state.

### `WPF_Motorcycle_Trip_Game.Services.CollisionService`

```csharp
public static class CollisionService
{
    public static bool HasCollision(
        Rect bikeBounds,
        IEnumerable<Rect> obstacleBounds);
}
```

This method is pure: it must not change UI, state, collections, or movement.

### `WPF_Motorcycle_Trip_Game.Managers.ScoreManager`

```csharp
public sealed class ScoreManager
{
    public int Score { get; }
    public bool HasReachedTarget { get; }
    public void Update(double deltaTime);
    public void Reset();
}
```

The manager must use a `double` accumulator, expose an integer score, and clamp
at `GameConstants.TargetScore`.

### `WPF_Motorcycle_Trip_Game.Managers.VisualManager`

```csharp
public sealed class VisualManager
{
    public VisualManager(
        TextBlock scoreText,
        TextBlock messageText,
        Button restartButton);
    public void ShowWaiting();
    public void ShowRunning();
    public void ShowGameOver(int score);
    public void ShowVictory();
    public void UpdateScore(int score);
}
```

Visual methods must only update presentation. They must not create a timer or
change the engine state.

Additional end-state UI requirements:

- `ShowGameOver(int score)` must clearly display the Game Over state and the
  exact `score` value passed by `GameEngine`.
- The score shown in the Game Over message and `ScoreText` must match the same
  `ScoreManager.Score` snapshot. `VisualManager` must not calculate or maintain
  a separate score.
- `ShowGameOver(int score)` and `ShowVictory()` must make `RestartButton`
  visible and usable.
- `ShowRunning()` must hide the end-state message and `RestartButton`.
- End-state text and the restart button must remain readable, visible, and
  free from overlap or clipping.

## Required XAML element names

The following fields are referenced directly by `MainWindow.xaml.cs` and must
not be renamed:

```xml
<Canvas x:Name="GameCanvas" />
<Image x:Name="BikeImage" />
<TextBlock x:Name="ScoreText" />
<TextBlock x:Name="MessageText" />
<Button x:Name="RestartButton" />
```

The window must retain these handlers unless code-behind is updated in the same
integration change:

```xml
KeyDown="OnWindowKeyDown"
Closed="OnWindowClosed"
Click="OnRestartClick"
```

Assets must use project-relative WPF resource URIs, never absolute machine
paths.

## Shared constants

`Core/GameConstants.cs` is the single source of truth. Do not duplicate or
independently change these values:

| Constant | Value |
| --- | ---: |
| `WindowWidth` | 1000 |
| `WindowHeight` | 450 |
| `GroundY` | 330 |
| `BikeStartX` | 120 |
| `BikeWidth` | 120 |
| `BikeHeight` | 80 |
| `JumpVelocity` | -650 |
| `Gravity` | 1800 |
| `BaseObstacleSpeed` | 360 |
| `TargetScore` | 1000 |
| `ScorePerSecond` | 25 |
| `MinimumSpawnGap` | 320 |
| `MaximumSpawnGap` | 560 |

## Merge procedure

Integrate in this order:

1. Van: final XAML, styles, and assets.
2. Quan: visual manager.
3. Khang: bike controller and score manager.
4. Vinh: models and obstacle manager.
5. Dang: collision service.
6. Thai: resolve integration errors and run the final checklist.

For each merge:

1. Preserve public method names, constructor signatures, namespaces, and XAML
   names.
2. Replace the complete scaffold file owned by that member.
3. Search the merged code for extra timers.
4. Build immediately.
5. Test reset before merging the next subsystem.

## Final acceptance checklist

- [ ] Exactly one timer exists, in `GameEngine`.
- [ ] `Space` starts a waiting game and jumps.
- [ ] `Space` and `Up` jump only while allowed by bike physics.
- [ ] The bike cannot double-jump and returns exactly to the road.
- [ ] All three obstacle types spawn without overlap.
- [ ] Obstacles move using the supplied speed and delta time.
- [ ] Collision immediately enters `GameOver`.
- [ ] The Game Over UI clearly shows the loss state, final score, and restart
      action without overlap or clipping.
- [ ] The final score shown in the Game Over message exactly matches
      `ScoreText` and `ScoreManager.Score`.
- [ ] Score changes only while state is `Running`.
- [ ] Score reaches and never exceeds 1000.
- [ ] Reaching 1000 enters `Victory`.
- [ ] `R` and the Restart button reset bike, obstacles, score, and visuals.
- [ ] `RestartButton` is visible and clickable in both `GameOver` and
      `Victory`.
- [ ] Closing the window stops and detaches the timer.
- [ ] Assets load from a fresh clone.
- [ ] The project builds with zero errors.

## Known integration limitations

At the time this document was last reviewed:

- `RestartButton` remains hidden in the current Game Over and Victory views.
- The Game Over score display has been reported as inconsistent with the
  actual score and requires a single-source-of-truth UI fix and runtime
  verification.
- The final acceptance checklist has not yet been completed end to end.

These are pending final-UI and integration-verification tasks.
