using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WreckGame.Input;

namespace WreckGame.GameStates
{
    public abstract class GameState
    {
        protected Game1 _game;
        protected ContentManager _content;
        protected InputManager _inputManager;
        
        public GameState(Game1 game, InputManager inputManager)
        {
            _game = game;
            _content = game.Content;
            _inputManager = inputManager;
        }
        
        public virtual void Initialize() { }
        
        public virtual void LoadContent() { }
        
        public virtual void UnloadContent() { }
        
        public abstract void Update(GameTime gameTime);
        
        public abstract void Draw(SpriteBatch spriteBatch);
        
        public virtual void OnStateEnter() { }
        
        public virtual void OnStateExit() { }
    }
}