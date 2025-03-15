using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WreckGame.Managers;

namespace WreckGame.States
{
    public class StartScreenState : GameState
    {
        private readonly InputManager _inputManager;
        private readonly GraphicsManager _graphicsManager;
        private readonly UI.Button[] _buttons;

        public StartScreenState(Game1 game, InputManager inputManager, GraphicsManager graphicsManager) : base(game)
        {
            _inputManager = inputManager;
            _graphicsManager = graphicsManager;
            _buttons = new UI.Button[2];
            _buttons[0] = new UI.Button(_graphicsManager, "START", 2.0f, Game.GraphicsDevice.Viewport.Height / 2 + 50);
            _buttons[1] = new UI.Button(_graphicsManager, "EXIT", 2.0f, Game.GraphicsDevice.Viewport.Height / 2 + 130);
        }

        public override void Update(GameTime gameTime)
        {
            if (_inputManager.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) && 
                _inputManager.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) && 
                _inputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.C))
            {
                GameState.EditMode = !GameState.EditMode;
            }

            if (_inputManager.IsLeftMousePressed())
            {
                Point mousePoint = _inputManager.GetMousePosition();
                if (_buttons[0].Contains(mousePoint))
                {
                    Game.SetState(new MainGameState(Game, _inputManager, _graphicsManager));
                }
                else if (_buttons[1].Contains(mousePoint))
                {
                    Game.Exit();
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _graphicsManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            _graphicsManager.SpriteBatch.Draw(_graphicsManager.LoadTexture("misc/pixel"), new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), new Color(255, 165, 0, 255));
            
            // Calculate title position and draw it
            Vector2 titleSize = Utilities.Utilities.MeasureText("WRECK GAME", 3f, 8f);
            Vector2 titlePosition = new Vector2(
                (Game.GraphicsDevice.Viewport.Width - titleSize.X) / 2, 
                (Game.GraphicsDevice.Viewport.Height - titleSize.Y) / 2 - 100
            );
            Utilities.Utilities.DrawColoredText(
                _graphicsManager.SpriteBatch, 
                "WRECK GAME", 
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
            
            // Update button positions
            _buttons[0].UpdateVerticalPosition(buttonY1);
            _buttons[1].UpdateVerticalPosition(buttonY2);
            
            foreach (var button in _buttons) button.Draw(_graphicsManager.SpriteBatch, _inputManager);
            _graphicsManager.SpriteBatch.End();
        }
    }
}