# Wreck of the Fallen Star

## Overview
"Wreck of the Fallen Star" is a 2D exploration game developed with MonoGame and .NET 8.0. Players navigate through a mysterious environment, collecting resources while avoiding enemies.

## Features
- Exploration-based gameplay in a 2D environment
- Resource collection system (data shards, parts, and charges)
- Enemy AI with different behaviors
- Custom pixel art graphics
- Interactive elements and particle effects

## Technical Architecture
The game is built using the following architecture:

### Core Components
- **Game Engine**: Built on MonoGame 3.8.2
- **Asset Pipeline**: Custom content pipeline for efficient asset loading
- **Manager System**: Modular managers handling specific game systems

### Directory Structure
```
WreckOfTheFallenStar/
├── WreckGame/               # Main project directory
│   ├── Content/             # Game assets
│   │   ├── entities/        # Character and enemy sprites
│   │   ├── font/            # Custom font textures
│   │   ├── interactives/    # Interactive object sprites
│   │   ├── items/           # Collectible item sprites
│   │   ├── misc/            # Miscellaneous assets
│   │   ├── particles/       # Particle effect sprites
│   │   ├── tiles/           # Environmental tile sprites
│   ├── Managers/            # Game system managers
│   ├── Utilities/           # Helper classes and functions
├── Docs/                    # Project documentation
```

## Requirements
- .NET 8.0 SDK
- MonoGame 3.8.2+
- Windows, macOS, or Linux operating system

## Getting Started
1. Clone the repository:
   ```
   git clone https://github.com/yourusername/WreckOfTheFallenStar.git
   ```

2. Navigate to the project directory:
   ```
   cd WreckOfTheFallenStar/WreckGame
   ```

3. Build and run the project:
   ```
   dotnet build
   dotnet run
   ```

## Development
For details on development practices and guidelines, see [DevNotes.md](DevNotes.md).
For gameplay information, see [Gameplay.md](Gameplay.md).