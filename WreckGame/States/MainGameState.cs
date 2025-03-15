using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WreckGame.Entities;
using WreckGame.Items;
using WreckGame.Managers;
using WreckGame.Map;
using WreckGame.Utilities;
using System;
using System.Collections.Generic;

namespace WreckGame.States
{
    public class MainGameState : GameState
    {
        private readonly InputManager _inputManager;
        private readonly GraphicsManager _graphicsManager;
        public Player Player { get; private set; }
        public Enemy[] Enemies { get; private set; }
        public Collectible[] Collectibles { get; private set; }
        public ProjectileManager ProjectileManager { get; private set; }
        private readonly Map.Map _map;
        private readonly ExplosionManager _explosionManager;
        private UI.Button _button;
        private readonly Random _random;

        public float GameScale { get; set; } = 2.0f;
        public float DamageFlashTimer { get; set; } = 0f;
        private const float DAMAGE_FLASH_DURATION = 0.3f;
        private const float SHOOT_COOLDOWN = 0.3f;
        private float _shootCooldownTimer = 0f;
        private const int BULLET_ENERGY_COST = 1;
        private const int BULLET_DAMAGE = 10;
        private bool _showButtonNotification;
        private float _buttonNotificationTimer;
        private const float BUTTON_NOTIFICATION_DURATION = 3.0f;

        private Entity _draggedEntity = null;
        private Vector2 _dragOffset = Vector2.Zero;

        public MainGameState(Game1 game, InputManager inputManager, GraphicsManager graphicsManager) : base(game)
        {
            _inputManager = inputManager;
            _graphicsManager = graphicsManager;
            _map = new Map.Map(_graphicsManager);
            _explosionManager = new ExplosionManager(game.Content);
            ProjectileManager = new ProjectileManager(game.Content, _graphicsManager);
            _random = new Random();
            ResetGame();
        }

        private void ResetGame()
        {
            GameScale = 2.0f;
            Player = new Player(_inputManager, _graphicsManager, this);
            Enemies = new Enemy[6];
            Enemies[0] = new Enemy(_graphicsManager, this, new Vector2(Player.WorldPosition.X, Player.WorldPosition.Y - 160), 1, 80f, 100, false, 0);
            Enemies[1] = new Enemy(_graphicsManager, this, new Vector2(Player.WorldPosition.X + 160, Player.WorldPosition.Y), 1, 60f, 100, false, 1);
            Enemies[2] = new Enemy(_graphicsManager, this, new Vector2(Player.WorldPosition.X - 160, Player.WorldPosition.Y), 1, 50f, 100, false, 2);
            Enemies[3] = new Enemy(_graphicsManager, this, new Vector2(Player.WorldPosition.X + 250, Player.WorldPosition.Y - 100), 1, 70f, 100, true, 3);
            Enemies[4] = new Enemy(_graphicsManager, this, new Vector2(Player.WorldPosition.X - 250, Player.WorldPosition.Y - 100), 1, 55f, 100, true, 4);
            Enemies[5] = new Enemy(_graphicsManager, this, new Vector2(Player.WorldPosition.X + 200, Player.WorldPosition.Y + 200), 1, 40f, 120, true, 5);

            InitializeCollectibles();
            _button = new UI.Button(_graphicsManager, new Vector2(_random.Next(Map.Map.TILE_SIZE, Map.Map.MAP_WIDTH_TILES * Map.Map.TILE_SIZE), _random.Next(Map.Map.TILE_SIZE, Map.Map.MAP_HEIGHT_TILES * Map.Map.TILE_SIZE)));
        }

        private void InitializeCollectibles()
        {
            Collectibles = new Collectible[5 + 3 + 3]; // Max shards + parts + charges
            int shardCount = _random.Next(2, 6);
            int partCount = _random.Next(1, 4);
            int chargeCount = _random.Next(1, 4);
            float minX = Map.Map.TILE_SIZE;
            float minY = Map.Map.TILE_SIZE;
            float maxX = Map.Map.MAP_WIDTH_TILES * Map.Map.TILE_SIZE;
            float maxY = Map.Map.MAP_HEIGHT_TILES * Map.Map.TILE_SIZE;

            for (int i = 0; i < 5; i++)
            {
                Collectibles[i] = new DataShard(_graphicsManager, new Vector2(_random.Next((int)minX, (int)maxX), _random.Next((int)minY, (int)maxY)), i) { Active = i < shardCount };
            }
            for (int i = 0; i < 3; i++)
            {
                Collectibles[5 + i] = new RepairPart(_graphicsManager, new Vector2(_random.Next((int)minX, (int)maxX), _random.Next((int)minY, (int)maxY)), i) { Active = i < partCount };
            }
            for (int i = 0; i < 3; i++)
            {
                Collectibles[8 + i] = new ChargeItem(_graphicsManager, new Vector2(_random.Next((int)minX, (int)maxX), _random.Next((int)minY, (int)maxY)), i) { Active = i < chargeCount };
            }
        }

