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
        private bool _showHitboxes = false;
        private KeyboardState _previousKeyboardState;
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
        }

        #region Init & Loading
        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
            
            // Initialize enemy array
            _enemies = new EnemyData[3];
            
            ResetGame();
            
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
            
            if (_isGameStarted && !_isPlayerDead)
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
            base.Update(gameTime);
        }

        private void HandleGameStateInput(KeyboardState keyboard)
        {
            // Start game
            if (!_isGameStarted && keyboard.GetPressedKeys().Length > 0 && _previousKeyboardState.GetPressedKeys().Length == 0)
            {
                _isGameStarted = true;
                ResetGame();
                return;
            }

            // Restart after death
            if (_isPlayerDead && keyboard.GetPressedKeys().Length > 0 && _previousKeyboardState.GetPressedKeys().Length == 0)
            {
                ResetGame();
                _isPlayerDead = false;
                return;
            }
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
            
            // Keep player within bounds
            float bufferFromWall = 16;
            float minX = TILE_SIZE + bufferFromWall;
            float minY = TILE_SIZE + bufferFromWall;
            float maxX = (MAP_WIDTH_TILES * TILE_SIZE) - bufferFromWall;
            float maxY = (MAP_HEIGHT_TILES * TILE_SIZE) - bufferFromWall;
            
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

            // Update hitboxes
            _playerHitbox = new Rectangle(
                (int)_droneWorldPosition.X,
                (int)(_droneWorldPosition.Y + _hoverOffset),
                _droneTexture.Width,
                _droneTexture.Height
            );
            
            // Update enemy hitboxes
            for (int i = 0; i < _enemies.Length; i++)
            {
                _enemies[i].Hitbox = new Rectangle(
                    (int)_enemies[i].WorldPosition.X,
                    (int)(_enemies[i].WorldPosition.Y + _enemies[i].HoverOffset),
                    _enemies[i].Texture.Width,
                    _enemies[i].Texture.Height
                );
            }
        }

        private void UpdateEnemyMovement(float delta)
        {
            // First enemy - horizontal movement
            if (_enemies[0].Active)
            {
                _enemies[0].WorldPosition.X += _enemies[0].Speed * _enemies[0].Direction * delta;
                
                // Handle boundary collision
                float enemyMinX = TILE_SIZE * 2;
                float enemyMaxX = (MAP_WIDTH_TILES * TILE_SIZE) - (TILE_SIZE * 2) - _enemies[0].Texture.Width;
                
                if (_enemies[0].WorldPosition.X <= enemyMinX)
                {
                    _enemies[0].WorldPosition.X = enemyMinX;
                    _enemies[0].Direction = 1;  // Change direction to right
                }
                else if (_enemies[0].WorldPosition.X >= enemyMaxX)
                {
                    _enemies[0].WorldPosition.X = enemyMaxX;
                    _enemies[0].Direction = -1;  // Change direction to left
                }
                
                _enemies[0].Position = _enemies[0].WorldPosition * _gameScale;
            }
            
            // Second enemy - vertical movement
            if (_enemies[1].Active)
            {
                _enemies[1].WorldPosition.Y += _enemies[1].Speed * _enemies[1].Direction * delta;
                
                float enemyMinY = TILE_SIZE * 2;
                float enemyMaxY = (MAP_HEIGHT_TILES * TILE_SIZE) - (TILE_SIZE * 2) - _enemies[1].Texture.Height;
                
                if (_enemies[1].WorldPosition.Y <= enemyMinY)
                {
                    _enemies[1].WorldPosition.Y = enemyMinY;
                    _enemies[1].Direction = 1;  // Change direction to down
                }
                else if (_enemies[1].WorldPosition.Y >= enemyMaxY)
                {
                    _enemies[1].WorldPosition.Y = enemyMaxY;
                    _enemies[1].Direction = -1;  // Change direction to up
                }
                
                _enemies[1].Position = _enemies[1].WorldPosition * _gameScale;
            }
            
            // Third enemy - follows player
            if (_enemies[2].Active)
            {
                Vector2 toPlayer = _droneWorldPosition - _enemies[2].WorldPosition;
                
                if (toPlayer.Length() > 16)
                {
                    toPlayer.Normalize();
                    _enemies[2].WorldPosition += toPlayer * _enemies[2].Speed * delta;
                    
                    // Keep within bounds
                    float minX = TILE_SIZE * 2;
                    float minY = TILE_SIZE * 2;
                    float maxX = (MAP_WIDTH_TILES * TILE_SIZE) - (TILE_SIZE * 2) - _enemies[2].Texture.Width;
                    float maxY = (MAP_HEIGHT_TILES * TILE_SIZE) - (TILE_SIZE * 2) - _enemies[2].Texture.Height;
                    
                    _enemies[2].WorldPosition.X = MathHelper.Clamp(_enemies[2].WorldPosition.X, minX, maxX);
                    _enemies[2].WorldPosition.Y = MathHelper.Clamp(_enemies[2].WorldPosition.Y, minY, maxY);
                }
                
                _enemies[2].Position = _enemies[2].WorldPosition * _gameScale;
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

        private void DrawGame()
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _viewMatrix);

            DrawMap();
            DrawCharacters();
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
            }
        }

        private void DrawDeathScreen()
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            
            DrawOverlay(new Color(255, 0, 0, 150));
            
            DrawCenteredText("DEATH", Color.White, 4f);
            DrawText("Press any key to restart", new Vector2(GraphicsDevice.Viewport.Width / 2 - 350, GraphicsDevice.Viewport.Height / 2 + 100), Color.White, 1f);

            _spriteBatch.End();
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
