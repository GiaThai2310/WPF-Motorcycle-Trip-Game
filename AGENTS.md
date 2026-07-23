# Repository instructions

Read [docs/THAI_GAME_ENGINE_INTEGRATION.md](docs/THAI_GAME_ENGINE_INTEGRATION.md)
before implementing or integrating a game subsystem.

## Non-negotiable integration rules

- Keep exactly one game loop. Only `Core/GameEngine.cs` may create a timer.
- Do not rename the public contracts listed in the integration specification.
- Do not rename `GameCanvas`, `BikeImage`, `ScoreText`, `MessageText`, or
  `RestartButton`.
- Movement and scoring must use the `deltaTime` supplied by `GameEngine`.
- A subsystem must not change `GameState`; only `GameEngine` owns state
  transitions.
- Reset methods must restore their subsystem completely and must be safe to call
  before the first game starts.
- Do not commit generated `bin/`, `obj/`, or `.vs/` content.
- Run `dotnet build WPF-Motorcycle-Trip-Game.csproj` after integration.
- Keep `.github/workflows/build.yml` passing. CI restores and builds the WPF
  project in Release mode on Windows with warnings treated as errors.

## File ownership

- Thai: `Core/*`, `MainWindow.xaml.cs`, `App.xaml`, final integration.
- Khang: `Controllers/BikeController.cs`.
- Vinh: `Models/*`, `Managers/ObstacleManager.cs`.
- Dang: `Services/CollisionService.cs`.
- Van: `Managers/ScoreManager.cs`.
- Quan: `MainWindow.xaml`, `Managers/VisualManager.cs`, `Resources/*`,
  `Assets/*`.

Some member-owned files currently contain integration scaffolds. Their comments
and the status table in the integration specification identify these files.
Replace scaffolds in the owning branch without changing their public contracts.
