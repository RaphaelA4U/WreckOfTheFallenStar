using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace WreckGame
{
    public class Game1 : Game
    {
        #region Constants
        private const int MAP_WIDTH_TILES = 32;
        private const int MAP_HEIGHT_TILES = 18;
        private const int TILE_SIZE = 32;
        private const int EXPLOSION_FRAME_COUNT = 3;
        private const float EXPLOSION_FRAME_TIME = 0.1f;
        #endregion

        #region Game State
        private float _gameScale = 2.5f;
        private bool _isPlayerDead = false;
        private bool _isGameStarted = false;
        private bool _isGamePaused = false;
        private bool _showHitboxes = false;
        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;
        #endregion

        #region Graphics & Camera
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Vector2 _cameraPosition;
        private Matrix _viewMatrix;
        private Texture2D _pixelTexture;
        private Texture2D _debugTexture;
        private Dictionary<char, Texture2D> _letterTextures;
        #endregion

        #region Map
        private Texture2D _asphaltTexture;
        private Texture2D _borderTexture;
        #endregion

        #region Player
        private Texture2D _droneTexture;
        private Vector2 _dronePosition;
        private Vector2 _droneVelocity;
        private Vector2 _droneWorldPosition;
        private float _hoverOffset;
        private Rectangle _playerHitbox;
        #endregion

        #region Enemies
        // Common enemy properties
        private struct EnemyData
        {
            public Texture2D Texture;
            public Vector2 Position;
            public Vector2 WorldPosition;
            public float HoverOffset;
            public Rectangle Hitbox;
            public float Speed;
            public int Direction;
            public bool Active;
        }

        #region Items
        private struct DataShard
        {
            public Texture2D Texture;
            public Vector2 Position;
            public Vector2 WorldPosition;
            public float HoverOffset;
            public Rectangle Hitbox;
            public bool Active;
        }

        private DataShard[] _dataShards;
        private Random _random;
        private const int MIN_SHARDS = 2;
        private const int MAX_SHARDS = 5;
        #endregion

        private EnemyData[] _enemies;
        #endregion

        #region Explosion
        private Texture2D[] _explosionTextures;
        
        // Player explosion
        private int _currentExplosionFrame = 0;
        private float _frameTimer = 0f;
        private bool _explosionActive = false;
        private Vector2 _explosionPosition;

        // Enemy explosion
        private int _enemyExplosionFrame = 0;
        private float _enemyExplosionTimer = 0f;
        private bool _enemyExplosionActive = false;
        private Vector2 _enemyExplosionPosition;
        #endregion

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnWindowSizeChanged;
            _random = new Random();
        }

        #region Init & Loading
        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
            
            // Initialize enemy array
            _enemies = new EnemyData[3];
            
            // Remove ResetGame() from here
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Load textures
            LoadMapTextures();
            LoadCharacterTextures();
            LoadExplosionTextures();
            LoadDebugTextures();
            LoadAlphabetTextures();
            LoadItemTextures();
            
            // Move ResetGame() to here, after all textures are loaded
            ResetGame();
        }

        private void LoadItemTextures()
        {
            var dataShardTexture = Content.Load<Texture2D>("items/data_shard");
            
            // Create data shard array with initial capacity
            _dataShards = new DataShard[MAX_SHARDS];
            
            // Initialize all possible data shards with the texture
            for (int i = 0; i < _dataShards.Length; i++)
            {
                _dataShards[i].Texture = dataShardTexture;
            }
        }

        private void LoadMapTextures()
        {
            _asphaltTexture = Content.Load<Texture2D>("tiles/asphalt");
            _borderTexture = Content.Load<Texture2D>("tiles/border");
        }

        private void LoadCharacterTextures()
        {
            _droneTexture = Content.Load<Texture2D>("entities/drone");
            _enemies[0].Texture = Content.Load<Texture2D>("entities/drone_enemy");
            _enemies[1].Texture = Content.Load<Texture2D>("entities/drone_enemy1");
            _enemies[2].Texture = Content.Load<Texture2D>("entities/drone_enemy2");
        }

        private void LoadExplosionTextures()
        {
            _explosionTextures = new Texture2D[EXPLOSION_FRAME_COUNT];
            for (int i = 0; i < EXPLOSION_FRAME_COUNT; i++)
            {
                _explosionTextures[i] = Content.Load<Texture2D>($"particles/explosion{i + 1}");
            }
        }

        private void LoadDebugTextures()
        {
            _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            _debugTexture = new Texture2D(GraphicsDevice, 1, 1);
            _debugTexture.SetData(new[] { Color.White });
        }

        private void LoadAlphabetTextures()
        {
            _letterTextures = new Dictionary<char, Texture2D>();
            string chars = "abcdefghijklmnopqrstuvwxyz";
            
            foreach (char c in chars)
            {
                _letterTextures[c] = Content.Load<Texture2D>($"font/{c}");
                _letterTextures[char.ToUpper(c)] = Content.Load<Texture2D>($"font/{c}");
            }
        }

        private void ResetGame()
        {
            // Player position
            _droneWorldPosition = new Vector2(
                MAP_WIDTH_TILES * TILE_SIZE / 2, 
                MAP_HEIGHT_TILES * TILE_SIZE / 2);
            _dronePosition = _droneWorldPosition * _gameScale;
            _droneVelocity = Vector2.Zero;
            
            // Initialize enemies
            // First enemy (horizontal mover)
            _enemies[0].WorldPosition = new Vector2(
                _droneWorldPosition.X,
                _droneWorldPosition.Y - 160);
            _enemies[0].Position = _enemies[0].WorldPosition * _gameScale;
            _enemies[0].Direction = 1;
            _enemies[0].Active = true;
            _enemies[0].Speed = 80f;
            
            // Second enemy (vertical mover)
            _enemies[1].WorldPosition = new Vector2(
                _droneWorldPosition.X + 160,
                _droneWorldPosition.Y);
            _enemies[1].Position = _enemies[1].WorldPosition * _gameScale;
            _enemies[1].Direction = 1;
            _enemies[1].Active = true;
            _enemies[1].Speed = 60f;
            
            // Third enemy (follower)
            _enemies[2].WorldPosition = new Vector2(
                _droneWorldPosition.X - 160,
                _droneWorldPosition.Y);
            _enemies[2].Position = _enemies[2].WorldPosition * _gameScale;
            _enemies[2].Direction = 1;
            _enemies[2].Active = true;
            _enemies[2].Speed = 50f;

            InitializeDataShards();
        }

        private void InitializeDataShards()
        {
            // Determine how many shards to spawn (between MIN_SHARDS and MAX_SHARDS)
            int shardCount = _random.Next(MIN_SHARDS, MAX_SHARDS + 1);

            // Define the playable area with the inner edges of the border tiles
            float minX = TILE_SIZE;
            float minY = TILE_SIZE;
            float maxX = MAP_WIDTH_TILES * TILE_SIZE;
            float maxY = MAP_HEIGHT_TILES * TILE_SIZE;

            // Activate and position the shards
            for (int i = 0; i < _dataShards.Length; i++)
            {
                if (i < shardCount)
                {
                    // Activate this shard
                    _dataShards[i].Active = true;

                    // Generate random position
                    _dataShards[i].WorldPosition = new Vector2(
                        _random.Next((int)minX, (int)maxX),
                        _random.Next((int)minY, (int)maxY)
                    );

                    _dataShards[i].Position = _dataShards[i].WorldPosition * _gameScale;

                    // Initialize hitbox with scaled size
                    int width = _dataShards[i].Texture != null ? (int)(_dataShards[i].Texture.Width * 0.7f) : 22;
                    int height = _dataShards[i].Texture != null ? (int)(_dataShards[i].Texture.Height * 0.7f) : 22;

                    _dataShards[i].Hitbox = new Rectangle(
                        (int)_dataShards[i].WorldPosition.X,
                        (int)(_dataShards[i].WorldPosition.Y + _dataShards[i].HoverOffset),
                        width,
                        height
                    );
                }
                else
                {
                    // Deactivate extra shards
                    _dataShards[i].Active = false;
                }
            }
        }

        private void OnWindowSizeChanged(object sender, EventArgs e)
        {
            // Handle window resize if needed
        }
        #endregion

        #region Update
        protected override void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            HandleGameStateInput(keyboard);
            
            // Only update gameplay if the game is active and not paused
            if (_isGameStarted && !_isPlayerDead && !_isGamePaused)
            {
                HandleDebugInput(keyboard);
                UpdatePlayerMovement(keyboard, delta);
                UpdateEnemyMovement(delta);
                UpdateCollision();
                CheckEnemyCollisions();
            }
            
            UpdateExplosions(delta);
            UpdateHoverEffects(gameTime);
            
            _previousKeyboardState = keyboard;
            // _previousMouseState is updated in HandleGameStateInput
            base.Update(gameTime);
        }

        private void HandleGameStateInput(KeyboardState keyboard)
        {
            MouseState mouse = Mouse.GetState();
            bool keyPressed = keyboard.GetPressedKeys().Length > 0 && _previousKeyboardState.GetPressedKeys().Length == 0;
            bool mouseClicked = mouse.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
            
            // Start game
            if (!_isGameStarted && (keyPressed || mouseClicked))
            {
                _isGameStarted = true;
                ResetGame();
                return;
            }

            // Restart after death
            if (_isPlayerDead && (keyPressed || mouseClicked))
            {
                ResetGame();
                _isPlayerDead = false;
                return;
            }
            
            // Handle pause toggle
            if (_isGameStarted && !_isPlayerDead && keyboard.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape))
            {
                _isGamePaused = !_isGamePaused;
                return;
            }
            
            // Resume from pause with any key or click
            if (_isGamePaused && (keyPressed || mouseClicked))
            {
                _isGamePaused = false;
                return;
            }
            
            // Update mouse state
            _previousMouseState = mouse;
        }

        private void HandleDebugInput(KeyboardState keyboard)
        {
            // Zoom in/out
            if (keyboard.IsKeyDown(Keys.Add) && _previousKeyboardState.IsKeyUp(Keys.Add))
            {
                _gameScale += 0.5f;
            }

            if (keyboard.IsKeyDown(Keys.Subtract) && _previousKeyboardState.IsKeyUp(Keys.Subtract))
            {
                _gameScale = Math.Max(0.5f, _gameScale - 0.5f);
            }

            // Toggle hitboxes
            if (keyboard.IsKeyDown(Keys.LeftControl) && 
                keyboard.IsKeyDown(Keys.LeftShift) && 
                keyboard.IsKeyDown(Keys.H) && 
                !(_previousKeyboardState.IsKeyDown(Keys.LeftControl) && 
                _previousKeyboardState.IsKeyDown(Keys.LeftShift) && 
                _previousKeyboardState.IsKeyDown(Keys.H)))
            {
                _showHitboxes = !_showHitboxes;
            }
        }

        private void UpdatePlayerMovement(KeyboardState keyboard, float delta)
        {
            float acceleration = 1500f;
            float maxSpeed = 500f;
            float friction = 3f;

            // Get input direction
            Vector2 direction = Vector2.Zero;
            if (keyboard.IsKeyDown(Keys.A)) direction.X -= 1;
            if (keyboard.IsKeyDown(Keys.D)) direction.X += 1;
            if (keyboard.IsKeyDown(Keys.W)) direction.Y -= 1;
            if (keyboard.IsKeyDown(Keys.S)) direction.Y += 1;

            // Apply acceleration
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                _droneVelocity += direction * acceleration * delta;
            }

            // Apply friction
            if (direction == Vector2.Zero || _droneVelocity.Length() > maxSpeed)
            {
                _droneVelocity -= _droneVelocity * friction * delta;
            }

            // Limit speed
            if (_droneVelocity.Length() > maxSpeed)
            {
                _droneVelocity.Normalize();
                _droneVelocity *= maxSpeed;
            }

            // Update position
            _droneWorldPosition += _droneVelocity * delta;

            // Clamp player to the inner edges of the border tiles
            float minX = TILE_SIZE; // Inner edge of left border
            float minY = TILE_SIZE; // Inner edge of top border
            float maxX = MAP_WIDTH_TILES * TILE_SIZE; // Inner edge of right border
            float maxY = MAP_HEIGHT_TILES * TILE_SIZE; // Inner edge of bottom border

            _droneWorldPosition.X = MathHelper.Clamp(_droneWorldPosition.X, minX, maxX);
            _droneWorldPosition.Y = MathHelper.Clamp(_droneWorldPosition.Y, minY, maxY);
            _dronePosition = _droneWorldPosition * _gameScale;

            // Update camera
            _cameraPosition = _droneWorldPosition;
            _viewMatrix = Matrix.CreateScale(_gameScale) *
                Matrix.CreateTranslation(
                    -_cameraPosition.X * _gameScale + GraphicsDevice.Viewport.Width / 2,
                    -_cameraPosition.Y * _gameScale + GraphicsDevice.Viewport.Height / 2,
                    0);

            // Update hitbox
            _playerHitbox = new Rectangle(
                (int)_droneWorldPosition.X,
                (int)(_droneWorldPosition.Y + _hoverOffset),
                _droneTexture.Width,
                _droneTexture.Height
            );
        }

        private void UpdateEnemyMovement(float delta)
        {
            // Define the inner edges of the border tiles (same as player)
            float minX = TILE_SIZE; // Inner edge of left border
            float minY = TILE_SIZE; // Inner edge of top border
            float maxX = MAP_WIDTH_TILES * TILE_SIZE; // Inner edge of right border
            float maxY = MAP_HEIGHT_TILES * TILE_SIZE; // Inner edge of bottom border

            // First enemy - horizontal movement
            if (_enemies[0].Active)
            {
                _enemies[0].WorldPosition.X += _enemies[0].Speed * _enemies[0].Direction * delta;

                // Handle boundary collision (same as player, no texture size adjustment)
                if (_enemies[0].WorldPosition.X <= minX)
                {
                    _enemies[0].WorldPosition.X = minX;
                    _enemies[0].Direction = 1; // Change direction to right
                }
                else if (_enemies[0].WorldPosition.X >= maxX)
                {
                    _enemies[0].WorldPosition.X = maxX;
                    _enemies[0].Direction = -1; // Change direction to left
                }

                _enemies[0].Position = _enemies[0].WorldPosition * _gameScale;

                // Update hitbox
                _enemies[0].Hitbox = new Rectangle(
                    (int)_enemies[0].WorldPosition.X,
                    (int)(_enemies[0].WorldPosition.Y + _enemies[0].HoverOffset),
                    _enemies[0].Texture.Width,
                    _enemies[0].Texture.Height
                );
            }

            // Second enemy - vertical movement
            if (_enemies[1].Active)
            {
                _enemies[1].WorldPosition.Y += _enemies[1].Speed * _enemies[1].Direction * delta;

                // Handle boundary collision (same as player, no texture size adjustment)
                if (_enemies[1].WorldPosition.Y <= minY)
                {
                    _enemies[1].WorldPosition.Y = minY;
                    _enemies[1].Direction = 1; // Change direction to down
                }
                else if (_enemies[1].WorldPosition.Y >= maxY)
                {
                    _enemies[1].WorldPosition.Y = maxY;
                    _enemies[1].Direction = -1; // Change direction to up
                }

                _enemies[1].Position = _enemies[1].WorldPosition * _gameScale;

                // Update hitbox
                _enemies[1].Hitbox = new Rectangle(
                    (int)_enemies[1].WorldPosition.X,
                    (int)(_enemies[1].WorldPosition.Y + _enemies[1].HoverOffset),
                    _enemies[1].Texture.Width,
                    _enemies[1].Texture.Height
                );
            }

            // Third enemy - follows player
            if (_enemies[2].Active)
            {
                Vector2 toPlayer = _droneWorldPosition - _enemies[2].WorldPosition;

                if (toPlayer.Length() > 16)
                {
                    toPlayer.Normalize();
                    _enemies[2].WorldPosition += toPlayer * _enemies[2].Speed * delta;

                    // Clamp to boundaries (same as player, no texture size adjustment)
                    _enemies[2].WorldPosition.X = MathHelper.Clamp(_enemies[2].WorldPosition.X, minX, maxX);
                    _enemies[2].WorldPosition.Y = MathHelper.Clamp(_enemies[2].WorldPosition.Y, minY, maxY);
                }

                _enemies[2].Position = _enemies[2].WorldPosition * _gameScale;

                // Update hitbox
                _enemies[2].Hitbox = new Rectangle(
                    (int)_enemies[2].WorldPosition.X,
                    (int)(_enemies[2].WorldPosition.Y + _enemies[2].HoverOffset),
                    _enemies[2].Texture.Width,
                    _enemies[2].Texture.Height
                );
            }
        }

        private void UpdateCollision()
        {
            // Check player collision with any enemy
            for (int i = 0; i < _enemies.Length; i++)
            {
                if (_enemies[i].Active && _playerHitbox.Intersects(_enemies[i].Hitbox))
                {
                    _isPlayerDead = true;
                    Vector2 collisionPosition = (_droneWorldPosition + _enemies[i].WorldPosition) / 2;
                    _explosionPosition = collisionPosition * _gameScale;
                    _explosionActive = true;
                    _currentExplosionFrame = 0;
                    _frameTimer = 0f;
                    break;
                }
            }
            
            // Check player collision with data shards
            for (int i = 0; i < _dataShards.Length; i++)
            {
                if (_dataShards[i].Active && _playerHitbox.Intersects(_dataShards[i].Hitbox))
                {
                    // Collected the shard!
                    _dataShards[i].Active = false;
                    
                    // You could add effects, score, or other gameplay elements here
                    // For example, play a sound, increase score, etc.
                }
            }
        }

        private void CheckEnemyCollisions()
        {
            // Check collisions between all enemies
            for (int i = 0; i < _enemies.Length; i++)
            {
                if (!_enemies[i].Active) continue;
                
                for (int j = i + 1; j < _enemies.Length; j++)
                {
                    if (!_enemies[j].Active) continue;
                    
                    if (_enemies[i].Hitbox.Intersects(_enemies[j].Hitbox))
                    {
                        Vector2 collisionPosition = (_enemies[i].WorldPosition + _enemies[j].WorldPosition) / 2;
                        _enemyExplosionPosition = collisionPosition * _gameScale;
                        _enemyExplosionActive = true;
                        _enemyExplosionFrame = 0;
                        _enemyExplosionTimer = 0f;
                        
                        _enemies[i].Active = false;
                        _enemies[j].Active = false;
                    }
                }
            }
        }

        private void UpdateExplosions(float delta)
        {
            // Update player explosion
            if (_explosionActive)
            {
                _frameTimer += delta;
                if (_frameTimer >= EXPLOSION_FRAME_TIME)
                {
                    _frameTimer = 0f;
                    _currentExplosionFrame++;
                    
                    if (_currentExplosionFrame >= EXPLOSION_FRAME_COUNT)
                    {
                        _explosionActive = false;
                    }
                }
            }
            
            // Update enemy explosion
            if (_enemyExplosionActive)
            {
                _enemyExplosionTimer += delta;
                if (_enemyExplosionTimer >= EXPLOSION_FRAME_TIME)
                {
                    _enemyExplosionTimer = 0f;
                    _enemyExplosionFrame++;
                    
                    if (_enemyExplosionFrame >= EXPLOSION_FRAME_COUNT)
                    {
                        _enemyExplosionActive = false;
                    }
                }
            }
        }

        private void UpdateHoverEffects(GameTime gameTime)
        {
            double time = gameTime.TotalGameTime.TotalSeconds;
            _hoverOffset = (float)Math.Sin(time * 5f) * 15f;
            
            // Different hover effects for each enemy
            _enemies[0].HoverOffset = (float)Math.Sin(time * 2.5f) * 10f;
            _enemies[1].HoverOffset = (float)Math.Sin(time * 3.5f) * 12f;
            _enemies[2].HoverOffset = (float)Math.Sin(time * 4.5f) * 8f;
            
            // Data shard hover effect - slight rotation effect
            for (int i = 0; i < _dataShards.Length; i++)
            {
                if (_dataShards[i].Active)
                {
                    // Each shard hovers at a different phase to look natural
                    _dataShards[i].HoverOffset = (float)Math.Sin(time * 3f + i * 0.5f) * 6f;
                    
                    // Update hitbox position with hover offset AND scale the hitbox to match 0.7 scale
                    _dataShards[i].Hitbox = new Rectangle(
                        (int)_dataShards[i].WorldPosition.X,
                        (int)(_dataShards[i].WorldPosition.Y + _dataShards[i].HoverOffset),
                        (int)(_dataShards[i].Texture.Width * 0.7f),
                        (int)(_dataShards[i].Texture.Height * 0.7f)
                    );
                }
            }
        }
        #endregion

        #region Drawing
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            if (!_isGameStarted)
            {
                DrawStartScreen();
            }
            else
            {
                DrawGame();
            }

            if (_isPlayerDead)
            {
                DrawDeathScreen();
            }
            else if (_isGamePaused)
            {
                DrawPauseScreen();
            }
            
            base.Draw(gameTime);
        }

        private void DrawStartScreen()
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            
            DrawOverlay(new Color(255, 165, 0, 150));
            
            string text = "START GAME";
            DrawCenteredText(text, Color.White, 3f);
            DrawText("Press any key to start", new Vector2(GraphicsDevice.Viewport.Width / 2 - 350, GraphicsDevice.Viewport.Height / 2 + 100), Color.White, 1f);

            _spriteBatch.End();
        }

        private void DrawPauseScreen()
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            
            DrawOverlay(new Color(0, 0, 128, 150)); // Blue tinted overlay
            
            DrawCenteredText("PAUSED", Color.White, 4f);
            DrawText("Press any key to continue", new Vector2(GraphicsDevice.Viewport.Width / 2 - 350, GraphicsDevice.Viewport.Height / 2 + 100), Color.White, 1f);

            _spriteBatch.End();
        }

        private void DrawDeathScreen()
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            
            DrawOverlay(new Color(255, 0, 0, 150));
            
            DrawCenteredText("DEATH", Color.White, 4f);
            DrawText("Press any key to restart", new Vector2(GraphicsDevice.Viewport.Width / 2 - 350, GraphicsDevice.Viewport.Height / 2 + 100), Color.White, 1f);

            _spriteBatch.End();
        }

        private void DrawGame()
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _viewMatrix);

            DrawMap();
            DrawCharacters();
            DrawDataShards();
            DrawExplosions();
            DrawDebugInfo();
            
            _spriteBatch.End();
        }

        private void DrawMap()
        {
            for (int y = 0; y < MAP_HEIGHT_TILES + 2; y++)
            {
                for (int x = 0; x < MAP_WIDTH_TILES + 2; x++)
                {
                    Texture2D tileTexture = (x == 0 || y == 0 || 
                                        x == MAP_WIDTH_TILES + 1 || 
                                        y == MAP_HEIGHT_TILES + 1) 
                                        ? _borderTexture : _asphaltTexture;
                    
                    Vector2 tileWorldPosition = new Vector2(x * TILE_SIZE, y * TILE_SIZE);
                    
                    _spriteBatch.Draw(
                        tileTexture,
                        tileWorldPosition,
                        null,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        1.0f,
                        SpriteEffects.None,
                        0f);
                }
            }
        }

        private void DrawCharacters()
        {
            // Draw player if alive
            if (!_isPlayerDead)
            {
                _spriteBatch.Draw(
                    _droneTexture,
                    new Vector2(_droneWorldPosition.X, _droneWorldPosition.Y + _hoverOffset),
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    1.0f,
                    SpriteEffects.None,
                    0f);
            }
            
            // Draw active enemies
            for (int i = 0; i < _enemies.Length; i++)
            {
                if (_enemies[i].Active)
                {
                    // Determine facing direction for enemies
                    SpriteEffects effect = SpriteEffects.None;
                    if (i == 0 && _enemies[i].Direction == -1)
                        effect = SpriteEffects.FlipHorizontally;
                    else if (i == 2)
                    {
                        Vector2 toPlayer = _droneWorldPosition - _enemies[i].WorldPosition;
                        if (toPlayer.X < 0)
                            effect = SpriteEffects.FlipHorizontally;
                    }
                    
                    _spriteBatch.Draw(
                        _enemies[i].Texture, 
                        new Vector2(_enemies[i].WorldPosition.X, _enemies[i].WorldPosition.Y + _enemies[i].HoverOffset), 
                        null,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        1.0f,
                        effect,
                        0f);
                }
            }
        }

        private void DrawDataShards()
        {
            for (int i = 0; i < _dataShards.Length; i++)
            {
                if (_dataShards[i].Active)
                {
                    // Draw data shard with hover offset and slightly smaller scale (0.7)
                    _spriteBatch.Draw(
                        _dataShards[i].Texture,
                        new Vector2(_dataShards[i].WorldPosition.X, _dataShards[i].WorldPosition.Y + _dataShards[i].HoverOffset),
                        null,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        0.7f, // Make it a bit smaller than the drones
                        SpriteEffects.None,
                        0f);
                }
            }
        }

        private void DrawExplosions()
        {
            // Draw player explosion
            if (_explosionActive && _currentExplosionFrame < EXPLOSION_FRAME_COUNT)
            {
                DrawExplosion(_explosionTextures[_currentExplosionFrame], _explosionPosition);
            }
            
            // Draw enemy explosion
            if (_enemyExplosionActive && _enemyExplosionFrame < EXPLOSION_FRAME_COUNT)
            {
                DrawExplosion(_explosionTextures[_enemyExplosionFrame], _enemyExplosionPosition);
            }
        }
        
        private void DrawExplosion(Texture2D texture, Vector2 position)
        {
            _spriteBatch.Draw(
                texture,
                position / _gameScale,
                null,
                Color.White,
                0f,
                new Vector2(texture.Width / 2, texture.Height / 2),
                2f / 2.5f,
                SpriteEffects.None,
                0);
        }

        private void DrawDebugInfo()
        {
            if (_showHitboxes)
            {
                // Draw player hitbox
                _spriteBatch.Draw(_debugTexture, _playerHitbox, new Color(255, 255, 255, 100));
                
                // Draw enemy hitboxes with different colors
                Color[] colors = {
                    new Color(255, 255, 0, 100),
                    new Color(255, 165, 0, 100),
                    new Color(0, 0, 255, 100)
                };
                
                for (int i = 0; i < _enemies.Length; i++)
                {
                    if (_enemies[i].Active)
                    {
                        _spriteBatch.Draw(_debugTexture, _enemies[i].Hitbox, colors[i]);
                    }
                }
                
                // Draw data shard hitboxes
                Color shardHitboxColor = new Color(0, 255, 0, 100); // Green
                for (int i = 0; i < _dataShards.Length; i++)
                {
                    if (_dataShards[i].Active)
                    {
                        _spriteBatch.Draw(_debugTexture, _dataShards[i].Hitbox, shardHitboxColor);
                    }
                }
            }
        }
        
        private void DrawOverlay(Color color)
        {
            _spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                null,
                color,
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                0f);
        }
        
        private void DrawCenteredText(string text, Color color, float scale)
        {
            float textWidth = text.Length * 32 * scale;
            Vector2 position = new Vector2(
                (GraphicsDevice.Viewport.Width - textWidth) / 2,
                (GraphicsDevice.Viewport.Height - 32 * scale) / 2);
                
            DrawText(text, position, color, scale);
        }

        private void DrawText(string text, Vector2 position, Color color, float scale)
        {
            float spacing = 32 * scale;
            Vector2 pos = position;
            
            foreach (char c in text)
            {
                if (c == ' ')
                {
                    pos.X += spacing;
                    continue;
                }
                
                if (_letterTextures.ContainsKey(c))
                {
                    Texture2D letterTexture = _letterTextures[c];
                    _spriteBatch.Draw(
                        letterTexture,
                        pos,
                        null,
                        color,
                        0f,
                        Vector2.Zero,
                        scale,
                        SpriteEffects.None,
                        0);
                }
                pos.X += spacing;
            }
        }
        #endregion
    }
}
