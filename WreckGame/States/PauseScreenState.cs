using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WreckGame.Managers;
using WreckGame.Utilities;

namespace WreckGame.States
{
    public class PauseScreenState : GameState
    {
        private readonly InputManager _inputManager;
        private readonly GraphicsManager _graphicsManager;
        private readonly UI.Button[] _buttons;
        private readonly MainGameState _previousGameState;

        public PauseScreenState(Game1 game, InputManager inputManager, GraphicsManager graphicsManager, MainGameState previousGameState) : base(game)
        {
            _inputManager = inputManager;
            _graphicsManager = graphicsManager;
            _previousGameState = previousGameState;
            _buttons = new UI.Button[3];
            _buttons[0] = new UI.Button(_graphicsManager, "RESUME", 2.0f, Game.GraphicsDevice.Viewport.Height / 2 + 50);
            _buttons[1] = new UI.Button(_graphicsManager, "RESTART", 2.0f, Game.GraphicsDevice.Viewport.Height / 2 + 130);
            _buttons[2] = new UI.Button(_graphicsManager, "EXIT", 2.0f, Game.GraphicsDevice.Viewport.Height / 2 + 210);
        }

        public override void Update(GameTime gameTime)
        {
            if (_inputManager.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) && 
                _inputManager.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) && 
                _inputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.C))
            {
                GameState.EditMode = !GameState.EditMode;
            }
            
            if (_inputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                Game.SetState(_previousGameState); 
            }
            if (_inputManager.IsLeftMousePressed())
            {
                Point mousePoint = _inputManager.GetMousePosition();
                if (_buttons[0].Contains(mousePoint))
                {
                    Game.SetState(_previousGameState);
                }
                else if (_buttons[1].Contains(mousePoint))
                {
                    Game.SetState(new MainGameState(Game, _inputManager, _graphicsManager));
                }
                else if (_buttons[2].Contains(mousePoint))
                {
                    Game.Exit();
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _graphicsManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            
            _graphicsManager.SpriteBatch.Draw(
                _graphicsManager.LoadTexture("misc/pixel"), 
                new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), 
                new Color(0, 0, 255)
            );
            
            // Calculate title position and draw it
            Vector2 titleSize = Utilities.Utilities.MeasureText("PAUSED", 3f, 8f);
            Vector2 titlePosition = new Vector2(
                (Game.GraphicsDevice.Viewport.Width - titleSize.X) / 2,
                (Game.GraphicsDevice.Viewport.Height - titleSize.Y) / 2 - 120
            );
            Utilities.Utilities.DrawColoredText(
                _graphicsManager.SpriteBatch, 
                "PAUSED", 
                titlePosition, 
                Color.White, 
                Color.Red, 
                3f, 
                true, 
                8f
            );
            
            // Update button positions relative to title
            int buttonY1 = (int)titlePosition.Y + (int)titleSize.Y + 50; // 50px below title
            int buttonY2 = buttonY1 + 80;  // 80px below first button
            int buttonY3 = buttonY2 + 80;  // 80px below second button
            
            // Update button positions
            _buttons[0].UpdateVerticalPosition(buttonY1); // RESUME
            _buttons[1].UpdateVerticalPosition(buttonY2); // RESTART
            _buttons[2].UpdateVerticalPosition(buttonY3); // EXIT
            
            foreach (var button in _buttons) button.Draw(_graphicsManager.SpriteBatch, _inputManager);
            _graphicsManager.SpriteBatch.End();
        }
    }
}