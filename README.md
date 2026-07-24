# Motorcycle Trip Game

A fast-paced, side-scrolling 2D motorcycle game built entirely with Windows Presentation Foundation (WPF) and C#, with inspiration from the Google Chrome Dinosaur Game.

## Gameplay
Take control of a motorcycle cruising down a seemingly endless highway. The goal is to survive as long as possible and reach a target score by jumping over incoming obstacles. 

### Controls
* **Spacebar or Up Arrow**: Start the game and jump over obstacles.
* **R**: Restart the game manually if needed. (Player can also press Space/Up to instantly restart after a game over).

### Mechanics
The game features a fully custom physics engine that handles gravity and jumping mechanics. The motorcycle is affected by gravity while in the air and snaps back to the ground upon landing. You can only jump while on the ground, so timing is crucial to avoid crashing into the randomly generated obstacles.

## Features
* **Custom Physics**: Smooth jumping mechanics with acceleration and gravity.
* **Dynamic Obstacles**: Obstacles spawn at randomized intervals, keeping the gameplay unpredictable.
* **Score System**: Survive and accumulate points to reach the victory target. 
* **Seamless Restarts**: Quickly jump straight back into the action after a game over with a single button press.

## Tech Stack
* **Framework**: .NET (WPF)
* **Language**: C#
* **Architecture**: The game uses a custom tick-based game engine loop (running at roughly 60 FPS) to orchestrate state, physics, rendering, and collision detection independently. 

## How to Run
Open the solution or project in Visual Studio or use the .NET CLI:
```bash
dotnet run
```