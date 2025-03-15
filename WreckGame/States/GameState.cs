using Microsoft.Xna.Framework;

namespace WreckGame.States
{
    public abstract class GameState
    {
        public static bool EditMode { get; set; } = false;
        
        protected Game1 Game { get; }

        public GameState(Game1 game) { Game = game; }
        public virtual void Update(GameTime gameTime) { }
        public virtual void Draw(GameTime gameTime) { }
    }
}