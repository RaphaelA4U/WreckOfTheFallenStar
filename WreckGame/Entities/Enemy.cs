using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WreckGame.Managers;
using WreckGame.States;
using System;

namespace WreckGame.Entities
{
    public class Enemy : Entity
    {
        private readonly GraphicsManager _graphicsManager;
        private readonly MainGameState _gameState;
        public int Direction { get; set; }
        public float Speed { get; set; }
        public int HP { get; set; }
        public float DamageFlashTimer { get; set; }
        public Vector2 Velocity { get; set; }
        public float DamageCooldownTimer { get; set; }
        public bool CanShoot { get; set; }
        public float ShootCooldown { get; set; }
        public float ShootTimer { get; set; }
        private readonly int _enemyIndex;
        private const float DAMAGE_FLASH_DURATION = 0.2f;

        public Enemy(GraphicsManager graphicsManager, MainGameState gameState, Vector2 position, int direction, float speed, int hp, bool canShoot, int enemyIndex)
        {
            _graphicsManager = graphicsManager;
            _gameState = gameState;
            _enemyIndex = enemyIndex;
            Texture = _graphicsManager.LoadTexture(GetTextureName(enemyIndex));
            WorldPosition = position;
            Position = WorldPosition * _gameState.GameScale;
            Direction = direction;
            Speed = speed;
            HP = hp;
            CanShoot = canShoot;
            ShootCooldown = 0.5f;
            ShootTimer = 0.25f;
        }

        public override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            double time = gameTime.TotalGameTime.TotalSeconds;
            float frequency = 2.5f + (_enemyIndex * 0.5f);
            float amplitude = 10f - (_enemyIndex % 2) * 3f;
            HoverOffset = (float)Math.Sin(time * frequency) * amplitude;

            if (DamageFlashTimer > 0) DamageFlashTimer -= delta;
            if (DamageCooldownTimer > -1) DamageCooldownTimer -= delta;

            if (Velocity != Vector2.Zero)
            {
                WorldPosition += Velocity * delta;
                Velocity *= 1 - 3f * delta;
                if (Velocity.LengthSquared() < 1f) Velocity = Vector2.Zero;
                WorldPosition = ClampToMap(WorldPosition);
            }
            else if (!GameState.EditMode)
            {
                switch (_enemyIndex)
                {
                    case 0: // Moves horizontally
                        WorldPosition = new Vector2(WorldPosition.X + Speed * Direction * delta, WorldPosition.Y);
                        if (WorldPosition.X <= Map.Map.TILE_SIZE) Direction = 1;
                        else if (WorldPosition.X >= Map.Map.MAP_WIDTH_TILES * Map.Map.TILE_SIZE) Direction = -1;
                        break;
                    case 1: // Moves vertically
                        WorldPosition = new Vector2(WorldPosition.X, WorldPosition.Y + Speed * Direction * delta);
                        if (WorldPosition.Y <= Map.Map.TILE_SIZE) Direction = 1;
                        else if (WorldPosition.Y >= Map.Map.MAP_HEIGHT_TILES * Map.Map.TILE_SIZE) Direction = -1;
                        break;
                    case 2: // Follows player
                        Vector2 toPlayer = _gameState.Player.WorldPosition - WorldPosition;
                        if (toPlayer.Length() > 16)
                        {
                            toPlayer.Normalize();
                            WorldPosition += toPlayer * Speed * delta;
                            WorldPosition = ClampToMap(WorldPosition);
                        }
                        break;
                    case 3: // Moves horizontally
                        WorldPosition = new Vector2(WorldPosition.X + Speed * Direction * delta, WorldPosition.Y);
                        if (WorldPosition.X <= Map.Map.TILE_SIZE) Direction = 1;
                        else if (WorldPosition.X >= Map.Map.MAP_WIDTH_TILES * Map.Map.TILE_SIZE) Direction = -1;
                        break;
                    case 4: // Moves vertically
                        WorldPosition = new Vector2(WorldPosition.X, WorldPosition.Y + Speed * Direction * delta);
                        if (WorldPosition.Y <= Map.Map.TILE_SIZE) Direction = 1;
                        else if (WorldPosition.Y >= Map.Map.MAP_HEIGHT_TILES * Map.Map.TILE_SIZE) Direction = -1;
                        break;
                    case 5: // Follows player
                        toPlayer = _gameState.Player.WorldPosition - WorldPosition;
                        if (toPlayer.Length() > 16)
                        {
                            toPlayer.Normalize();
                            WorldPosition += toPlayer * Speed * delta;
                            WorldPosition = ClampToMap(WorldPosition);
                        }
                        break;
                }
            }

            Position = WorldPosition * _gameState.GameScale;
            Hitbox = new Rectangle((int)WorldPosition.X, (int)(WorldPosition.Y + HoverOffset), Texture.Width, Texture.Height);

            if (CanShoot && !GameState.EditMode)
            {
                ShootTimer -= delta;
                if (ShootTimer <= 0)
                {
                    ShootTimer = ShootCooldown;
                    Vector2 shootDirection = _enemyIndex switch
                    {
                        3 => new Vector2(Direction, 0),
                        4 => new Vector2(0, Direction),
                        _ => Vector2.Normalize(_gameState.Player.WorldPosition - WorldPosition)
                    };
                    Vector2 bulletPos = new Vector2(WorldPosition.X + Texture.Width / 2, WorldPosition.Y + HoverOffset + Texture.Height / 2);
                    _gameState.ProjectileManager.AddProjectile(bulletPos, shootDirection, true);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Color color = DamageFlashTimer > 0 ? Color.Red : Color.White;
            SpriteEffects effect = SpriteEffects.None;
            if ((_enemyIndex == 0 || _enemyIndex == 3) && Direction == -1) effect = SpriteEffects.FlipHorizontally;
            else if (_enemyIndex == 2 || _enemyIndex == 5)
            {
                Vector2 toPlayer = _gameState.Player.WorldPosition - WorldPosition;
                if (toPlayer.X < 0) effect = SpriteEffects.FlipHorizontally;
            }
            spriteBatch.Draw(Texture, new Vector2(WorldPosition.X, WorldPosition.Y + HoverOffset), null, color, 0f, Vector2.Zero, 1.0f, effect, 0f);
        }

        private Vector2 ClampToMap(Vector2 position)
        {
            float minX = Map.Map.TILE_SIZE;
            float minY = Map.Map.TILE_SIZE;
            float maxX = Map.Map.MAP_WIDTH_TILES * Map.Map.TILE_SIZE;
            float maxY = Map.Map.MAP_HEIGHT_TILES * Map.Map.TILE_SIZE;
            position.X = MathHelper.Clamp(position.X, minX, maxX);
            position.Y = MathHelper.Clamp(position.Y, minY, maxY);
            return position;
        }

        private string GetTextureName(int index)
        {
            return index switch
            {
                0 => "entities/drone_enemy",
                1 => "entities/drone_enemy1",
                2 => "entities/drone_enemy2",
                3 => "entities/drone_enemy3",
                4 => "entities/drone_enemy4",
                5 => "entities/drone_enemy5",
                _ => "entities/drone_enemy"
            };
        }
    }
}