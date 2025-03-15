using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WreckGame.Managers;
using WreckGame.States;
using System;

namespace WreckGame.Entities
{
    public class Player : Entity
    {
        private readonly InputManager _inputManager;
        private readonly GraphicsManager _graphicsManager;
        private readonly MainGameState _gameState;
        private Vector2 _velocity;

        public int HP { get; set; } = 100;
        public int Charge { get; set; } = 100;
        public float ChargeTimer { get; set; } = 0f;
        public float DamageCooldownTimer { get; set; } = -1f;

        public MainGameState GameState => _gameState; // Added for access to game state from other classes

        public Player(InputManager inputManager, GraphicsManager graphicsManager, MainGameState gameState)
        {
            _inputManager = inputManager;
            _graphicsManager = graphicsManager;
            _gameState = gameState;
            Texture = _graphicsManager.LoadTexture("entities/drone");
            WorldPosition = new Vector2(Map.Map.MAP_WIDTH_TILES * Map.Map.TILE_SIZE / 2, Map.Map.MAP_HEIGHT_TILES * Map.Map.TILE_SIZE / 2);
            Position = WorldPosition * _gameState.GameScale;
        }

        public override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            double time = gameTime.TotalGameTime.TotalSeconds;
            HoverOffset = (float)(Math.Sin(time * 5) * 15f);

            // Movement
            float acceleration = 1500f;
            float maxSpeed = 500f;
            float friction = 3f;
            Vector2 direction = Vector2.Zero;
            if (_inputManager.IsKeyDown(Keys.A)) direction.X -= 1;
            if (_inputManager.IsKeyDown(Keys.D)) direction.X += 1;
            if (_inputManager.IsKeyDown(Keys.W)) direction.Y -= 1;
            if (_inputManager.IsKeyDown(Keys.S)) direction.Y += 1;

            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                _velocity += direction * acceleration * delta;
            }
            if (direction == Vector2.Zero || _velocity.Length() > maxSpeed)
            {
                _velocity -= _velocity * friction * delta;
            }
            if (_velocity.Length() > maxSpeed)
            {
                _velocity = Vector2.Normalize(_velocity) * maxSpeed;
            }

            WorldPosition += _velocity * delta;
            WorldPosition = ClampToMap(WorldPosition);
            Position = WorldPosition * _gameState.GameScale;
            Hitbox = new Rectangle((int)WorldPosition.X, (int)(WorldPosition.Y + HoverOffset), Texture.Width, Texture.Height);

            // Charge management
            if (!MainGameState.EditMode)
            {
                ChargeTimer += delta;
                if (ChargeTimer >= 1.0f)
                {
                    Charge--;
                    ChargeTimer -= 1.0f;
                    if (Charge <= 0)
                    {
                        _gameState.PlayerDied(DeathReason.EnergyDepleted);
                    }
                }
            }

            if (DamageCooldownTimer > -1)
            {
                DamageCooldownTimer -= delta;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Color color = _gameState.DamageFlashTimer > 0 ? Color.Red : Color.White;
            spriteBatch.Draw(Texture, new Vector2(WorldPosition.X, WorldPosition.Y + HoverOffset), null, color, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0f);
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

        public Vector2 Velocity
        {
            get => _velocity;
            set => _velocity = value;
        }
    }
}