        public override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_inputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                Game.SetState(new PauseScreenState(Game, _inputManager, _graphicsManager, this));
                return;
            }

            HandleDebugInput();
            if (EditMode) HandleEditModeInput();

            Player.Update(gameTime);
            foreach (var enemy in Enemies) if (enemy.Active) enemy.Update(gameTime);
            foreach (var collectible in Collectibles) if (collectible.Active) collectible.Update(gameTime);
            _button.Update(gameTime, Player, _inputManager);
            ProjectileManager.Update(gameTime);
            _explosionManager.Update(gameTime);

            if (!EditMode)
                {
                    UpdateCollisions();
                }            
            
            HandlePlayerShooting(gameTime);

            if (DamageFlashTimer > 0) DamageFlashTimer -= delta;
            if (_shootCooldownTimer > 0) _shootCooldownTimer -= delta;
            if (_showButtonNotification)
            {
                _buttonNotificationTimer += delta;
                if (_buttonNotificationTimer >= BUTTON_NOTIFICATION_DURATION) _showButtonNotification = false;
            }
        }

        private void HandleDebugInput()
        {
            if (_inputManager.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) && _inputManager.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) && _inputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.C))
            {
                EditMode = !EditMode;
                GameState.EditMode = EditMode;
            }
            if (_inputManager.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) && _inputManager.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) && _inputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.K))
            {
                PlayerDied(DeathReason.DebugKill);
            }
            int scrollChange = _inputManager.GetScrollWheelValue() - _inputManager.GetPreviousScrollWheelValue();
            if (scrollChange != 0)
            {
                GameScale += scrollChange * 0.001f;
                GameScale = MathHelper.Clamp(GameScale, 0.5f, 5.0f);
            }
        }

        private void HandleEditModeInput()
        {
            Vector2 worldPosition = Vector2.Transform(_inputManager.GetMousePosition().ToVector2(), Matrix.Invert(GetViewMatrix()));
            
            // Remove items with right click
            if (_inputManager.IsRightMousePressed())
            {
                if (_button.Hitbox.Contains((int)worldPosition.X, (int)worldPosition.Y)) _button.Active = false;
                foreach (var enemy in Enemies) if (enemy.Active && enemy.Hitbox.Contains((int)worldPosition.X, (int)worldPosition.Y)) enemy.Active = false;
                foreach (var collectible in Collectibles) if (collectible.Active && collectible.Hitbox.Contains((int)worldPosition.X, (int)worldPosition.Y)) collectible.Active = false;
            }
            
            // Start dragging with left mouse press
            if (_inputManager.IsLeftMousePressed() && _draggedEntity == null)
            {
                // Check button first
                if (_button.Active && _button.Hitbox.Contains((int)worldPosition.X, (int)worldPosition.Y))
                {
                    _draggedEntity = _button;
                    _dragOffset = _button.WorldPosition - worldPosition;
                    return;
                }
                
                // Check enemies
                foreach (var enemy in Enemies)
                {
                    if (enemy.Active && enemy.Hitbox.Contains((int)worldPosition.X, (int)worldPosition.Y))
                    {
                        _draggedEntity = enemy;
                        _dragOffset = enemy.WorldPosition - worldPosition;
                        return;
                    }
                }
                
                // Check collectibles
                foreach (var collectible in Collectibles)
                {
                    if (collectible.Active && collectible.Hitbox.Contains((int)worldPosition.X, (int)worldPosition.Y))
                    {
                        _draggedEntity = collectible;
                        _dragOffset = collectible.WorldPosition - worldPosition;
                        return;
                    }
                }
            }
            
            // Update position while dragging
            if (_draggedEntity != null)
            {
                if (_inputManager.IsLeftMouseDown())
                {
                    // Update the entity position
                    _draggedEntity.WorldPosition = worldPosition + _dragOffset;
                    
                    // Make sure the entity stays within the map boundaries
                    float minX = Map.Map.TILE_SIZE;
                    float minY = Map.Map.TILE_SIZE;
                    float maxX = Map.Map.MAP_WIDTH_TILES * Map.Map.TILE_SIZE;
                    float maxY = Map.Map.MAP_HEIGHT_TILES * Map.Map.TILE_SIZE;
                    
                    _draggedEntity.WorldPosition = new Vector2(
                        MathHelper.Clamp(_draggedEntity.WorldPosition.X, minX, maxX),
                        MathHelper.Clamp(_draggedEntity.WorldPosition.Y, minY, maxY)
                    );
                }
                else
                {
                    // Release the entity when mouse button is released
                    _draggedEntity = null;
                }
            }
        }

        private void UpdateCollisions()
        {
            foreach (var enemy in Enemies)
            {
                if (enemy.Active && Player.Hitbox.Intersects(enemy.Hitbox) && Player.DamageCooldownTimer < 0 && enemy.DamageCooldownTimer < 0)
                {
                    Player.HP -= 25;
                    Player.DamageCooldownTimer = 1.0f;
                    DamageFlashTimer = DAMAGE_FLASH_DURATION;
                    enemy.HP -= 25;
                    enemy.DamageCooldownTimer = 1.0f;
                    enemy.DamageFlashTimer = DAMAGE_FLASH_DURATION;

                    Vector2 collisionDirection = Player.WorldPosition - enemy.WorldPosition;
                    if (collisionDirection != Vector2.Zero)
                    {
                        collisionDirection.Normalize();
                        Player.Velocity += collisionDirection * 300f;
                        enemy.Velocity = -collisionDirection * 300f;
                    }

                    if (enemy.HP <= 0)
                    {
                        enemy.Active = false;
                        _explosionManager.AddExplosion(enemy.WorldPosition, GameScale);
                    }
                    if (Player.HP <= 0) PlayerDied(DeathReason.EnemyCollision);
                }
            }

            foreach (var collectible in Collectibles)
            {
                if (collectible.Active && Player.Hitbox.Intersects(collectible.Hitbox))
                {
                    collectible.OnCollect(Player);
                    collectible.Active = false;
                }
            }

            var projectiles = ProjectileManager.GetProjectiles();
            foreach (var projectile in projectiles)
            {
                if (projectile.Active)
                {
                    if (projectile.IsEnemyBullet && Player.Hitbox.Intersects(projectile.Hitbox) && Player.DamageCooldownTimer < 0)
                    {
                        Player.HP -= BULLET_DAMAGE;
                        Player.DamageCooldownTimer = 1.0f;
                        DamageFlashTimer = DAMAGE_FLASH_DURATION;
                        if (Player.HP <= 0) PlayerDied(DeathReason.EnemyBullet);
                        projectile.Active = false;
                    }
                    else if (!projectile.IsEnemyBullet)
                    {
                        foreach (var enemy in Enemies)
                        {
                            if (enemy.Active && projectile.Hitbox.Intersects(enemy.Hitbox))
                            {
                                enemy.HP -= BULLET_DAMAGE;
                                enemy.DamageFlashTimer = DAMAGE_FLASH_DURATION;
                                if (enemy.HP <= 0)
                                {
                                    enemy.Active = false;
                                    _explosionManager.AddExplosion(enemy.WorldPosition, GameScale);
                                }
                                projectile.Active = false;
                                break;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < Enemies.Length; i++)
            {
                if (!Enemies[i].Active || Enemies[i].DamageCooldownTimer >= 0)
                    continue;
                    
                for (int j = i + 1; j < Enemies.Length; j++)
                {
                    if (!Enemies[j].Active || Enemies[j].DamageCooldownTimer >= 0)
                        continue;
                        
                    if (Enemies[i].Hitbox.Intersects(Enemies[j].Hitbox))
                    {
                        // Apply damage
                        Enemies[i].HP -= 15;  
                        Enemies[i].DamageCooldownTimer = 1.0f;
                        Enemies[i].DamageFlashTimer = DAMAGE_FLASH_DURATION;
                        Enemies[j].HP -= 15;
                        Enemies[j].DamageCooldownTimer = 1.0f;
                        Enemies[j].DamageFlashTimer = DAMAGE_FLASH_DURATION;
                        
                        // Push enemies away from each other
                        Vector2 collisionDirection = Enemies[i].WorldPosition - Enemies[j].WorldPosition;
                        if (collisionDirection != Vector2.Zero)
                        {
                            collisionDirection.Normalize();
                            Enemies[i].Velocity += collisionDirection * 200f;
                            Enemies[j].Velocity -= collisionDirection * 200f;
                        }
                        
                        // Handle deaths and explosions
                        if (Enemies[i].HP <= 0)
                        {
                            Enemies[i].Active = false;
                            _explosionManager.AddExplosion(Enemies[i].WorldPosition, GameScale);
                        }
                        
                        if (Enemies[j].HP <= 0)
                        {
                            Enemies[j].Active = false;
                            _explosionManager.AddExplosion(Enemies[j].WorldPosition, GameScale);
                        }
                    }
                }
            }
        }

        private void HandlePlayerShooting(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_shootCooldownTimer > 0) _shootCooldownTimer -= delta;

            if (!EditMode && _inputManager.IsLeftMouseDown() && _shootCooldownTimer <= 0 && Player.Charge >= BULLET_ENERGY_COST)
            {
                Vector2 mouseWorldPos = Vector2.Transform(_inputManager.GetMousePosition().ToVector2(), Matrix.Invert(GetViewMatrix()));
                Vector2 direction = mouseWorldPos - Player.WorldPosition;
                if (direction != Vector2.Zero)
                {
                    direction.Normalize();
                    Vector2 bulletPos = new Vector2(Player.WorldPosition.X + Player.Texture.Width / 2, Player.WorldPosition.Y + Player.HoverOffset + Player.Texture.Height / 2);
                    ProjectileManager.AddProjectile(bulletPos, direction, false);
                    _shootCooldownTimer = SHOOT_COOLDOWN;
                    Player.Charge -= BULLET_ENERGY_COST;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _graphicsManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, GetViewMatrix());
            _map.Draw(_graphicsManager.SpriteBatch);
            Player.Draw(_graphicsManager.SpriteBatch);
            foreach (var enemy in Enemies) if (enemy.Active) enemy.Draw(_graphicsManager.SpriteBatch);
            foreach (var collectible in Collectibles) if (collectible.Active) collectible.Draw(_graphicsManager.SpriteBatch);
            ProjectileManager.Draw(_graphicsManager.SpriteBatch, GetViewMatrix());
            _explosionManager.Draw(_graphicsManager.SpriteBatch, GetViewMatrix(), GameScale);
            _button.Draw(_graphicsManager.SpriteBatch);
            if (EditMode)
            {
                Utilities.Utilities.DrawRectangleOutline(_graphicsManager.SpriteBatch, Player.Hitbox, Color.White);
                foreach (var enemy in Enemies) if (enemy.Active) Utilities.Utilities.DrawRectangleOutline(_graphicsManager.SpriteBatch, enemy.Hitbox, Color.White);
                foreach (var collectible in Collectibles) if (collectible.Active) Utilities.Utilities.DrawRectangleOutline(_graphicsManager.SpriteBatch, collectible.Hitbox, Color.Green);
            }
            _graphicsManager.SpriteBatch.End();

            _graphicsManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            DrawUI();
            if (DamageFlashTimer > 0)
            {
                float alpha = DamageFlashTimer / DAMAGE_FLASH_DURATION;
                Color outerColor = new Color(1f, 0f, 0f, alpha * 0.6f);
                Utilities.Utilities.DrawRectangleOutline(_graphicsManager.SpriteBatch, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), outerColor, 80);
            }
            if (_showButtonNotification)
            {
                Vector2 textSize = Utilities.Utilities.MeasureText("BUTTON PRESSED", 2f, 8f);
                Utilities.Utilities.DrawColoredText(_graphicsManager.SpriteBatch, "BUTTON PRESSED", new Vector2((Game.GraphicsDevice.Viewport.Width - textSize.X) / 2, (Game.GraphicsDevice.Viewport.Height - textSize.Y) / 2), Color.Green, Color.Transparent, 2f, false, 8f);
            }
            _graphicsManager.SpriteBatch.End();
        }

        private Matrix GetViewMatrix()
        {
            return Matrix.CreateScale(GameScale) *
                   Matrix.CreateTranslation(
                       -Player.WorldPosition.X * GameScale + Game.GraphicsDevice.Viewport.Width / 2,
                       -Player.WorldPosition.Y * GameScale + Game.GraphicsDevice.Viewport.Height / 2,
                       0);
        }

        private void DrawUI()
        {
            float barWidth = 200;
            float barHeight = 20;
            float margin = 10;
            float padding = 2;
            int screenX = (int)margin;
            int screenY = (int)(Game.GraphicsDevice.Viewport.Height - margin - (barHeight * 2 + padding));

            _graphicsManager.SpriteBatch.Draw(_graphicsManager.LoadTexture("misc/pixel"), new Rectangle(screenX, screenY, (int)barWidth, (int)barHeight), Color.DarkRed);
            _graphicsManager.SpriteBatch.Draw(_graphicsManager.LoadTexture("misc/pixel"), new Rectangle(screenX, screenY, (int)(barWidth * Player.HP / 100), (int)barHeight), Color.Red);
            _graphicsManager.SpriteBatch.Draw(_graphicsManager.LoadTexture("misc/pixel"), new Rectangle(screenX, screenY + (int)(barHeight + padding), (int)barWidth, (int)barHeight), Color.DarkGreen);
            _graphicsManager.SpriteBatch.Draw(_graphicsManager.LoadTexture("misc/pixel"), new Rectangle(screenX, screenY + (int)(barHeight + padding), (int)(barWidth * Player.Charge / 100), (int)barHeight), Color.Green);
        }

        public void PlayerDied(DeathReason reason)
        {
            _explosionManager.AddExplosion(Player.WorldPosition, GameScale);
            Game.SetState(new DeathScreenState(Game, _inputManager, _graphicsManager, reason));
        }

        public void ActivateButtonNotification()
        {
            _showButtonNotification = true;
            _buttonNotificationTimer = 0f;
        }
    }
}