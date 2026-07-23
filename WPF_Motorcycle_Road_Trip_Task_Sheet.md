# WPF Motorcycle Road Trip — Task Sheet

## 1. Project goal

Build a short WPF game inspired by the offline runner format, but with a different theme:

- A man drives a motorcycle with a female passenger.
- The motorcycle stays near the left side of the screen.
- The road and obstacles move from right to left.
- The player presses `Space` or `↑` to jump.
- Obstacles include:
  - Pedestrian
  - Pothole
  - Rock
- Collision with any obstacle causes `Game Over`.
- The player wins when the score reaches `1000`.
- No database, game engine, Entity Framework, or external UI framework.

## 2. MVP rules

### Required

- `Space` or `↑` starts the game and makes the motorcycle jump.
- The motorcycle cannot double-jump.
- Gravity brings the motorcycle back to the road.
- Obstacles spawn from the right and move left.
- Obstacles must not overlap when spawned.
- Collision immediately ends the game.
- Score increases only while the game is running.
- Reaching `1000` points triggers `Victory`.
- `R` or a Restart button starts a new run.
- One shared game loop controls the entire game.

### Not included in MVP

- Left/right movement
- Multiple lives
- Database or leaderboard
- Login
- Multiple levels
- Items or power-ups
- AI/RAG
- Complex physics
- Full MVVM architecture

## 3. Technical approach

Use a simple WPF game structure:

- `Canvas` for positioning and moving game objects.
- `DispatcherTimer` for one shared game loop.
- `Canvas.SetLeft()` and `Canvas.SetTop()` for movement.
- `Rect.IntersectsWith()` for collision detection.
- `Image`, `Rectangle`, or `Border` for game visuals.
- `TextBlock` for score and messages.
- Code-behind only for window-level input and integration.
- Separate classes for movement, obstacles, collision, score, and UI updates.

## 4. Shared constants

These values must be agreed on before coding and should not be changed independently.

```csharp
public static class GameConstants
{
    public const double WindowWidth = 1000;
    public const double WindowHeight = 450;

    public const double GroundY = 330;

    public const double BikeStartX = 120;
    public const double BikeWidth = 120;
    public const double BikeHeight = 80;

    public const double JumpVelocity = -650;
    public const double Gravity = 1800;

    public const double BaseObstacleSpeed = 360;

    public const int TargetScore = 1000;
    public const double ScorePerSecond = 40;

    public const double MinimumSpawnGap = 320;
    public const double MaximumSpawnGap = 560;
}
```

Expected play time:

```text
1000 points ÷ 40 points/second ≈ 25 seconds
```

## 5. Project structure

```text
MotorcycleRoadTrip/
├── Core/
│   ├── GameConstants.cs
│   ├── GameEngine.cs
│   └── GameState.cs
├── Controllers/
│   └── BikeController.cs
├── Managers/
│   ├── ObstacleManager.cs
│   ├── ScoreManager.cs
│   └── VisualManager.cs
├── Models/
│   ├── Obstacle.cs
│   └── ObstacleType.cs
├── Services/
│   └── CollisionService.cs
├── Assets/
│   ├── motorcycle.png
│   ├── pedestrian.png
│   ├── pothole.png
│   └── rock.png
├── Resources/
│   └── Styles.xaml
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── App.xaml
└── App.xaml.cs
```

## 6. Shared contracts

All members must code against these method names and properties.

### `GameState`

```csharp
public enum GameState
{
    Waiting,
    Running,
    GameOver,
    Victory
}
```

### `BikeController`

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

### `ObstacleManager`

```csharp
public sealed class ObstacleManager
{
    public ObstacleManager(Canvas gameCanvas);

    public IReadOnlyList<Rect> ObstacleBounds { get; }

    public void Update(double deltaTime, double speed);
    public void Reset();
}
```

### `CollisionService`

```csharp
public static class CollisionService
{
    public static bool HasCollision(
        Rect bikeBounds,
        IEnumerable<Rect> obstacleBounds);
}
```

### `ScoreManager`

```csharp
public sealed class ScoreManager
{
    public int Score { get; }
    public bool HasReachedTarget { get; }

    public void Update(double deltaTime);
    public void Reset();
}
```

### `VisualManager`

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

## 7. Parallel task assignment

---

## Member 1 — Thái  
### Project setup, game loop, input, and final integration

#### Files

```text
Core/GameConstants.cs
Core/GameState.cs
Core/GameEngine.cs
MainWindow.xaml.cs
App.xaml
```

#### Tasks

