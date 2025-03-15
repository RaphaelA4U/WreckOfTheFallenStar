using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WreckGame.Entities
{
    public abstract class Entity
    {
        public Vector2 Position { get; set; }
        public Vector2 WorldPosition { get; set; }
        public float HoverOffset { get; set; }
        public Rectangle Hitbox { get; set; }
        public Texture2D Texture { get; set; }
        public bool Active { get; set; } = true;

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);
    }
}