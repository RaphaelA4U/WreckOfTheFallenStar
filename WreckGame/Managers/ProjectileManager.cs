using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace WreckGame.Managers
{
    public class ProjectileManager
    {
        private readonly List<Projectile> _projectiles = new List<Projectile>();
        private readonly Texture2D _projectileTexture;
        private const int MAX_BULLETS = 50;
        private const float BULLET_SPEED = 400f;
        private const float BULLET_MAX_LIFETIME = 2.0f;

        public ProjectileManager(ContentManager content, GraphicsManager graphicsManager)
        {
            _projectileTexture = graphicsManager.CreateTexture(1, 1, Color.White);

            for (int i = 0; i < MAX_BULLETS; i++)
            {
                _projectiles.Add(new Projectile(Vector2.Zero, Vector2.Zero, false, _projectileTexture, BULLET_SPEED));
            }
        }

        public void AddProjectile(Vector2 position, Vector2 direction, bool isEnemyBullet)
        {
            foreach (var projectile in _projectiles)
            {
                if (!projectile.Active)
                {
                    projectile.Position = position;
                    projectile.Direction = direction;
                    projectile.Active = true;
                    projectile.LifeTime = 0f;
                    projectile.IsEnemyBullet = isEnemyBullet;
                    projectile.Hitbox = new Rectangle((int)position.X - 2, (int)position.Y - 2, 4, 4);
                    break;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            foreach (var projectile in _projectiles)
            {
                if (projectile.Active)
                {
                    projectile.Update(delta);
                    if (projectile.LifeTime >= BULLET_MAX_LIFETIME ||
                        projectile.Position.X < Map.Map.TILE_SIZE || projectile.Position.X > Map.Map.MAP_WIDTH_TILES * Map.Map.TILE_SIZE ||
                        projectile.Position.Y < Map.Map.TILE_SIZE || projectile.Position.Y > Map.Map.MAP_HEIGHT_TILES * Map.Map.TILE_SIZE)
                    {
                        projectile.Active = false;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Matrix viewMatrix)
        {
            foreach (var projectile in _projectiles)
            {
                if (projectile.Active)
                {
                    projectile.Draw(spriteBatch);
                }
            }
        }

        public List<Projectile> GetProjectiles() => _projectiles;
    }

    public class Projectile
    {
        public Vector2 Position { get; set; }
        public Vector2 Direction { get; set; }
        public float Speed { get; }
        public bool Active { get; set; }
        public Rectangle Hitbox { get; set; }
        public float LifeTime { get; set; }
        public bool IsEnemyBullet { get; set; }
        private readonly Texture2D _texture;

        public Projectile(Vector2 position, Vector2 direction, bool isEnemyBullet, Texture2D texture, float speed)
        {
            Position = position;
            Direction = direction;
            IsEnemyBullet = isEnemyBullet;
            _texture = texture;
            Speed = speed;
            Active = false;
        }

        public void Update(float delta)
        {
            LifeTime += delta;
            Position += Direction * Speed * delta;
            Hitbox = new Rectangle((int)Position.X - 2, (int)Position.Y - 2, 4, 4);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Color color = Color.Red;
            spriteBatch.Draw(_texture, new Rectangle((int)Position.X - 2, (int)Position.Y - 2, 4, 4), color);
        }
    }
}