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
        private bool _isDragging = false;
        private int _draggedEnemyIndex = -1;
        private int _draggedDataShardIndex = -1;
        private int _draggedRepairPartIndex = -1;
        private int _draggedChargeItemIndex = -1;
        private Vector2 _dragOffset = Vector2.Zero;
        private Effect _colorReplaceEffect;

        #region Constants
        private const int MAP_WIDTH_TILES = 32;
        private const int MAP_HEIGHT_TILES = 18;
        private const int TILE_SIZE = 32;
        private const int EXPLOSION_FRAME_COUNT = 3;
        private const float EXPLOSION_FRAME_TIME = 0.1f;
        #endregion

        #region Game State
        private float _gameScale = 2.0f;
        private bool _isPlayerDead = false;
        private bool _isGameStarted = false;
        private bool _isGamePaused = false;
        private bool _showHitboxes = false;
        private bool _editMode = false;
        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;
        private float _damageFlashTimer = 0f;
        private const float DAMAGE_FLASH_DURATION = 0.3f;
        #endregion

        #region Graphics & Camera
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Vector2 _cameraPosition;
        private Matrix _viewMatrix;
        private Texture2D _pixelTexture;
        private Texture2D _debugTexture;
        private Dictionary<char, Texture2D> _fontTextures;
        private Texture2D _fontBackgroundTexture;
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
        private int _playerHP = 100;
        private int _playerCharge = 100;
        private float _chargeTimer = 0f;
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

        private struct RepairPart
        {
            public Texture2D Texture;
            public Vector2 Position;
            public Vector2 WorldPosition;
            public float HoverOffset;
            public Rectangle Hitbox;
            public bool Active;
        }

        private struct ChargeItem
        {
            public Texture2D Texture;
            public Vector2 Position;
            public Vector2 WorldPosition;
            public float HoverOffset;
            public Rectangle Hitbox;
            public bool Active;
        }

        private DataShard[] _dataShards;
        private RepairPart[] _repairParts;
        private ChargeItem[] _chargeItems;
        private readonly Random _random;
        private const int MIN_SHARDS = 2;
        private const int MAX_SHARDS = 5;
        private const int MIN_PARTS = 1;
        private const int MAX_PARTS = 3;
        private const int MIN_CHARGES = 1;
        private const int MAX_CHARGES = 3;
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
            _colorReplaceEffect = Content.Load<Effect>("ColorReplace");

            _colorReplaceEffect.Parameters["TextColor"].SetValue(Color.White.ToVector4());
            _colorReplaceEffect.Parameters["BackgroundColor"].SetValue(Color.White.ToVector4());
            _colorReplaceEffect.Parameters["IsBackground"].SetValue(false);

            // Load textures
            LoadMapTextures();
            LoadCharacterTextures();
            LoadExplosionTextures();
            LoadDebugTextures();
            LoadFontTextures();
            LoadItemTextures();
            
            // Move ResetGame() to here, after all textures are loaded
            ResetGame();
        }

        private void LoadItemTextures()
        {
            var dataShardTexture = Content.Load<Texture2D>("items/data_shard");
            var repairPartTexture = Content.Load<Texture2D>("items/parts");
            var chargeItemTexture = Content.Load<Texture2D>("items/charge");
            
            // Create data shard array with initial capacity
            _dataShards = new DataShard[MAX_SHARDS];
            _repairParts = new RepairPart[MAX_PARTS];
            _chargeItems = new ChargeItem[MAX_CHARGES];
            
            // Initialize all possible data shards with the texture
            for (int i = 0; i < _dataShards.Length; i++)
            {
                _dataShards[i].Texture = dataShardTexture;
            }
            
            // Initialize all possible repair parts with the texture
            for (int i = 0; i < _repairParts.Length; i++)
            {
                _repairParts[i].Texture = repairPartTexture;
            }
            
            // Initialize all possible charge items with the texture
            for (int i = 0; i < _chargeItems.Length; i++)
            {
                _chargeItems[i].Texture = chargeItemTexture;
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
            _pixelTexture.SetData([Color.White]);

            _debugTexture = new Texture2D(GraphicsDevice, 1, 1);
            _debugTexture.SetData([Color.White]);
        }

        private void LoadFontTextures()
        {
            _fontTextures = [];
            string chars = "abcdefghijklmnopqrstuvwxyz0123456789"; // Added numbers 0-9
            
            foreach (char c in chars)
            {
                _fontTextures[c] = Content.Load<Texture2D>($"font/{c}");
                
                // Only apply uppercase mapping for letters, not for numbers
                if (char.IsLetter(c))
                {
                    _fontTextures[char.ToUpper(c)] = Content.Load<Texture2D>($"font/{c}");
                }
            }
            
            // Load the background texture
            _fontBackgroundTexture = Content.Load<Texture2D>("font/background");
        }

        private void ResetGame()
        {
            // Player position
            _droneWorldPosition = new Vector2(
                MAP_WIDTH_TILES * TILE_SIZE / 2, 
                MAP_HEIGHT_TILES * TILE_SIZE / 2);
            _dronePosition = _droneWorldPosition * _gameScale;
            _droneVelocity = Vector2.Zero;
            _playerHP = 100;
            _playerCharge = 100;
            _chargeTimer = 0f;
            
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
            InitializeRepairParts();
            InitializeChargeItems();
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

        private void InitializeRepairParts()
        {
            // Determine how many parts to spawn (between MIN_PARTS and MAX_PARTS)
            int partCount = _random.Next(MIN_PARTS, MAX_PARTS + 1);

            // Define the playable area with the inner edges of the border tiles
            float minX = TILE_SIZE;
            float minY = TILE_SIZE;
            float maxX = MAP_WIDTH_TILES * TILE_SIZE;
            float maxY = MAP_HEIGHT_TILES * TILE_SIZE;

            // Activate and position the parts
            for (int i = 0; i < _repairParts.Length; i++)
            {
                if (i < partCount)
                {
                    // Activate this part
                    _repairParts[i].Active = true;

                    // Generate random position
                    _repairParts[i].WorldPosition = new Vector2(
                        _random.Next((int)minX, (int)maxX),
                        _random.Next((int)minY, (int)maxY)
                    );

                    _repairParts[i].Position = _repairParts[i].WorldPosition * _gameScale;

                    // Initialize hitbox with scaled size
                    int width = _repairParts[i].Texture != null ? (int)(_repairParts[i].Texture.Width * 0.7f) : 22;
                    int height = _repairParts[i].Texture != null ? (int)(_repairParts[i].Texture.Height * 0.7f) : 22;

                    _repairParts[i].Hitbox = new Rectangle(
                        (int)_repairParts[i].WorldPosition.X,
                        (int)(_repairParts[i].WorldPosition.Y + _repairParts[i].HoverOffset),
                        width,
                        height
                    );
                }
                else
                {
                    // Deactivate extra parts
                    _repairParts[i].Active = false;
                }
            }
        }

        private void InitializeChargeItems()
        {
            // Determine how many charge items to spawn (between MIN_CHARGES and MAX_CHARGES)
            int chargeCount = _random.Next(MIN_CHARGES, MAX_CHARGES + 1);

            // Define the playable area with the inner edges of the border tiles
            float minX = TILE_SIZE;
            float minY = TILE_SIZE;
            float maxX = MAP_WIDTH_TILES * TILE_SIZE;
            float maxY = MAP_HEIGHT_TILES * TILE_SIZE;

            // Activate and position the charge items
            for (int i = 0; i < _chargeItems.Length; i++)
            {
                if (i < chargeCount)
                {
                    // Activate this charge item
                    _chargeItems[i].Active = true;

                    // Generate random position
                    _chargeItems[i].WorldPosition = new Vector2(
                        _random.Next((int)minX, (int)maxX),
                        _random.Next((int)minY, (int)maxY)
                    );

                    _chargeItems[i].Position = _chargeItems[i].WorldPosition * _gameScale;

                    // Initialize hitbox with scaled size
                    int width = _chargeItems[i].Texture != null ? (int)(_chargeItems[i].Texture.Width * 0.7f) : 22;
                    int height = _chargeItems[i].Texture != null ? (int)(_chargeItems[i].Texture.Height * 0.7f) : 22;

                    _chargeItems[i].Hitbox = new Rectangle(
                        (int)_chargeItems[i].WorldPosition.X,
                        (int)(_chargeItems[i].WorldPosition.Y + _chargeItems[i].HoverOffset),
                        width,
                        height
                    );
                }
                else
                {
                    // Deactivate extra charge items
                    _chargeItems[i].Active = false;
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
            MouseState mouse = Mouse.GetState(); // Get current mouse state
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
                
                // Update damage flash timer
                if (_damageFlashTimer > 0)
                {
                    _damageFlashTimer -= delta;
                }
                
                if (!_editMode) 
                {
                    // Deplete charge over time
                    _chargeTimer += delta;
                    if (_chargeTimer >= 1.0f)
                    {
                        _playerCharge--;
                        _chargeTimer -= 1.0f;
                        
                        // Check if player is out of charge
                        if (_playerCharge <= 0)
                        {
                            _isPlayerDead = true;
                            Vector2 collisionPosition = _droneWorldPosition;
                            _explosionPosition = collisionPosition * _gameScale;
                            _explosionActive = true;
                            _currentExplosionFrame = 0;
                            _frameTimer = 0f;
                        }
                    }
                }
            }
            
            UpdateExplosions(delta);
            UpdateHoverEffects(gameTime);
            
            _previousKeyboardState = keyboard;
            
            if (_editMode)
            {
                HandleEditModeInput();
            }

            // Update previous mouse state here instead of in HandleGameStateInput
            _previousMouseState = mouse;

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
            
            // Remove this line - we'll update mouse state at the end of the Update method instead
            // _previousMouseState = mouse;
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

            // Toggle edit mode
            if (keyboard.IsKeyDown(Keys.LeftControl) && 
                keyboard.IsKeyDown(Keys.C) && 
                !(_previousKeyboardState.IsKeyDown(Keys.LeftControl) && 
                _previousKeyboardState.IsKeyDown(Keys.C)))
            {
                _editMode = !_editMode;
                _showHitboxes = !_showHitboxes;
            }
        }

        private void HandleEditModeInput()
        {
            // Handle right-click deletion in edit mode (existing code)
            MouseState mouse = Mouse.GetState();
            
            // Convert screen position to world position
            Vector2 screenPosition = new Vector2(mouse.X, mouse.Y);
            Vector2 worldPosition = Vector2.Transform(screenPosition, Matrix.Invert(_viewMatrix));
            
            // Define the inner edges of the border tiles (same as player boundaries)
            float minX = TILE_SIZE; // Inner edge of left border
            float minY = TILE_SIZE; // Inner edge of top border
            float maxX = MAP_WIDTH_TILES * TILE_SIZE; // Inner edge of right border
            float maxY = MAP_HEIGHT_TILES * TILE_SIZE; // Inner edge of bottom border
            
            // Check for right mouse button click (existing deletion code)
            if (mouse.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released)
            {
                // ...existing deletion code...
            }
            
            // Handle dragging with left mouse button
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (!_isDragging && _previousMouseState.LeftButton == ButtonState.Released)
                {
                    // Start dragging - check what we're clicking on
                    // Check enemies first
                    for (int i = 0; i < _enemies.Length; i++)
                    {
                        if (_enemies[i].Active && _enemies[i].Hitbox.Contains((int)worldPosition.X, (int)worldPosition.Y))
                        {
                            _isDragging = true;
                            _draggedEnemyIndex = i;
                            _draggedDataShardIndex = -1;
                            _draggedRepairPartIndex = -1;
                            _draggedChargeItemIndex = -1;
                            _dragOffset = _enemies[i].WorldPosition - worldPosition;
                            return;
                        }
                    }
                    
                    // Check data shards
                    for (int i = 0; i < _dataShards.Length; i++)
                    {
                        if (_dataShards[i].Active && _dataShards[i].Hitbox.Contains((int)worldPosition.X, (int)worldPosition.Y))
                        {
                            _isDragging = true;
                            _draggedEnemyIndex = -1;
                            _draggedDataShardIndex = i;
                            _draggedRepairPartIndex = -1;
                            _draggedChargeItemIndex = -1;
                            _dragOffset = _dataShards[i].WorldPosition - worldPosition;
                            return;
                        }
                    }
                    
                    // Check repair parts
                    for (int i = 0; i < _repairParts.Length; i++)
                    {
                        if (_repairParts[i].Active && _repairParts[i].Hitbox.Contains((int)worldPosition.X, (int)worldPosition.Y))
                        {
                            _isDragging = true;
                            _draggedEnemyIndex = -1;
                            _draggedDataShardIndex = -1;
                            _draggedRepairPartIndex = i;
                            _draggedChargeItemIndex = -1;
                            _dragOffset = _repairParts[i].WorldPosition - worldPosition;
                            return;
                        }
                    }
                    
                    // Check charge items
                    for (int i = 0; i < _chargeItems.Length; i++)
                    {
                        if (_chargeItems[i].Active && _chargeItems[i].Hitbox.Contains((int)worldPosition.X, (int)worldPosition.Y))
                        {
                            _isDragging = true;
                            _draggedEnemyIndex = -1;
                            _draggedDataShardIndex = -1;
                            _draggedRepairPartIndex = -1;
                            _draggedChargeItemIndex = i;
                            _dragOffset = _chargeItems[i].WorldPosition - worldPosition;
                            return;
                        }
                    }
                }
                else if (_isDragging)
                {
                    // Continue dragging - update position of the dragged entity
                    Vector2 newPosition = worldPosition + _dragOffset;
                    
                    // Clamp position to stay within map boundaries
                    newPosition.X = MathHelper.Clamp(newPosition.X, minX, maxX);
                    newPosition.Y = MathHelper.Clamp(newPosition.Y, minY, maxY);
                    
                    // Update position based on what we're dragging
                    if (_draggedEnemyIndex >= 0)
                    {
                        _enemies[_draggedEnemyIndex].WorldPosition = newPosition;
                        _enemies[_draggedEnemyIndex].Position = _enemies[_draggedEnemyIndex].WorldPosition * _gameScale;
                        
                        // Update hitbox
                        _enemies[_draggedEnemyIndex].Hitbox = new Rectangle(
                            (int)_enemies[_draggedEnemyIndex].WorldPosition.X,
                            (int)(_enemies[_draggedEnemyIndex].WorldPosition.Y + _enemies[_draggedEnemyIndex].HoverOffset),
                            _enemies[_draggedEnemyIndex].Texture.Width,
                            _enemies[_draggedEnemyIndex].Texture.Height
                        );
                    }
                    else if (_draggedDataShardIndex >= 0)
                    {
                        _dataShards[_draggedDataShardIndex].WorldPosition = newPosition;
                        _dataShards[_draggedDataShardIndex].Position = _dataShards[_draggedDataShardIndex].WorldPosition * _gameScale;
                        
                        // Update hitbox
                        _dataShards[_draggedDataShardIndex].Hitbox = new Rectangle(
                            (int)_dataShards[_draggedDataShardIndex].WorldPosition.X,
                            (int)(_dataShards[_draggedDataShardIndex].WorldPosition.Y + _dataShards[_draggedDataShardIndex].HoverOffset),
                            (int)(_dataShards[_draggedDataShardIndex].Texture.Width * 0.7f),
                            (int)(_dataShards[_draggedDataShardIndex].Texture.Height * 0.7f)
                        );
                    }
                    else if (_draggedRepairPartIndex >= 0)
                    {
                        _repairParts[_draggedRepairPartIndex].WorldPosition = newPosition;
                        _repairParts[_draggedRepairPartIndex].Position = _repairParts[_draggedRepairPartIndex].WorldPosition * _gameScale;
                        
                        // Update hitbox
                        _repairParts[_draggedRepairPartIndex].Hitbox = new Rectangle(
                            (int)_repairParts[_draggedRepairPartIndex].WorldPosition.X,
                            (int)(_repairParts[_draggedRepairPartIndex].WorldPosition.Y + _repairParts[_draggedRepairPartIndex].HoverOffset),
                            (int)(_repairParts[_draggedRepairPartIndex].Texture.Width * 0.7f),
                            (int)(_repairParts[_draggedRepairPartIndex].Texture.Height * 0.7f)
                        );
                    }
                    else if (_draggedChargeItemIndex >= 0)
                    {
                        _chargeItems[_draggedChargeItemIndex].WorldPosition = newPosition;
                        _chargeItems[_draggedChargeItemIndex].Position = _chargeItems[_draggedChargeItemIndex].WorldPosition * _gameScale;
                        
                        // Update hitbox
                        _chargeItems[_draggedChargeItemIndex].Hitbox = new Rectangle(
                            (int)_chargeItems[_draggedChargeItemIndex].WorldPosition.X,
                            (int)(_chargeItems[_draggedChargeItemIndex].WorldPosition.Y + _chargeItems[_draggedChargeItemIndex].HoverOffset),
                            (int)(_chargeItems[_draggedChargeItemIndex].Texture.Width * 0.7f),
                            (int)(_chargeItems[_draggedChargeItemIndex].Texture.Height * 0.7f)
                        );
                    }
                }
            }
            else if (_isDragging)
            {
                // Mouse was released, stop dragging
                _isDragging = false;
                _draggedEnemyIndex = -1;
                _draggedDataShardIndex = -1;
                _draggedRepairPartIndex = -1;
                _draggedChargeItemIndex = -1;
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
            if (_editMode)
                return;

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
                    // Reduce player HP instead of instant death
                    _playerHP -= 25;
                    
                    // Trigger damage flash effect
                    _damageFlashTimer = DAMAGE_FLASH_DURATION;
                    
                    // Create explosion at collision point for ANY enemy collision
                    Vector2 collisionPosition = (_droneWorldPosition + _enemies[i].WorldPosition) / 2;
                    _explosionPosition = collisionPosition * _gameScale;
                    _explosionActive = true;
                    _currentExplosionFrame = 0;
                    _frameTimer = 0f;
                    
                    // Check if player is out of HP
                    if (_playerHP <= 0)
                    {
                        _isPlayerDead = true;
                    }
                    
                    // Deactivate the enemy
                    _enemies[i].Active = false;
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
            
            // Check player collision with repair parts
            for (int i = 0; i < _repairParts.Length; i++)
            {
                if (_repairParts[i].Active && _playerHitbox.Intersects(_repairParts[i].Hitbox))
                {
                    // Collected the repair part!
                    _repairParts[i].Active = false;
                    
                    // Add health
                    _playerHP = Math.Min(_playerHP + 25, 100);
                }
            }
            
            // Check player collision with charge items
            for (int i = 0; i < _chargeItems.Length; i++)
            {
                if (_chargeItems[i].Active && _playerHitbox.Intersects(_chargeItems[i].Hitbox))
                {
                    // Collected the charge item!
                    _chargeItems[i].Active = false;
                    
                    // Add charge
                    _playerCharge = Math.Min(_playerCharge + 25, 100);
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
            
            // Update enemy hitboxes to match their hover offsets when in edit mode
            if (_editMode) {
                for (int i = 0; i < _enemies.Length; i++) {
                    if (_enemies[i].Active) {
                        _enemies[i].Hitbox = new Rectangle(
                            (int)_enemies[i].WorldPosition.X,
                            (int)(_enemies[i].WorldPosition.Y + _enemies[i].HoverOffset),
                            _enemies[i].Texture.Width,
                            _enemies[i].Texture.Height
                        );
                    }
                }
            }
            
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
            
            // Repair part hover effect
            for (int i = 0; i < _repairParts.Length; i++)
            {
                if (_repairParts[i].Active)
                {
                    _repairParts[i].HoverOffset = (float)Math.Sin(time * 2.7f + i * 0.3f) * 8f;
                    
                    _repairParts[i].Hitbox = new Rectangle(
                        (int)_repairParts[i].WorldPosition.X,
                        (int)(_repairParts[i].WorldPosition.Y + _repairParts[i].HoverOffset),
                        (int)(_repairParts[i].Texture.Width * 0.7f),
                        (int)(_repairParts[i].Texture.Height * 0.7f)
                    );
                }
            }
            
            // Charge item hover effect
            for (int i = 0; i < _chargeItems.Length; i++)
            {
                if (_chargeItems[i].Active)
                {
                    _chargeItems[i].HoverOffset = (float)Math.Sin(time * 3.3f + i * 0.7f) * 10f;
                    
                    _chargeItems[i].Hitbox = new Rectangle(
                        (int)_chargeItems[i].WorldPosition.X,
                        (int)(_chargeItems[i].WorldPosition.Y + _chargeItems[i].HoverOffset),
                        (int)(_chargeItems[i].Texture.Width * 0.7f),
                        (int)(_chargeItems[i].Texture.Height * 0.7f)
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
            // Define a more vibrant and fully opaque background color
            Color overlayColor = new Color(255, 165, 0, 255);
            Color textBgColor = new Color(255, 140, 0); // Darker orange for text background
            
            // First draw the overlay
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            DrawOverlay(overlayColor);
            _spriteBatch.End();
            
            // Draw text with shader effect
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, 
                            SamplerState.PointClamp, null, null, _colorReplaceEffect);
            
            string text = "START GAME";
            DrawCenteredText(text, Color.Black, textBgColor, 3f, false, true, 8f);
            
            DrawColoredText("Press any key to start", 
                new Vector2(GraphicsDevice.Viewport.Width / 2 - 350, GraphicsDevice.Viewport.Height / 2 + 100), 
                Color.Black, Color.Transparent, 1f, false, false, 0f);
            
            _spriteBatch.End();
        }

        private void DrawPauseScreen()
        {
            // Define vibrant and distinct colors
            Color overlayColor = new Color(0, 0, 255, 255);
            Color textBgColor = new Color(70, 70, 220); // Slightly darker blue
            
            // First draw the overlay
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            DrawOverlay(overlayColor);
            _spriteBatch.End();
            
            // Draw text with shader effect
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, 
                            SamplerState.PointClamp, null, null, _colorReplaceEffect);
            
            DrawCenteredText("PAUSED", Color.Black, textBgColor, 4f, false, true, 8f);
            
            DrawColoredText("Press any key to resume", 
                new Vector2(GraphicsDevice.Viewport.Width / 2 - 350, GraphicsDevice.Viewport.Height / 2 + 100), 
                Color.Black, Color.Transparent, 1f, false, false, 0f);
            
            _spriteBatch.End();
        }

        private void DrawDeathScreen()
        {
            // Define vibrant and distinct colors
            Color overlayColor = new Color(255, 0, 0, 150);
            Color textBgColor = new Color(200, 0, 0); // Darker red for text background
            
            // First draw the overlay
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            DrawOverlay(overlayColor);
            _spriteBatch.End();
            
            // Draw text with shader effect
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, 
                            SamplerState.PointClamp, null, null, _colorReplaceEffect);
            
            DrawCenteredText("SYSTEM FAILURE", Color.Black, textBgColor, 4f, false, true, 8f);
            
            DrawColoredText("Press any key to restart", 
                new Vector2(GraphicsDevice.Viewport.Width / 2 - 350, GraphicsDevice.Viewport.Height / 2 + 100), 
                Color.Black, Color.Transparent, 1f, false, false, 0f);
            
            _spriteBatch.End();
        }

        private void DrawGame()
        {
            // Draw world elements with camera transform
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _viewMatrix);

            DrawMap();
            DrawCharacters();
            DrawDataShards();
            DrawRepairParts();
            DrawChargeItems();
            DrawExplosions();
            DrawDebugInfo();
            
            _spriteBatch.End();
            
            // Draw UI elements in screen space (no camera transform)
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            DrawStatusBars();
            
            // Draw damage vignette effect
            if (_damageFlashTimer > 0)
            {
                // Calculate alpha based on remaining time (fades out)
                float alpha = _damageFlashTimer / DAMAGE_FLASH_DURATION;
                
                // Draw vignette (multiple rectangles with decreasing size and increasing transparency toward center)
                int width = GraphicsDevice.Viewport.Width;
                int height = GraphicsDevice.Viewport.Height;
                
                // Outer rectangles (more opaque)
                Color outerColor = new Color(1f, 0f, 0f, alpha * 0.6f);
                DrawVignetteRectangle(0, 0, width, height, outerColor);
            }
            
            _spriteBatch.End();
        }

        // Helper method to draw hollow rectangle for vignette effect
        private void DrawVignetteRectangle(int x, int y, int width, int height, Color color)
        {
            int thickness = 80; // Thickness of each vignette layer
            
            // Top edge
            _spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, width, thickness), color);
            
            // Bottom edge
            _spriteBatch.Draw(_pixelTexture, new Rectangle(x, y + height - thickness, width, thickness), color);
            
            // Left edge
            _spriteBatch.Draw(_pixelTexture, new Rectangle(x, y + thickness, thickness, height - 2 * thickness), color);
            
            // Right edge
            _spriteBatch.Draw(_pixelTexture, new Rectangle(x + width - thickness, y + thickness, thickness, height - 2 * thickness), color);
        }

        private void DrawRepairParts()
        {
            for (int i = 0; i < _repairParts.Length; i++)
            {
                if (_repairParts[i].Active)
                {
                    _spriteBatch.Draw(
                        _repairParts[i].Texture,
                        new Vector2(_repairParts[i].WorldPosition.X, _repairParts[i].WorldPosition.Y + _repairParts[i].HoverOffset),
                        null,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        0.7f,
                        SpriteEffects.None,
                        0f);
                }
            }
        }

        private void DrawChargeItems()
        {
            for (int i = 0; i < _chargeItems.Length; i++)
            {
                if (_chargeItems[i].Active)
                {
                    _spriteBatch.Draw(
                        _chargeItems[i].Texture,
                        new Vector2(_chargeItems[i].WorldPosition.X, _chargeItems[i].WorldPosition.Y + _chargeItems[i].HoverOffset),
                        null,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        0.7f,
                        SpriteEffects.None,
                        0f);
                }
            }
        }

        private void DrawStatusBars()
        {
            // Fixed position in screen space (not affected by camera zoom)
            float barWidth = 200;
            float barHeight = 20;
            float margin = 10;
            float padding = 2;
            
            // Position the bars at bottom left corner of the screen
            int screenX = (int)margin;
            int screenY = (int)(GraphicsDevice.Viewport.Height - margin - (barHeight * 2 + padding));
            
            // Draw HP bar background
            _spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    screenX,
                    screenY,
                    (int)barWidth,
                    (int)barHeight),
                Color.DarkRed);
            
            // Draw HP bar fill
            _spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    screenX,
                    screenY,
                    (int)(barWidth * _playerHP / 100),
                    (int)barHeight),
                Color.Red);
            
            // Draw Charge bar background
            _spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    screenX,
                    screenY + (int)(barHeight + padding),
                    (int)barWidth,
                    (int)barHeight),
                Color.DarkGreen);
            
            // Draw Charge bar fill
            _spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(
                    screenX,
                    screenY + (int)(barHeight + padding),
                    (int)(barWidth * _playerCharge / 100),
                    (int)barHeight),
                Color.Green);
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
                    
                    Vector2 tileWorldPosition = new(x * TILE_SIZE, y * TILE_SIZE);
                    
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
            // Draw explosion with a larger scale for better visibility
            _spriteBatch.Draw(
                texture,
                position / _gameScale, // Convert to world coordinates
                null,
                Color.White,
                0f,
                new Vector2(texture.Width / 2, texture.Height / 2), // Center of explosion
                1.5f, // Larger scale
                SpriteEffects.None,
                0);
        }

        private void DrawDebugInfo()
        {
            if (_showHitboxes)
            {
                // Draw player hitbox outline
                DrawRectangleOutline(_playerHitbox, Color.White, 1);
                
                // Draw enemy hitboxes
                for (int i = 0; i < _enemies.Length; i++)
                {
                    if (_enemies[i].Active)
                    {
                        DrawRectangleOutline(_enemies[i].Hitbox, Color.White, 1);
                    }
                }
                
                // Draw data shard hitboxes
                for (int i = 0; i < _dataShards.Length; i++)
                {
                    if (_dataShards[i].Active)
                    {
                        DrawRectangleOutline(_dataShards[i].Hitbox, Color.White, 1);
                    }
                }
                
                // Draw repair part hitboxes
                for (int i = 0; i < _repairParts.Length; i++)
                {
                    if (_repairParts[i].Active)
                    {
                        DrawRectangleOutline(_repairParts[i].Hitbox, Color.White, 1);
                    }
                }
                
                // Draw charge item hitboxes
                for (int i = 0; i < _chargeItems.Length; i++)
                {
                    if (_chargeItems[i].Active)
                    {
                        DrawRectangleOutline(_chargeItems[i].Hitbox, Color.White, 1);
                    }
                }
            }
        }

        // Helper method to draw a rectangle outline
        private void DrawRectangleOutline(Rectangle rect, Color color, int thickness = 1)
        {
            // Top line
            _spriteBatch.Draw(_pixelTexture, 
                new Rectangle(rect.Left, rect.Top, rect.Width, thickness), 
                color);
            
            // Bottom line
            _spriteBatch.Draw(_pixelTexture, 
                new Rectangle(rect.Left, rect.Bottom - thickness, rect.Width, thickness), 
                color);
            
            // Left line
            _spriteBatch.Draw(_pixelTexture, 
                new Rectangle(rect.Left, rect.Top, thickness, rect.Height), 
                color);
            
            // Right line
            _spriteBatch.Draw(_pixelTexture, 
                new Rectangle(rect.Right - thickness, rect.Top, thickness, rect.Height), 
                color);
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
        
        private void DrawColoredText(string text, Vector2 position, Color textColor, Color backgroundColor, float scale, bool beginBatch = true, bool showBackground = false, float letterSpacing = 0f)
        {
            // Base spacing is 32 pixels (the width of a character) plus additional spacing
            float baseSpacing = 32 * scale;
            float spacing = baseSpacing + letterSpacing * scale;
            Vector2 pos = position;
            
            // Begin a new SpriteBatch with the shader if requested
            if (beginBatch)
            {
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, 
                                SamplerState.PointClamp, null, null, _colorReplaceEffect);
            }
            
            foreach (char c in text)
            {
                if (c == ' ')
                {
                    pos.X += spacing;
                    continue;
                }
                
                if (_fontTextures.TryGetValue(c, out Texture2D letterTexture))
                {
                    if (showBackground)
                    {
                        // Calculate proper positioning to center letter on background
                        float backgroundScale = scale * 1.3f;
                        Vector2 backgroundPosition = new Vector2(
                            pos.X - ((_fontBackgroundTexture.Width * backgroundScale - letterTexture.Width * scale) / 2),
                            pos.Y - ((_fontBackgroundTexture.Height * backgroundScale - letterTexture.Height * scale) / 2)
                        );
                        
                        // Set background color for the shader and indicate this is background
                        _colorReplaceEffect.Parameters["BackgroundColor"].SetValue(backgroundColor.ToVector4());
                        _colorReplaceEffect.Parameters["IsBackground"].SetValue(true);
                        
                        // Draw background first
                        _spriteBatch.Draw(
                            _fontBackgroundTexture,
                            backgroundPosition,
                            null,
                            Color.White, // White, shader will apply the color
                            0f,
                            Vector2.Zero,
                            backgroundScale,
                            SpriteEffects.None,
                            0);
                    }
                    
                    // Set text color for the shader and indicate this is not background
                    _colorReplaceEffect.Parameters["TextColor"].SetValue(textColor.ToVector4());
                    _colorReplaceEffect.Parameters["IsBackground"].SetValue(false);
                    
                    // Draw letter on top
                    _spriteBatch.Draw(
                        letterTexture,
                        pos,
                        null,
                        Color.White, // White, shader will apply the color
                        0f,
                        Vector2.Zero,
                        scale,
                        SpriteEffects.None,
                        0);
                }
                pos.X += spacing;
            }
            
            // End the SpriteBatch if we began it
            if (beginBatch)
            {
                _spriteBatch.End();
            }
        }

        // Update DrawCenteredText to support the new letterSpacing parameter
        private void DrawCenteredText(string text, Color textColor, Color backgroundColor, float scale, bool beginBatch = true, bool showBackground = false, float letterSpacing = 0f)
        {
            // Calculate the width with the additional spacing
            float baseSpacing = 32 * scale;
            float spacing = baseSpacing + letterSpacing * scale;
            float textWidth = text.Length * spacing;
            
            Vector2 position = new(
                (GraphicsDevice.Viewport.Width - textWidth) / 2,
                (GraphicsDevice.Viewport.Height - 32 * scale) / 2);
                    
            DrawColoredText(text, position, textColor, backgroundColor, scale, beginBatch, showBackground, letterSpacing);
        }
        #endregion
    }
}