- Create the WPF project and folder structure.
- Add the shared constants and `GameState`.
- Create one `DispatcherTimer`.
- Calculate `deltaTime` for every frame.
- Handle keyboard input:
  - `Space` or `↑`: start/jump
  - `R`: restart
- Call subsystems in this order:

```text
BikeController.Update()
ObstacleManager.Update()
CollisionService.HasCollision()
ScoreManager.Update()
VisualManager.UpdateScore()
```

- Change state to `GameOver` on collision.
- Change state to `Victory` when score reaches `1000`.
- Stop gameplay updates outside `Running`.
- Integrate all branches and fix build errors.

#### Acceptance criteria

- Only one game timer exists.
- The game supports `Waiting`, `Running`, `GameOver`, and `Victory`.
- Restart resets every subsystem.
- The final project builds and runs.

#### Branch

```text
feature/game-engine-integration
```

---

## Member 2 — Khang  
### Motorcycle jump movement and physics

#### Files

```text
Controllers/BikeController.cs
```

#### Tasks

- Receive the motorcycle `FrameworkElement`.
- Store vertical velocity.
- Implement jump using `JumpVelocity`.
- Apply gravity each frame.
- Prevent double-jump.
- Stop the motorcycle exactly at `GroundY`.
- Update the element position using `Canvas.SetTop()`.
- Return a collision rectangle slightly smaller than the image.
- Reset position and velocity on restart.

#### Suggested hitbox adjustment

```text
Left inset:   10 px
Top inset:    10 px
Right inset:  12 px
Bottom inset: 8 px
```

#### Acceptance criteria

- The motorcycle jumps only when grounded.
- The jump is smooth and frame-rate independent.
- The motorcycle never falls through the road.
- Restart returns it to the starting position.
- `Bounds` matches the visible motorcycle reasonably well.

#### Branch

```text
feature/bike-controller
```

---

## Member 3 — Vinh  
### Obstacle models, spawning, and movement

#### Files

```text
Models/Obstacle.cs
Models/ObstacleType.cs
Managers/ObstacleManager.cs
```

#### Tasks

- Create three obstacle types:
  - `Pedestrian`
  - `Pothole`
  - `Rock`
- Spawn obstacles at the right edge of the `Canvas`.
- Randomly choose obstacle type.
- Randomize spacing using the shared minimum and maximum gap.
- Move obstacles from right to left.
- Remove obstacles after leaving the screen.
- Maintain the obstacle list.
- Expose obstacle hitboxes through `ObstacleBounds`.
- Clear all obstacles on restart.
- Use placeholder shapes if final images are unavailable.

#### Spawn rule

Do not spawn based only on random time each frame.

Use traveled distance or a countdown:

```text
remainingDistance -= speed × deltaTime

if remainingDistance <= 0:
    spawn obstacle
    remainingDistance = random(minGap, maxGap)
```

#### Acceptance criteria

- Obstacles do not overlap when spawned.
- All three obstacle types can appear.
- Obstacles move at the provided speed.
- Off-screen obstacles are removed.
- Restart leaves no old obstacles on screen.

#### Branch

```text
feature/obstacle-manager
```

---

## Member 4 — Đăng  
### Collision detection and hitbox testing

#### Files

```text
Services/CollisionService.cs
```

#### Tasks

- Implement rectangle intersection using `Rect.IntersectsWith()`.
- Accept one motorcycle hitbox and multiple obstacle hitboxes.
- Return `true` on the first collision.
- Keep the service independent from UI elements.
- Test edge cases:
  - Jumping above a rock
  - Passing close to a pedestrian
  - Motorcycle touching the edge of a pothole
  - Empty obstacle list
- Document how hitbox insets affect difficulty.

#### Acceptance criteria

- No collision is reported when hitboxes do not overlap.
- Collision is reported when rectangles overlap.
- The service works with an empty list.
- It does not modify game state or UI.
- It contains no timer and no movement logic.

#### Branch

```text
feature/collision-service
```

---

## Member 5 — Văn  
### Score, win condition, and game-state messages

#### Files

```text
Managers/ScoreManager.cs
```

#### Tasks

- Increase score based on elapsed time.
- Use an internal `double` accumulator.
- Expose integer score to the UI.
- Clamp the maximum score to `1000`.
- Report when the target has been reached.
- Stop increasing when `Update()` is not called.
- Reset score to zero on restart.
- Prepare message text for:
  - Waiting
  - Game Over
  - Victory

#### Score logic

```csharp
_scoreAccumulator += deltaTime * GameConstants.ScorePerSecond;
Score = Math.Min(
    GameConstants.TargetScore,
    (int)_scoreAccumulator);
```

