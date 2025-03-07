using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WreckGame.Entities
{
    public abstract class Entity
    {
        protected Texture2D _texture;
        protected Vector2 _worldPosition;
        protected Vector2 _screenPosition;
        protected float _gameScale;
        protected float _hoverOffset;
        
        public Vector2 WorldPosition => _worldPosition;
        public Vector2 ScreenPosition => _screenPosition;
        
        public Entity(Texture2D texture, Vector2 worldPosition, float gameScale)
        {
            _texture = texture;
            _worldPosition = worldPosition;
            _gameScale = gameScale;
            _screenPosition = worldPosition * gameScale;
            _hoverOffset = 0f;
        }
        
        public virtual void Update(GameTime gameTime)
        {
            _screenPosition = _worldPosition * _gameScale;
        }
        
        public virtual void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            spriteBatch.Draw(
                _texture,
                _worldPosition + new Vector2(0, _hoverOffset), // Use world position directly with camera transform
                null,
                Color.White,
                0f,
                Vector2.Zero,
                _gameScale,
                SpriteEffects.None,
                0f);
        }
        
        public Rectangle GetBoundingBox()
        {
            return new Rectangle(
                (int)_screenPosition.X,
                (int)(_screenPosition.Y + _hoverOffset) + (int)(2.5f * _gameScale),
                (int)(32 * _gameScale),
                (int)((32 - 6) * _gameScale)
            );
        }
        
        public bool Collides(Entity other)
        {
            return GetBoundingBox().Intersects(other.GetBoundingBox());
        }
    }
}