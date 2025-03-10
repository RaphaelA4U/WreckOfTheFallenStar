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
        private int _previousScrollWheelValue;
        private bool _isDragging = false;
        private int _draggedEnemyIndex = -1;
        private int _draggedDataShardIndex = -1;
        private int _draggedRepairPartIndex = -1;
        private int _draggedChargeItemIndex = -1;
        private Vector2 _dragOffset = Vector2.Zero;

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
        private Texture2D _cursorTexture;
        private Color _cursorColor = Color.DarkSlateGray;
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
        private int _currentExplosionFrame = 0;
        private float _frameTimer = 0f;
        private bool _explosionActive = false;
        private Vector2 _explosionPosition;

        private int _enemyExplosionFrame = 0;
        private float _enemyExplosionTimer = 0f;
        private bool _enemyExplosionActive = false;
        private Vector2 _enemyExplosionPosition;
        #endregion

        #region Buttons
        private class Button
        {
            public Rectangle Bounds;
            public string Text;
            public Color TextColor;
            public float Scale;

            public bool Contains(Point point)
            {
                return Bounds.Contains(point);
            }
        }

        private Button[] _startScreenButtons;
        private Button[] _pauseScreenButtons;
        private Button[] _deathScreenButtons;
        #endregion

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
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
            
            _enemies = new EnemyData[3];
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            LoadMapTextures();
            LoadCharacterTextures();
            LoadExplosionTextures();
            LoadDebugTextures();
            LoadFontTextures();
            LoadItemTextures();
            
            _cursorTexture = Content.Load<Texture2D>("misc/cursor");
            
            InitializeButtons();
            
            ResetGame();
        }

        private void InitializeButtons()
        {
            _startScreenButtons = new Button[2];
            _startScreenButtons[0] = new Button
            {
                Text = "START",
                TextColor = Color.White,
                Scale = 2.0f,
                Bounds = CalculateButtonBounds("START", 2.0f, GraphicsDevice.Viewport.Height / 2 + 50)
            };
            _startScreenButtons[1] = new Button
            {
                Text = "EXIT",
                TextColor = Color.White,
                Scale = 2.0f,
                Bounds = CalculateButtonBounds("EXIT", 2.0f, GraphicsDevice.Viewport.Height / 2 + 130)
            };
            
            _pauseScreenButtons = new Button[3];
            _pauseScreenButtons[0] = new Button
            {
                Text = "RESUME",
                TextColor = Color.White,
                Scale = 2.0f,
                Bounds = CalculateButtonBounds("RESUME", 2.0f, GraphicsDevice.Viewport.Height / 2 + 50)
            };
            _pauseScreenButtons[1] = new Button
            {
                Text = "RESTART",
                TextColor = Color.White,
                Scale = 2.0f,
                Bounds = CalculateButtonBounds("RESTART", 2.0f, GraphicsDevice.Viewport.Height / 2 + 130)
            };
            _pauseScreenButtons[2] = new Button
            {
                Text = "EXIT",
                TextColor = Color.White,
                Scale = 2.0f,
                Bounds = CalculateButtonBounds("EXIT", 2.0f, GraphicsDevice.Viewport.Height / 2 + 210)
            };

            _deathScreenButtons = new Button[2];
            _deathScreenButtons[0] = new Button
            {
                Text = "RESTART",
                TextColor = Color.White,
                Scale = 2.0f,
                Bounds = CalculateButtonBounds("RESTART", 2.0f, GraphicsDevice.Viewport.Height / 2 + 50)
            };
            _deathScreenButtons[1] = new Button
            {
                Text = "EXIT",
                TextColor = Color.White,
                Scale = 2.0f,
                Bounds = CalculateButtonBounds("EXIT", 2.0f, GraphicsDevice.Viewport.Height / 2 + 130)
            };
        }

        private Rectangle CalculateButtonBounds(string text, float scale, int y)
        {
            float letterSpacing = 8.0f;
            Vector2 textSize = MeasureText(text, scale, letterSpacing);
            int width = (int)textSize.X;
            int height = 60;
            int x = GraphicsDevice.Viewport.Width / 2 - width / 2;
            return new Rectangle(x, y, width, height);
        }

        private void LoadItemTextures()
        {
            var dataShardTexture = Content.Load<Texture2D>("items/data_shard");
            var repairPartTexture = Content.Load<Texture2D>("items/parts");
            var chargeItemTexture = Content.Load<Texture2D>("items/charge");
            
            _dataShards = new DataShard[MAX_SHARDS];
            _repairParts = new RepairPart[MAX_PARTS];
            _chargeItems = new ChargeItem[MAX_CHARGES];
            
            for (int i = 0; i < _dataShards.Length; i++)
            {
                _dataShards[i].Texture = dataShardTexture;
            }
            for (int i = 0; i < _repairParts.Length; i++)
            {
                _repairParts[i].Texture = repairPartTexture;
            }
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
            string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            foreach (char c in chars)
            {
                _fontTextures[c] = Content.Load<Texture2D>($"font/{c}");
                if (char.IsLetter(c))
                {
                    _fontTextures[char.ToUpper(c)] = Content.Load<Texture2D>($"font/{c}");
                }
            }
            _fontBackgroundTexture = Content.Load<Texture2D>("font/background");
        }

        private void ResetGame()
        {
            _droneWorldPosition = new Vector2(MAP_WIDTH_TILES * TILE_SIZE / 2, MAP_HEIGHT_TILES * TILE_SIZE / 2);
            _dronePosition = _droneWorldPosition * _gameScale;
            _droneVelocity = Vector2.Zero;
            _playerHP = 100;
            _playerCharge = 100;
            _chargeTimer = 0f;
            
            _enemies[0].WorldPosition = new Vector2(_droneWorldPosition.X, _droneWorldPosition.Y - 160);
            _enemies[0].Position = _enemies[0].WorldPosition * _gameScale;
            _enemies[0].Direction = 1;
            _enemies[0].Active = true;
            _enemies[0].Speed = 80f;
            
            _enemies[1].WorldPosition = new Vector2(_droneWorldPosition.X + 160, _droneWorldPosition.Y);
            _enemies[1].Position = _enemies[1].WorldPosition * _gameScale;
            _enemies[1].Direction = 1;
            _enemies[1].Active = true;
            _enemies[1].Speed = 60f;
            
            _enemies[2].WorldPosition = new Vector2(_droneWorldPosition.X - 160, _droneWorldPosition.Y);
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
            int shardCount = _random.Next(MIN_SHARDS, MAX_SHARDS + 1);
            float minX = TILE_SIZE;
            float minY = TILE_SIZE;
            float maxX = MAP_WIDTH_TILES * TILE_SIZE;
            float maxY = MAP_HEIGHT_TILES * TILE_SIZE;

            for (int i = 0; i < _dataShards.Length; i++)
            {
                if (i < shardCount)
                {
                    _dataShards[i].Active = true;
                    _dataShards[i].WorldPosition = new Vector2(_random.Next((int)minX, (int)maxX), _random.Next((int)minY, (int)maxY));
                    _dataShards[i].Position = _dataShards[i].WorldPosition * _gameScale;
                    int width = _dataShards[i].Texture != null ? (int)(_dataShards[i].Texture.Width * 0.7f) : 22;
                    int height = _dataShards[i].Texture != null ? (int)(_dataShards[i].Texture.Height * 0.7f) : 22;
                    _dataShards[i].Hitbox = new Rectangle((int)_dataShards[i].WorldPosition.X, (int)(_dataShards[i].WorldPosition.Y + _dataShards[i].HoverOffset), width, height);
                }
                else
                {
                    _dataShards[i].Active = false;
                }
            }
        }

        private void InitializeRepairParts()
        {
            int partCount = _random.Next(MIN_PARTS, MAX_PARTS + 1);
            float minX = TILE_SIZE;
            float minY = TILE_SIZE;
            float maxX = MAP_WIDTH_TILES * TILE_SIZE;
            float maxY = MAP_HEIGHT_TILES * TILE_SIZE;

            for (int i = 0; i < _repairParts.Length; i++)
            {
                if (i < partCount)
                {
                    _repairParts[i].Active = true;
                    _repairParts[i].WorldPosition = new Vector2(_random.Next((int)minX, (int)maxX), _random.Next((int)minY, (int)maxY));
                    _repairParts[i].Position = _repairParts[i].WorldPosition * _gameScale;
                    int width = _repairParts[i].Texture != null ? (int)(_repairParts[i].Texture.Width * 0.7f) : 22;
                    int height = _repairParts[i].Texture != null ? (int)(_repairParts[i].Texture.Height * 0.7f) : 22;
                    _repairParts[i].Hitbox = new Rectangle((int)_repairParts[i].WorldPosition.X, (int)(_repairParts[i].WorldPosition.Y + _repairParts[i].HoverOffset), width, height);
                }
                else
                {
                    _repairParts[i].Active = false;
                }
            }
        }

        private void InitializeChargeItems()
        {
            int chargeCount = _random.Next(MIN_CHARGES, MAX_CHARGES + 1);
            float minX = TILE_SIZE;
            float minY = TILE_SIZE;
            float maxX = MAP_WIDTH_TILES * TILE_SIZE;
            float maxY = MAP_HEIGHT_TILES * TILE_SIZE;

            for (int i = 0; i < _chargeItems.Length; i++)
            {
                if (i < chargeCount)
                {
                    _chargeItems[i].Active = true;
                    _chargeItems[i].WorldPosition = new Vector2(_random.Next((int)minX, (int)maxX), _random.Next((int)minY, (int)maxY));
                    _chargeItems[i].Position = _chargeItems[i].WorldPosition * _gameScale;
                    int width = _chargeItems[i].Texture != null ? (int)(_chargeItems[i].Texture.Width * 0.7f) : 22;
                    int height = _chargeItems[i].Texture != null ? (int)(_chargeItems[i].Texture.Height * 0.7f) : 22;
                    _chargeItems[i].Hitbox = new Rectangle((int)_chargeItems[i].WorldPosition.X, (int)(_chargeItems[i].WorldPosition.Y + _chargeItems[i].HoverOffset), width, height);
                }
                else
                {
                    _chargeItems[i].Active = false;
                }
            }
        }

        private void OnWindowSizeChanged(object sender, EventArgs e)
        {
            if (_startScreenButtons != null)
            {
                _startScreenButtons[0].Bounds = CalculateButtonBounds("START", 2.0f, GraphicsDevice.Viewport.Height / 2 + 50);
                _startScreenButtons[1].Bounds = CalculateButtonBounds("EXIT", 2.0f, GraphicsDevice.Viewport.Height / 2 + 130);
            }
            if (_pauseScreenButtons != null)
            {
                _pauseScreenButtons[0].Bounds = CalculateButtonBounds("RESUME", 2.0f, GraphicsDevice.Viewport.Height / 2 + 50);
                _pauseScreenButtons[1].Bounds = CalculateButtonBounds("RESTART", 2.0f, GraphicsDevice.Viewport.Height / 2 + 130);
                _pauseScreenButtons[2].Bounds = CalculateButtonBounds("EXIT", 2.0f, GraphicsDevice.Viewport.Height / 2 + 210);
            }
            if (_deathScreenButtons != null)
            {
                _deathScreenButtons[0].Bounds = CalculateButtonBounds("RESTART", 2.0f, GraphicsDevice.Viewport.Height / 2 + 50);
                _deathScreenButtons[1].Bounds = CalculateButtonBounds("EXIT", 2.0f, GraphicsDevice.Viewport.Height / 2 + 130);
            }
        }
        #endregion

        #region Update
        protected override void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            HandleDebugInput(keyboard);
            HandleGameStateInput(keyboard);
            
            if (_isGameStarted && !_isPlayerDead && !_isGamePaused)
            {
                UpdatePlayerMovement(keyboard, delta);
                UpdateEnemyMovement(delta);
                UpdateCollision();
                CheckEnemyCollisions();
                
                if (_damageFlashTimer > 0)
                {
                    _damageFlashTimer -= delta;
                }
                
                if (!_editMode)
                {
                    _chargeTimer += delta;
                    if (_chargeTimer >= 1.0f)
                    {
                        _playerCharge--;
                        _chargeTimer -= 1.0f;
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
            _previousMouseState = mouse;

            base.Update(gameTime);
        }

        private void HandleGameStateInput(KeyboardState keyboard)
        {
            if (!IsActive) return;
            MouseState mouse = Mouse.GetState();
            Point mousePoint = new Point(mouse.X, mouse.Y);
            
            if (!_isGameStarted)
            {
                if (mouse.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
                {
                    if (_startScreenButtons[0].Contains(mousePoint))
                    {
                        _isGameStarted = true;
                        ResetGame();
                        return;
                    }
                    if (_startScreenButtons[1].Contains(mousePoint))
                    {
                        Exit();
                        return;
                    }
                }
                return;
            }
            
            if (_isPlayerDead)
            {
                if (mouse.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
                {
                    if (_deathScreenButtons[0].Contains(mousePoint))
                    {
                        ResetGame();
                        _isPlayerDead = false;
                        return;
                    }
                    if (_deathScreenButtons[1].Contains(mousePoint))
                    {
                        Exit();
                        return;
                    }
                }
                return;
            }
            
            if (_isGameStarted && !_isPlayerDead && keyboard.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape))
            {
                _isGamePaused = !_isGamePaused;
                return;
            }
            
            if (_isGamePaused)
            {
                if (mouse.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
                {
                    if (_pauseScreenButtons[0].Contains(mousePoint))
                    {
                        _isGamePaused = false;
                        return;
                    }
                    if (_pauseScreenButtons[1].Contains(mousePoint))
                    {
                        ResetGame();
                        _isGamePaused = false;
                        return;
                    }
                    if (_pauseScreenButtons[2].Contains(mousePoint))
                    {
                        Exit();
                        return;
                    }
                }
            }
        }

        private void HandleDebugInput(KeyboardState keyboard)
        {
            MouseState mouse = Mouse.GetState();
            int currentScrollValue = mouse.ScrollWheelValue;
            
            if (currentScrollValue != _previousScrollWheelValue)
            {
                int scrollChange = currentScrollValue - _previousScrollWheelValue;
                
                float zoomChange = scrollChange * 0.001f;
                _gameScale += zoomChange;
                
                _gameScale = Math.Max(0.5f, _gameScale);
                
                _gameScale = Math.Min(_gameScale, 5.0f);
                
                _previousScrollWheelValue = currentScrollValue;
            }

            if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.LeftShift) && keyboard.IsKeyDown(Keys.C) &&
                !(_previousKeyboardState.IsKeyDown(Keys.LeftControl) && _previousKeyboardState.IsKeyDown(Keys.LeftShift) && _previousKeyboardState.IsKeyDown(Keys.C)))
            {
                _editMode = !_editMode;
                _showHitboxes = !_showHitboxes;
            }
            if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.LeftShift) && keyboard.IsKeyDown(Keys.K) &&
                !(_previousKeyboardState.IsKeyDown(Keys.LeftControl) && _previousKeyboardState.IsKeyDown(Keys.LeftShift) && _previousKeyboardState.IsKeyDown(Keys.K)))
            {
                _playerHP = 0;
                _isPlayerDead = true;
                _explosionPosition = _droneWorldPosition * _gameScale;
                _explosionActive = true;
                _currentExplosionFrame = 0;
                _frameTimer = 0f;
            }
        }

        private void HandleEditModeInput()
        {
            MouseState mouse = Mouse.GetState();
            Vector2 screenPosition = new Vector2(mouse.X, mouse.Y);
            Vector2 worldPosition = Vector2.Transform(screenPosition, Matrix.Invert(_viewMatrix));
            
            float minX = TILE_SIZE;
            float minY = TILE_SIZE;
            float maxX = MAP_WIDTH_TILES * TILE_SIZE;
            float maxY = MAP_HEIGHT_TILES * TILE_SIZE;
            
            if (mouse.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released)
            {
                for (int i = 0; i < _enemies.Length; i++)
                {
                    if (_enemies[i].Active && _enemies[i].Hitbox.Contains((int)worldPosition.X, (int)worldPosition.Y))
                    {
                        _enemies[i].Active = false;
                        return;
                    }
                }
                for (int i = 0; i < _dataShards.Length; i++)
                {
                    if (_dataShards[i].Active && _dataShards[i].Hitbox.Contains((int)worldPosition.X, (int)worldPosition.Y))
                    {
                        _dataShards[i].Active = false;
                        return;
                    }
                }
                for (int i = 0; i < _repairParts.Length; i++)
                {
                    if (_repairParts[i].Active && _repairParts[i].Hitbox.Contains((int)worldPosition.X, (int)worldPosition.Y))
                    {
                        _repairParts[i].Active = false;
                        return;
                    }
                }
                for (int i = 0; i < _chargeItems.Length; i++)
                {
                    if (_chargeItems[i].Active && _chargeItems[i].Hitbox.Contains((int)worldPosition.X, (int)worldPosition.Y))
                    {
                        _chargeItems[i].Active = false;
                        return;
                    }
                }
            }
            
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (!_isDragging && _previousMouseState.LeftButton == ButtonState.Released)
                {
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
                    Vector2 newPosition = worldPosition + _dragOffset;
                    newPosition.X = MathHelper.Clamp(newPosition.X, minX, maxX);
                    newPosition.Y = MathHelper.Clamp(newPosition.Y, minY, maxY);
                    
                    if (_draggedEnemyIndex >= 0)
                    {
                        _enemies[_draggedEnemyIndex].WorldPosition = newPosition;
                        _enemies[_draggedEnemyIndex].Position = _enemies[_draggedEnemyIndex].WorldPosition * _gameScale;
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

            Vector2 direction = Vector2.Zero;
            if (keyboard.IsKeyDown(Keys.A)) direction.X -= 1;
            if (keyboard.IsKeyDown(Keys.D)) direction.X += 1;
            if (keyboard.IsKeyDown(Keys.W)) direction.Y -= 1;
            if (keyboard.IsKeyDown(Keys.S)) direction.Y += 1;

            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                _droneVelocity += direction * acceleration * delta;
            }
            if (direction == Vector2.Zero || _droneVelocity.Length() > maxSpeed)
            {
                _droneVelocity -= _droneVelocity * friction * delta;
            }
            if (_droneVelocity.Length() > maxSpeed)
            {
                _droneVelocity.Normalize();
                _droneVelocity *= maxSpeed;
            }

            _droneWorldPosition += _droneVelocity * delta;
            float minX = TILE_SIZE;
            float minY = TILE_SIZE;
            float maxX = MAP_WIDTH_TILES * TILE_SIZE;
            float maxY = MAP_HEIGHT_TILES * TILE_SIZE;

            _droneWorldPosition.X = MathHelper.Clamp(_droneWorldPosition.X, minX, maxX);
            _droneWorldPosition.Y = MathHelper.Clamp(_droneWorldPosition.Y, minY, maxY);
            _dronePosition = _droneWorldPosition * _gameScale;

            _cameraPosition = _droneWorldPosition;
            _viewMatrix = Matrix.CreateScale(_gameScale) *
                Matrix.CreateTranslation(
                    -_cameraPosition.X * _gameScale + GraphicsDevice.Viewport.Width / 2,
                    -_cameraPosition.Y * _gameScale + GraphicsDevice.Viewport.Height / 2,
                    0);

            _playerHitbox = new Rectangle((int)_droneWorldPosition.X, (int)(_droneWorldPosition.Y + _hoverOffset), _droneTexture.Width, _droneTexture.Height);
        }

        private void UpdateEnemyMovement(float delta)
        {
            if (_editMode) return;

            float minX = TILE_SIZE;
            float minY = TILE_SIZE;
            float maxX = MAP_WIDTH_TILES * TILE_SIZE;
            float maxY = MAP_HEIGHT_TILES * TILE_SIZE;

            if (_enemies[0].Active)
            {
                _enemies[0].WorldPosition.X += _enemies[0].Speed * _enemies[0].Direction * delta;
                if (_enemies[0].WorldPosition.X <= minX)
                {
                    _enemies[0].WorldPosition.X = minX;
                    _enemies[0].Direction = 1;
                }
                else if (_enemies[0].WorldPosition.X >= maxX)
                {
                    _enemies[0].WorldPosition.X = maxX;
                    _enemies[0].Direction = -1;
                }
                _enemies[0].Position = _enemies[0].WorldPosition * _gameScale;
                _enemies[0].Hitbox = new Rectangle((int)_enemies[0].WorldPosition.X, (int)(_enemies[0].WorldPosition.Y + _enemies[0].HoverOffset), _enemies[0].Texture.Width, _enemies[0].Texture.Height);
            }
            if (_enemies[1].Active)
            {
                _enemies[1].WorldPosition.Y += _enemies[1].Speed * _enemies[1].Direction * delta;
                if (_enemies[1].WorldPosition.Y <= minY)
                {
                    _enemies[1].WorldPosition.Y = minY;
                    _enemies[1].Direction = 1;
                }
                else if (_enemies[1].WorldPosition.Y >= maxY)
                {
                    _enemies[1].WorldPosition.Y = maxY;
                    _enemies[1].Direction = -1;
                }
                _enemies[1].Position = _enemies[1].WorldPosition * _gameScale;
                _enemies[1].Hitbox = new Rectangle((int)_enemies[1].WorldPosition.X, (int)(_enemies[1].WorldPosition.Y + _enemies[1].HoverOffset), _enemies[1].Texture.Width, _enemies[1].Texture.Height);
            }
            if (_enemies[2].Active)
            {
                Vector2 toPlayer = _droneWorldPosition - _enemies[2].WorldPosition;
                if (toPlayer.Length() > 16)
                {
                    toPlayer.Normalize();
                    _enemies[2].WorldPosition += toPlayer * _enemies[2].Speed * delta;
                    _enemies[2].WorldPosition.X = MathHelper.Clamp(_enemies[2].WorldPosition.X, minX, maxX);
                    _enemies[2].WorldPosition.Y = MathHelper.Clamp(_enemies[2].WorldPosition.Y, minY, maxY);
                }
                _enemies[2].Position = _enemies[2].WorldPosition * _gameScale;
                _enemies[2].Hitbox = new Rectangle((int)_enemies[2].WorldPosition.X, (int)(_enemies[2].WorldPosition.Y + _enemies[2].HoverOffset), _enemies[2].Texture.Width, _enemies[2].Texture.Height);
            }
        }

        private void UpdateCollision()
        {
            for (int i = 0; i < _enemies.Length; i++)
            {
                if (_enemies[i].Active && _playerHitbox.Intersects(_enemies[i].Hitbox))
                {
                    _playerHP -= 25;
                    _damageFlashTimer = DAMAGE_FLASH_DURATION;
                    Vector2 collisionPosition = (_droneWorldPosition + _enemies[i].WorldPosition) / 2;
                    _explosionPosition = collisionPosition * _gameScale;
                    _explosionActive = true;
                    _currentExplosionFrame = 0;
                    _frameTimer = 0f;
                    if (_playerHP <= 0) _isPlayerDead = true;
                    _enemies[i].Active = false;
                    break;
                }
            }
            for (int i = 0; i < _dataShards.Length; i++)
            {
                if (_dataShards[i].Active && _playerHitbox.Intersects(_dataShards[i].Hitbox))
                {
                    _dataShards[i].Active = false;
                }
            }
            for (int i = 0; i < _repairParts.Length; i++)
            {
                if (_repairParts[i].Active && _playerHitbox.Intersects(_repairParts[i].Hitbox))
                {
                    _repairParts[i].Active = false;
                    _playerHP = Math.Min(_playerHP + 25, 100);
                }
            }
            for (int i = 0; i < _chargeItems.Length; i++)
            {
                if (_chargeItems[i].Active && _playerHitbox.Intersects(_chargeItems[i].Hitbox))
                {
                    _chargeItems[i].Active = false;
                    _playerCharge = Math.Min(_playerCharge + 25, 100);
                }
            }
        }

        private void CheckEnemyCollisions()
        {
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
            if (_explosionActive)
            {
                _frameTimer += delta;
                if (_frameTimer >= EXPLOSION_FRAME_TIME)
                {
                    _frameTimer = 0f;
                    _currentExplosionFrame++;
                    if (_currentExplosionFrame >= EXPLOSION_FRAME_COUNT) _explosionActive = false;
                }
            }
            if (_enemyExplosionActive)
            {
                _enemyExplosionTimer += delta;
                if (_enemyExplosionTimer >= EXPLOSION_FRAME_TIME)
                {
                    _enemyExplosionTimer = 0f;
                    _enemyExplosionFrame++;
                    if (_enemyExplosionFrame >= EXPLOSION_FRAME_COUNT) _enemyExplosionActive = false;
                }
            }
        }

        private void UpdateHoverEffects(GameTime gameTime)
        {
            double time = gameTime.TotalGameTime.TotalSeconds;
            _hoverOffset = (float)Math.Sin(time * 5f) * 15f;
            _enemies[0].HoverOffset = (float)Math.Sin(time * 2.5f) * 10f;
            _enemies[1].HoverOffset = (float)Math.Sin(time * 3.5f) * 12f;
            _enemies[2].HoverOffset = (float)Math.Sin(time * 4.5f) * 8f;
            
            if (_editMode)
            {
                for (int i = 0; i < _enemies.Length; i++)
                {
                    if (_enemies[i].Active)
                    {
                        _enemies[i].Hitbox = new Rectangle((int)_enemies[i].WorldPosition.X, (int)(_enemies[i].WorldPosition.Y + _enemies[i].HoverOffset), _enemies[i].Texture.Width, _enemies[i].Texture.Height);
                    }
                }
            }
            
            for (int i = 0; i < _dataShards.Length; i++)
            {
                if (_dataShards[i].Active)
                {
                    _dataShards[i].HoverOffset = (float)Math.Sin(time * 3f + i * 0.5f) * 6f;
                    _dataShards[i].Hitbox = new Rectangle((int)_dataShards[i].WorldPosition.X, (int)(_dataShards[i].WorldPosition.Y + _dataShards[i].HoverOffset), (int)(_dataShards[i].Texture.Width * 0.7f), (int)(_dataShards[i].Texture.Height * 0.7f));
                }
            }
            for (int i = 0; i < _repairParts.Length; i++)
            {
                if (_repairParts[i].Active)
                {
                    _repairParts[i].HoverOffset = (float)Math.Sin(time * 2.7f + i * 0.3f) * 8f;
                    _repairParts[i].Hitbox = new Rectangle((int)_repairParts[i].WorldPosition.X, (int)(_repairParts[i].WorldPosition.Y + _repairParts[i].HoverOffset), (int)(_repairParts[i].Texture.Width * 0.7f), (int)(_repairParts[i].Texture.Height * 0.7f));
                }
            }
            for (int i = 0; i < _chargeItems.Length; i++)
            {
                if (_chargeItems[i].Active)
                {
                    _chargeItems[i].HoverOffset = (float)Math.Sin(time * 3.3f + i * 0.7f) * 10f;
                    _chargeItems[i].Hitbox = new Rectangle((int)_chargeItems[i].WorldPosition.X, (int)(_chargeItems[i].WorldPosition.Y + _chargeItems[i].HoverOffset), (int)(_chargeItems[i].Texture.Width * 0.7f), (int)(_chargeItems[i].Texture.Height * 0.7f));
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
            
            DrawCursor();
            
            base.Draw(gameTime);
        }

        private void DrawStartScreen()
        {
            Color overlayColor = new Color(255, 165, 0, 255);
            Color titleBgColor = Color.Red;
            Color titleColor = Color.White;
            
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            DrawOverlay(overlayColor);
            DrawCenteredText("WRECK GAME", titleBgColor, titleBgColor, 3f, false, true, 8f, -100);
            DrawCenteredText("WRECK GAME", titleColor, Color.Transparent, 3f, false, false, 8f, -100);
            foreach (Button button in _startScreenButtons)
            {
                DrawButton(button);
            }
            _spriteBatch.End();
        }
        
        private void DrawPauseScreen()
        {
            Color overlayColor = new Color(0, 0, 255, 255);
            Color titleBgColor = Color.Red;
            Color titleColor = Color.White;
            
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            DrawOverlay(overlayColor);
            DrawCenteredText("PAUSED", titleBgColor, titleBgColor, 3f, false, true, 8f, -100);
            DrawCenteredText("PAUSED", titleColor, Color.Transparent, 3f, false, false, 8f, -100);
            foreach (Button button in _pauseScreenButtons)
            {
                DrawButton(button);
            }
            _spriteBatch.End();
        }

        private void DrawDeathScreen()
        {
            Color overlayColor = new Color(255, 0, 0, 255);
            Color titleBgColor = Color.Red;
            Color titleColor = Color.White;

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            DrawOverlay(overlayColor);
            DrawCenteredText("GAME OVER", titleBgColor, titleBgColor, 3f, false, true, 8f, -100);
            DrawCenteredText("GAME OVER", titleColor, Color.Transparent, 3f, false, false, 8f, -100);
            foreach (Button button in _deathScreenButtons)
            {
                DrawButton(button);
            }
            _spriteBatch.End();
        }

        private void DrawGame()
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _viewMatrix);
            DrawMap();
            DrawCharacters();
            DrawDataShards();
            DrawRepairParts();
            DrawChargeItems();
            DrawExplosions();
            DrawDebugInfo();
            _spriteBatch.End();
            
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            DrawStatusBars();
            if (_damageFlashTimer > 0)
            {
                float alpha = _damageFlashTimer / DAMAGE_FLASH_DURATION;
                int width = GraphicsDevice.Viewport.Width;
                int height = GraphicsDevice.Viewport.Height;
                Color outerColor = new Color(1f, 0f, 0f, alpha * 0.6f);
                DrawVignetteRectangle(0, 0, width, height, outerColor);
            }
            _spriteBatch.End();
        }

        private void DrawVignetteRectangle(int x, int y, int width, int height, Color color)
        {
            int thickness = 80;
            _spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, width, thickness), color);
            _spriteBatch.Draw(_pixelTexture, new Rectangle(x, y + height - thickness, width, thickness), color);
            _spriteBatch.Draw(_pixelTexture, new Rectangle(x, y + thickness, thickness, height - 2 * thickness), color);
            _spriteBatch.Draw(_pixelTexture, new Rectangle(x + width - thickness, y + thickness, thickness, height - 2 * thickness), color);
        }

        private void DrawRepairParts()
        {
            for (int i = 0; i < _repairParts.Length; i++)
            {
                if (_repairParts[i].Active)
                {
                    _spriteBatch.Draw(_repairParts[i].Texture, new Vector2(_repairParts[i].WorldPosition.X, _repairParts[i].WorldPosition.Y + _repairParts[i].HoverOffset), null, Color.White, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
                }
            }
        }

        private void DrawChargeItems()
        {
            for (int i = 0; i < _chargeItems.Length; i++)
            {
                if (_chargeItems[i].Active)
                {
                    _spriteBatch.Draw(_chargeItems[i].Texture, new Vector2(_chargeItems[i].WorldPosition.X, _chargeItems[i].WorldPosition.Y + _chargeItems[i].HoverOffset), null, Color.White, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
                }
            }
        }

        private void DrawStatusBars()
        {
            float barWidth = 200;
            float barHeight = 20;
            float margin = 10;
            float padding = 2;
            int screenX = (int)margin;
            int screenY = (int)(GraphicsDevice.Viewport.Height - margin - (barHeight * 2 + padding));
            
            _spriteBatch.Draw(_pixelTexture, new Rectangle(screenX, screenY, (int)barWidth, (int)barHeight), Color.DarkRed);
            _spriteBatch.Draw(_pixelTexture, new Rectangle(screenX, screenY, (int)(barWidth * _playerHP / 100), (int)barHeight), Color.Red);
            _spriteBatch.Draw(_pixelTexture, new Rectangle(screenX, screenY + (int)(barHeight + padding), (int)barWidth, (int)barHeight), Color.DarkGreen);
            _spriteBatch.Draw(_pixelTexture, new Rectangle(screenX, screenY + (int)(barHeight + padding), (int)(barWidth * _playerCharge / 100), (int)barHeight), Color.Green);
        }

        private void DrawMap()
        {
            for (int y = 0; y < MAP_HEIGHT_TILES + 2; y++)
            {
                for (int x = 0; x < MAP_WIDTH_TILES + 2; x++)
                {
                    Texture2D tileTexture = (x == 0 || y == 0 || x == MAP_WIDTH_TILES + 1 || y == MAP_HEIGHT_TILES + 1) ? _borderTexture : _asphaltTexture;
                    Vector2 tileWorldPosition = new(x * TILE_SIZE, y * TILE_SIZE);
                    _spriteBatch.Draw(tileTexture, tileWorldPosition, null, Color.White, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0f);
                }
            }
        }

        private void DrawCharacters()
        {
            if (!_isPlayerDead)
            {
                _spriteBatch.Draw(_droneTexture, new Vector2(_droneWorldPosition.X, _droneWorldPosition.Y + _hoverOffset), null, Color.White, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0f);
            }
            for (int i = 0; i < _enemies.Length; i++)
            {
                if (_enemies[i].Active)
                {
                    SpriteEffects effect = SpriteEffects.None;
                    if (i == 0 && _enemies[i].Direction == -1) effect = SpriteEffects.FlipHorizontally;
                    else if (i == 2)
                    {
                        Vector2 toPlayer = _droneWorldPosition - _enemies[i].WorldPosition;
                        if (toPlayer.X < 0) effect = SpriteEffects.FlipHorizontally;
                    }
                    _spriteBatch.Draw(_enemies[i].Texture, new Vector2(_enemies[i].WorldPosition.X, _enemies[i].WorldPosition.Y + _enemies[i].HoverOffset), null, Color.White, 0f, Vector2.Zero, 1.0f, effect, 0f);
                }
            }
        }

        private void DrawDataShards()
        {
            for (int i = 0; i < _dataShards.Length; i++)
            {
                if (_dataShards[i].Active)
                {
                    _spriteBatch.Draw(_dataShards[i].Texture, new Vector2(_dataShards[i].WorldPosition.X, _dataShards[i].WorldPosition.Y + _dataShards[i].HoverOffset), null, Color.White, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
                }
            }
        }

        private void DrawExplosions()
        {
            if (_explosionActive && _currentExplosionFrame < EXPLOSION_FRAME_COUNT)
            {
                DrawExplosion(_explosionTextures[_currentExplosionFrame], _explosionPosition);
            }
            if (_enemyExplosionActive && _enemyExplosionFrame < EXPLOSION_FRAME_COUNT)
            {
                DrawExplosion(_explosionTextures[_enemyExplosionFrame], _enemyExplosionPosition);
            }
        }
        
        private void DrawExplosion(Texture2D texture, Vector2 position)
        {
            _spriteBatch.Draw(texture, position / _gameScale, null, Color.White, 0f, new Vector2(texture.Width / 2, texture.Height / 2), 1.5f, SpriteEffects.None, 0);
        }

        private void DrawDebugInfo()
        {
            if (_showHitboxes)
            {
                DrawRectangleOutline(_playerHitbox, Color.White, 1);
                for (int i = 0; i < _enemies.Length; i++)
                {
                    if (_enemies[i].Active)
                    {
                        DrawRectangleOutline(_enemies[i].Hitbox, Color.White, 1);
                    }
                }

                for (int i = 0; i < _dataShards.Length; i++)
                {
                    if (_dataShards[i].Active)
                    {
                        DrawRectangleOutline(_dataShards[i].Hitbox, Color.Green, 1);
                    }
                }
                for (int i = 0; i < _repairParts.Length; i++)
                {
                    if (_repairParts[i].Active)
                    {
                        DrawRectangleOutline(_repairParts[i].Hitbox, Color.Green, 1);
                    }
                }
                for (int i = 0; i < _chargeItems.Length; i++)
                {
                    if (_chargeItems[i].Active)
                    {
                        DrawRectangleOutline(_chargeItems[i].Hitbox, Color.Green, 1);
                    }
                }
            }
        }

        private void DrawRectangleOutline(Rectangle rect, Color color, int thickness = 1)
        {
            _spriteBatch.Draw(_pixelTexture, new Rectangle(rect.Left, rect.Top, rect.Width, thickness), color);
            _spriteBatch.Draw(_pixelTexture, new Rectangle(rect.Left, rect.Bottom - thickness, rect.Width, thickness), color);
            _spriteBatch.Draw(_pixelTexture, new Rectangle(rect.Left, rect.Top, thickness, rect.Height), color);
            _spriteBatch.Draw(_pixelTexture, new Rectangle(rect.Right - thickness, rect.Top, thickness, rect.Height), color);
        }
        
        private void DrawOverlay(Color color)
        {
            _spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), null, color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
        }
        
        private void DrawColoredText(string text, Vector2 position, Color textColor, Color backgroundColor, float scale, bool beginBatch = true, bool showBackground = false, float letterSpacing = 0f)
        {
            float baseSpacing = 32 * scale;
            float spacing = baseSpacing + letterSpacing * scale;
            Vector2 pos = position;

            if (beginBatch)
            {
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
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
                        float backgroundScale = scale * 1.3f;
                        Vector2 backgroundPosition = new Vector2(
                            pos.X - ((_fontBackgroundTexture.Width * backgroundScale - letterTexture.Width * scale) / 2),
                            pos.Y - ((_fontBackgroundTexture.Height * backgroundScale - letterTexture.Height * scale) / 2)
                        );
                        _spriteBatch.Draw(_fontBackgroundTexture, backgroundPosition, null, backgroundColor, 0f, Vector2.Zero, backgroundScale, SpriteEffects.None, 0f);
                    }
                    _spriteBatch.Draw(letterTexture, pos, null, textColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
                pos.X += spacing;
            }

            if (beginBatch)
            {
                _spriteBatch.End();
            }
        }

        private void DrawCenteredText(string text, Color textColor, Color backgroundColor, float scale, bool beginBatch = true, bool showBackground = false, float letterSpacing = 0f, float yOffset = 0)
        {
            float baseSpacing = 32 * scale;
            float spacing = baseSpacing + letterSpacing * scale;
            float textWidth = text.Length * spacing;
            Vector2 position = new((GraphicsDevice.Viewport.Width - textWidth) / 2, (GraphicsDevice.Viewport.Height - 32 * scale) / 2 + yOffset);
            DrawColoredText(text, position, textColor, backgroundColor, scale, beginBatch, showBackground, letterSpacing);
        }

        private void DrawButton(Button button)
        {
            MouseState mouse = Mouse.GetState();
            Point mousePoint = new Point(mouse.X, mouse.Y);
            bool isHovered = button.Contains(mousePoint);
            
            Color textColor = isHovered 
                ? new Color((int)(button.TextColor.R * 0.8f), (int)(button.TextColor.G * 0.8f), (int)(button.TextColor.B * 0.8f), button.TextColor.A) 
                : button.TextColor;
            
            Color backgroundColor = isHovered ? Color.DarkGreen : Color.Transparent;
            
            float letterSpacing = 8.0f;
            
            Vector2 textSize = MeasureText(button.Text, button.Scale, letterSpacing);
            Vector2 textPos = new Vector2(button.Bounds.X + (button.Bounds.Width - textSize.X) / 2, button.Bounds.Y + (button.Bounds.Height - textSize.Y) / 2);
            
            DrawColoredText(button.Text, textPos, textColor, backgroundColor, button.Scale, false, isHovered, letterSpacing);
            
            if (_showHitboxes)
            {
                DrawRectangleOutline(button.Bounds, Color.Yellow, 2);
            }
        }

        private Vector2 MeasureText(string text, float scale, float letterSpacing = 0f)
        {
            float width = 0;
            float height = 32 * scale;
            
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == ' ')
                {
                    width += 32 * scale;
                }
                else if (_fontTextures.TryGetValue(text[i], out Texture2D texture))
                {
                    width += texture.Width * scale;
                }
                
                if (i < text.Length - 1)
                {
                    width += letterSpacing * scale;
                }
            }
            
            return new Vector2(width, height);
        }

        private readonly Vector2 _cursorHotspot = new Vector2(16, 16);
        private void DrawCursor()
        {
            MouseState mouse = Mouse.GetState();
            Vector2 position = new Vector2(mouse.X, mouse.Y);
            
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            _spriteBatch.Draw(_cursorTexture, position, null, _cursorColor, 0f, _cursorHotspot, 1.0f, SpriteEffects.None, 0f);
            _spriteBatch.End();
        }
        #endregion
    }
}