#### Acceptance criteria

- Score is frame-rate independent.
- Score reaches approximately `1000` after 25 seconds.
- Score never exceeds `1000`.
- Reset returns score to zero.
- Victory can be detected through `HasReachedTarget`.

#### Branch

```text
feature/score-manager
```

---

## Member 6 — Quân  
### Main UI, assets, styles, and visual states

#### Files

```text
MainWindow.xaml
Managers/VisualManager.cs
Resources/Styles.xaml
Assets/*
```

#### Tasks

- Build the main `Canvas`.
- Add:
  - Score display
  - Motorcycle image
  - Road and lane markings
  - Start instruction
  - Game Over/Victory message
  - Restart button
- Prepare or create the four required assets.
- Configure assets with relative paths and WPF `Resource` build action.
- Create reusable text and button styles.
- Implement `VisualManager`.
- Keep animation simple:
  - Wheel rotation or small vertical bounce while running
  - Fade message in/out
  - Moving road markings if time remains
- Ensure the interface remains usable at the fixed MVP window size.

#### Required element names

```xml
<Canvas x:Name="GameCanvas" />
<Image x:Name="BikeImage" />
<TextBlock x:Name="ScoreText" />
<TextBlock x:Name="MessageText" />
<Button x:Name="RestartButton" />
```

Do not rename these after integration starts.

#### Acceptance criteria

- All required elements are visible.
- Start, Game Over, and Victory messages are distinguishable.
- Restart button is hidden while running.
- Assets load on another computer.
- UI does not use absolute local file paths.

#### Branch

```text
feature/game-ui
```

## 8. Work sequence

### Phase 1 — Initial setup: 20–30 minutes

Thái creates and pushes:

- Empty WPF project
- Folder structure
- Shared constants
- `GameState`
- Empty class files with agreed method signatures
- `MainWindow.xaml` containing the named UI elements

Everyone pulls this commit before coding.

### Phase 2 — Parallel development

```text
Thái  → Game loop and integration
Khang → Motorcycle movement
Vinh  → Obstacles
Đăng  → Collision
Văn   → Score
Quân  → UI and assets
```

### Phase 3 — Local self-test

Each member must:

- Build the project.
- Test their class independently where possible.
- Commit only their assigned files.
- Write a short note describing:
  - Completed work
  - Public methods
  - Known issues

### Phase 4 — Integration order

Merge in this order:

```text
1. UI and named elements
2. BikeController
3. ObstacleManager
4. CollisionService
5. ScoreManager
6. GameEngine integration
```

### Phase 5 — Final testing

Run the complete checklist:

- Pressing `Space` starts the game.
- `Space` and `↑` make the motorcycle jump.
- Double-jump is blocked.
- Motorcycle returns to the road.
- All obstacle types spawn.
- Obstacles do not overlap.
- Collision causes Game Over.
- Score stops after Game Over.
- Score reaches `1000`.
- Reaching `1000` causes Victory.
- Restart resets score, motorcycle, obstacles, and messages.
- Closing the window stops the timer.
- Assets load after cloning onto another computer.

## 9. Git rules

Each member works on one branch only.

```text
feature/game-engine-integration
feature/bike-controller
feature/obstacle-manager
feature/collision-service
feature/score-manager
feature/game-ui
```

Rules:

- Do not edit files owned by another member.
- Do not rename shared classes, methods, or XAML elements.
- Pull the latest integration branch before opening a pull request.
- Keep commits small and descriptive.
- Do not commit generated `bin/` or `obj/` folders.
- Only Thái merges into the main integration branch.

Recommended commit messages:

```text
feat: implement motorcycle jump physics
feat: add obstacle spawn and movement
feat: add collision detection service
feat: implement score and victory condition
feat: create game canvas and visual states
fix: reset all game objects on restart
```

## 10. Priority levels

### P0 — Must complete

- Start
- Jump
- Obstacles
- Collision
- Score
- Game Over
- Victory at `1000`
- Restart

### P1 — Complete only after P0 works

- Three different obstacle images
- Animated road markings
- Wheel animation
- Sound effects
- High score during the current app session

### P2 — Do not start unless the whole MVP is stable

- Speed increase
- Additional scenes
- More controls
- Save data to a file
- Multiple difficulty modes

## 11. Definition of done

The MVP is considered finished when:

1. The project builds without errors.
2. A player can complete one full run.
3. Collision consistently triggers Game Over.
4. Reaching `1000` consistently triggers Victory.
5. Restart works without reopening the application.
6. No subsystem creates its own timer.
7. The source can run after being cloned onto another computer.
