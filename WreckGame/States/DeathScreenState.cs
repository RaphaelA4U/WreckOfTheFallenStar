using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WreckGame.Managers;
using WreckGame.Utilities;

namespace WreckGame.States
{
    public class DeathScreenState : GameState
    {
        private InputManager _inputManager;
        private GraphicsManager _graphicsManager;
        private DeathReason _deathReason;
        private UI.Button[] _buttons;

        public DeathScreenState(Game1 game, InputManager inputManager, GraphicsManager graphicsManager, DeathReason deathReason) : base(game)
        {
            _inputManager = inputManager;
            _graphicsManager = graphicsManager;
            _deathReason = deathReason;
            _buttons = new UI.Button[2];
            _buttons[0] = new UI.Button(_graphicsManager, "RESTART", 2.0f, Game.GraphicsDevice.Viewport.Height / 2 + 50);
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
            _graphicsManager.SpriteBatch.Draw(_graphicsManager.LoadTexture("misc/pixel"), new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), new Color(255, 0, 0, 255));
            
            // Calculate title position and draw it
            Vector2 titleSize = Utilities.Utilities.MeasureText("GAME OVER", 3f, 8f);
            Vector2 titlePosition = new Vector2(
                (Game.GraphicsDevice.Viewport.Width - titleSize.X) / 2,
                (Game.GraphicsDevice.Viewport.Height - titleSize.Y) / 2 - 140
            );
            Utilities.Utilities.DrawColoredText(
                _graphicsManager.SpriteBatch, 
                "GAME OVER", 
                titlePosition, 
                Color.White, 
                Color.Red, 
                3f, 
                true, 
                8f
            );
            
            // Calculate death message position and draw it
            string deathMessage = GetDeathMessage(_deathReason);
            Vector2 deathSize = Utilities.Utilities.MeasureText(deathMessage, 1.5f, 4f);
            Vector2 deathPosition = new Vector2(
                (Game.GraphicsDevice.Viewport.Width - deathSize.X) / 2,
                (Game.GraphicsDevice.Viewport.Height - deathSize.Y) / 2 - 40
            );
            Utilities.Utilities.DrawColoredText(
                _graphicsManager.SpriteBatch,
                deathMessage,
                deathPosition,
                Color.DarkRed,
                Color.Transparent,
                1.5f,
                false,
                4f
            );
            
            // Update button positions relative to death message
            int buttonY1 = (int)deathPosition.Y + (int)deathSize.Y + 50; // 50px below death message
            int buttonY2 = buttonY1 + 80;  // 80px below first button
            
            // Update button positions
            _buttons[0].UpdateVerticalPosition(buttonY1);
            _buttons[1].UpdateVerticalPosition(buttonY2);
            
            foreach (var button in _buttons) button.Draw(_graphicsManager.SpriteBatch, _inputManager);
            _graphicsManager.SpriteBatch.End();
        }

        private string GetDeathMessage(DeathReason reason)
        {
            return reason switch
            {
                DeathReason.EnemyCollision => "DESTROYED BY ENEMY COLLISION",
                DeathReason.EnemyBullet => "DESTROYED BY ENEMY FIRE",
                DeathReason.EnergyDepleted => "ENERGY DEPLETED",
                DeathReason.DebugKill => "DEBUG KILL",
                _ => "DRONE DESTROYED"
            };
        }
    }

    public enum DeathReason
    {
        DebugKill,
        Unknown,
        EnemyCollision,
        EnemyBullet,
        EnergyDepleted
    }
}