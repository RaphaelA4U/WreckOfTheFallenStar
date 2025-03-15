# Wreck of the Fallen Star - Development Notes

## Getting Started
- To start the game:
    - Run `dotnet run` in the `/WreckGame` directory
    - Or press the "Play" button in your IDE

## Adding Content Files
To add assets (sprites, sounds, etc.) to the game:

1. Open the MonoGame Content Builder:
     ```bash
     dotnet mgcb-editor Content/Content.mgcb
     ```
2. Click "Add Existing Item" in the editor
3. Select your asset file (e.g., PNG for sprites)
4. Build the content to generate XNB files

> **Note:** Content files must be inside the project directory structure to be properly processed.

## Content Pipeline
- Content files are compiled into XNB format during build
- Access content in code using `Content.Load<T>("assetName")`

## Code Architecture

### Manager Classes
The game uses a manager-based architecture following dependency injection principles:

```csharp
// Example of manager implementation
public class InputManager
{
    private readonly GameWindow _window;
    
    public InputManager(GameWindow window)
    {
        _window = window;
    }
    
    // Methods for handling input
}
```

Managers should be injected into game components and states rather than created directly.

### Custom Font System
The game uses a custom font system with individual character textures:

1. Each character is stored as a separate texture in `/Content/font/`
2. The `Utilities.DrawColoredText()` method handles text rendering

To add new characters:
1. Create a PNG file for each new character
2. Add them to the `/Content/font/` directory
3. Include them in the content pipeline

### Entity System
Game entities follow a component-based design:

```csharp
// Basic entity structure
public class Entity
{
    public Vector2 Position { get; set; }
    public bool IsActive { get; set; }
    
    public virtual void Update(GameTime gameTime) { }
    public virtual void Draw(SpriteBatch spriteBatch) { }
}
```

### Particle System
The particle system handles visual effects:

```csharp
// Example usage of particle system
ParticleSystem.CreateExplosion(position, 20, 1.5f, Color.Orange);
```

## Debugging Tips

### Visual Studio Code
- Use the "Run and Debug" panel with the .NET Core configuration
- Add breakpoints by clicking in the left margin of the code editor

### Visual Studio
- Press F5 to start debugging
- Use the Immediate Window (Ctrl+Alt+I) to execute code during debugging

### Common Issues
- If assets aren't loading, check that the content pipeline is building them correctly
- For performance issues, use a profiler or add timing code to identify bottlenecks

## Pull Request Guidelines
1. Ensure the code follows the project's style guide
2. Add proper documentation for new features
3. Include test coverage for new functionality
4. Update relevant documentation files

## Performance Considerations
- Batch similar draw operations to minimize SpriteBatch state changes
- Implement spatial partitioning for large numbers of entities
- Reuse objects rather than creating new instances frequently