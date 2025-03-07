using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WreckGame.Graphics
{
    public class Animation
    {
        private Texture2D[] _frames;
        private int _currentFrame;
        private float _frameTime;
        private float _frameTimer;
        private bool _isActive;
        private Vector2 _position;
        private float _scale;
        
        public bool IsActive => _isActive;
        public bool IsComplete => !_isActive && _currentFrame >= _frames.Length;
        
        public Animation(Texture2D[] frames, float frameTime, float scale)
        {
            _frames = frames;
            _frameTime = frameTime;
            _scale = scale;
            _isActive = false;
            _currentFrame = 0;
            _frameTimer = 0f;
        }
        
        public void Start(Vector2 position)
        {
            _position = position;
            _isActive = true;
            _currentFrame = 0;
            _frameTimer = 0f;
        }
        
        public void Update(float deltaTime)
        {
            if (!_isActive) return;
            
            _frameTimer += deltaTime;
            if (_frameTimer >= _frameTime)
            {
                _frameTimer = 0f;
                _currentFrame++;
                
                if (_currentFrame >= _frames.Length)
                {
                    _isActive = false;
                }
            }
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_isActive || _currentFrame >= _frames.Length) return;
            
            Texture2D currentTexture = _frames[_currentFrame];
            
            spriteBatch.Draw(
                currentTexture,
                _position,
                null,
                Color.White,
                0f,
                new Vector2(currentTexture.Width / 2, currentTexture.Height / 2),
                _scale,
                SpriteEffects.None,
                0);
        }
    }
}