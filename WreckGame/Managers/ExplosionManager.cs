using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace WreckGame.Managers
{
    public class ExplosionManager
    {
        private readonly List<Explosion> _explosions = new List<Explosion>();
        private readonly Texture2D[] _explosionTextures;

        public ExplosionManager(ContentManager content)
        {
            _explosionTextures = new Texture2D[3];
            for (int i = 0; i < 3; i++)
            {
                _explosionTextures[i] = content.Load<Texture2D>($"particles/explosion{i + 1}");
            }
        }

        public void AddExplosion(Vector2 position, float gameScale)
        {
            _explosions.Add(new Explosion(position * gameScale));
        }

        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            for (int i = _explosions.Count - 1; i >= 0; i--)
            {
                _explosions[i].Update(delta);
                if (_explosions[i].IsFinished)
                {
                    _explosions.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Matrix viewMatrix, float gameScale)
        {
            foreach (var explosion in _explosions)
            {
                explosion.Draw(spriteBatch, _explosionTextures, gameScale);
            }
        }
    }

    public class Explosion
    {
        private Vector2 _position;
        private float _timer;
        private int _currentFrame;
        private const float FRAME_TIME = 0.1f;
        private const int FRAME_COUNT = 3;

        public bool IsFinished => _currentFrame >= FRAME_COUNT;

        public Explosion(Vector2 position)
        {
            _position = position;
            _currentFrame = 0;
        }

        public void Update(float delta)
        {
            _timer += delta;
            if (_timer >= FRAME_TIME)
            {
                _timer = 0f;
                _currentFrame++;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D[] textures, float gameScale)
        {
            if (_currentFrame < FRAME_COUNT)
            {
                Texture2D texture = textures[_currentFrame];
                spriteBatch.Draw(texture, _position / gameScale, null, Color.White, 0f, new Vector2(texture.Width / 2, texture.Height / 2), 1.5f, SpriteEffects.None, 0);
            }
        }
    }
}