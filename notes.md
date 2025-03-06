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