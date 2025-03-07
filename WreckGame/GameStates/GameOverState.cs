using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WreckGame.Graphics;
using WreckGame.Input;

namespace WreckGame.GameStates
{
    public class GameOverState : GameState
    {
        private TextRenderer _textRenderer;
        private Texture2D _backgroundTexture;
        private float _elapsedTime;
        private float _textScale = 3f;
        private float _pulsateSpeed = 2f;
        
        public GameOverState(Game1 game, InputManager inputManager) 
            : base(game, inputManager)
        {
        }
        
        public override void LoadContent()
        {
            // Load text renderer
            var letterTextures = new System.Collections.Generic.Dictionary<char, Texture2D>();
            for (char c = 'a'; c <= 'z'; c++)
            {
                letterTextures[c] = _content.Load<Texture2D>(c.ToString());
            }
            for (char c = 'A'; c <= 'Z'; c++)
            {
                letterTextures[c] = _content.Load<Texture2D>(c.ToString().ToLower());
            }
            
            _textRenderer = new TextRenderer(letterTextures);
            
            // Create black background texture
            _backgroundTexture = new Texture2D(_game.GraphicsDevice, 1, 1);
            _backgroundTexture.SetData(new[] { Color.Black });
            
            base.LoadContent();
        }
        
        public override void OnStateEnter()
        {
            _elapsedTime = 0;
        }
        
        public override void Update(GameTime gameTime)
        {
            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Check for restart
            if (_inputManager.IsKeyPressed(Keys.Space) || 
                _inputManager.IsKeyPressed(Keys.Enter))
            {
                _game.StateManager.SwitchState<PlayingState>();
            }
        }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            int viewportWidth = _game.GraphicsDevice.Viewport.Width;
            int viewportHeight = _game.GraphicsDevice.Viewport.Height;
            
            // Draw full-screen black background with some transparency
            spriteBatch.Draw(
                _backgroundTexture,
                new Rectangle(0, 0, viewportWidth, viewportHeight),
                null,
                new Color(0, 0, 0, 200),
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                0f);
            
            // Draw "GAME OVER" text
            string gameOverText = "GAME OVER";
            float gameOverScale = _textScale;
            
            float totalWidth = _textRenderer.GetTextWidth(gameOverText, gameOverScale);
            
            Vector2 textPosition = new Vector2(
                viewportWidth / 2 - totalWidth / 2,
                viewportHeight / 3 - (32 * gameOverScale) / 2);
            
            _textRenderer.DrawText(spriteBatch, gameOverText, textPosition, Color.White, gameOverScale);
            
            // Draw pulsating "PRESS SPACE TO RESTART" text
            string restartText = "PRESS SPACE TO RESTART";
            float pulsate = (float)(1.0 + 0.2 * Math.Sin(_elapsedTime * _pulsateSpeed));
            float restartScale = 1.5f * pulsate;
            
            float restartWidth = _textRenderer.GetTextWidth(restartText, restartScale);
            
            Vector2 restartPosition = new Vector2(
                viewportWidth / 2 - restartWidth / 2,
                viewportHeight * 2/3);
            
            _textRenderer.DrawText(spriteBatch, restartText, restartPosition, Color.White, restartScale);
        }
    }
}