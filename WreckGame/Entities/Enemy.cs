using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WreckGame.Entities
{
    public class Enemy : Entity
    {
        private float _hoverSpeed = 2.5f;
        
        public Enemy(Texture2D texture, Vector2 worldPosition, float gameScale)
            : base(texture, worldPosition, gameScale)
        {
        }
        
        public override void Update(GameTime gameTime)
        {
            _hoverOffset = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * _hoverSpeed) * 10f;
            base.Update(gameTime);
        }
        
        public void Reset(Vector2 position)
        {
            _worldPosition = position;
            _screenPosition = _worldPosition * _gameScale;
        }
    }
}