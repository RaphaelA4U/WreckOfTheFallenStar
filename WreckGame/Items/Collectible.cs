using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WreckGame.Managers;
using WreckGame.Entities;

namespace WreckGame.Items
{
    public abstract class Collectible : Entity
    {
        protected readonly GraphicsManager _graphicsManager;

        public Collectible(GraphicsManager graphicsManager, string textureName, Vector2 position)
        {
            _graphicsManager = graphicsManager;
            Texture = _graphicsManager.LoadTexture(textureName);
            WorldPosition = position;
            Position = WorldPosition; // Wordt later geschaald in MainGameState
        }

        public override void Update(GameTime gameTime)
        {
            double time = gameTime.TotalGameTime.TotalSeconds;
            HoverOffset = GetHoverOffset(time);
            Hitbox = new Rectangle((int)WorldPosition.X, (int)(WorldPosition.Y + HoverOffset), (int)(Texture.Width * 0.7f), (int)(Texture.Height * 0.7f));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, new Vector2(WorldPosition.X, WorldPosition.Y + HoverOffset), null, Color.White, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
        }

        public abstract void OnCollect(Player player);
        protected abstract float GetHoverOffset(double time);
    }
}