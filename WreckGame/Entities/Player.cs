using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WreckGame.Input;

namespace WreckGame.Entities
{
    public class Player : Entity
    {
        private Vector2 _velocity;
        private float _acceleration = 1500f;
        private float _maxSpeed = 500f;
        private float _friction = 3f;
        private float _hoverSpeed = 5f;
        
        public Player(Texture2D texture, Vector2 worldPosition, float gameScale)
            : base(texture, worldPosition, gameScale)
        {
            _velocity = Vector2.Zero;
        }
        
        public override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Hover animation
            _hoverOffset = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * _hoverSpeed) * 15f;
            
            base.Update(gameTime);
        }
        
        public void HandleInput(InputManager inputManager, float delta)
        {
            Vector2 direction = inputManager.GetMovementDirection();
            
            if (direction != Vector2.Zero)
            {
                _velocity += direction * _acceleration * delta;
            }
            
            if (direction == Vector2.Zero || _velocity.Length() > _maxSpeed)
            {
                _velocity -= _velocity * _friction * delta;
            }
            
            if (_velocity.Length() > _maxSpeed)
            {
                _velocity.Normalize();
                _velocity *= _maxSpeed;
            }
            
            _worldPosition += _velocity * delta;
        }
        
        public void ConstrainToMap(int mapWidth, int mapHeight, float bufferFromWall)
        {
            // Calculate map boundaries in world coordinates
            int tileSize = 32;
            int mapWidthPixels = (mapWidth + 2) * tileSize;
            int mapHeightPixels = (mapHeight + 2) * tileSize;
            float startX = -mapWidthPixels / 2;
            float startY = -mapHeightPixels / 2;
            
            float minX = startX + tileSize + bufferFromWall;
            float minY = startY + tileSize + bufferFromWall;
            float maxX = startX + ((mapWidth + 2) * tileSize) - tileSize - bufferFromWall;
            float maxY = startY + ((mapHeight + 2) * tileSize) - tileSize - bufferFromWall;
            
            _worldPosition.X = MathHelper.Clamp(_worldPosition.X, minX, maxX);
            _worldPosition.Y = MathHelper.Clamp(_worldPosition.Y, minY, maxY);
        }
        
        public void Reset(Vector2 position)
        {
            _worldPosition = position;
            _screenPosition = _worldPosition * _gameScale;
            _velocity = Vector2.Zero;
        }
    }
}