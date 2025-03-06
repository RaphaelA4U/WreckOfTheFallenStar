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
        private float _gameScale = 2.5f;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _droneTexture;
        private Vector2 _dronePosition;
        private Vector2 _droneVelocity;
        private float _hoverOffset;
        
        private Texture2D _enemyDroneTexture;
        private Vector2 _enemyDronePosition;
        private float _enemyHoverOffset;

        private Vector2 _droneWorldPosition;
        private Vector2 _enemyWorldPosition;

        private Texture2D[] _explosionTextures;
        private int _explosionFrameCount = 3;
        private int _currentExplosionFrame = 0;
        private float _frameTime = 0.1f;
        private float _frameTimer = 0f;
        private bool _explosionActive = false;

        private KeyboardState _previousKeyboardState;

        private bool _isPlayerDead = false;
        private Texture2D _pixelTexture;
        private Vector2 _explosionPosition;

        private Dictionary<char, Texture2D> _letterTextures;

        private Texture2D _asphaltTexture;
        private Texture2D _borderTexture;

        private const int MAP_WIDTH_TILES = 50;
        private const int MAP_HEIGHT_TILES = 20;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            Window.AllowUserResizing = true;
            
            Window.ClientSizeChanged += OnWindowSizeChanged;
        }

        private void OnWindowSizeChanged(object sender, EventArgs e)
        {
            // Update any scale factors or position calculations that depend on screen size
            // This ensures everything adjusts when the window size changes
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
            
            ResetGame();
            
            base.Initialize();
        }

        private void ResetGame()
        {
            int tileSize = 32;
            
            _droneWorldPosition = new Vector2(
                MAP_WIDTH_TILES * tileSize / 2, 
                MAP_HEIGHT_TILES * tileSize / 2);
            _dronePosition = _droneWorldPosition * _gameScale;
            _droneVelocity = Vector2.Zero;
            
            Random random = new Random();
            
            int borderBuffer = 2;
            int minTileX = borderBuffer;
            int maxTileX = MAP_WIDTH_TILES - borderBuffer;
            int minTileY = borderBuffer;
            int maxTileY = MAP_HEIGHT_TILES - borderBuffer;
            
            int minX = minTileX * tileSize;
            int maxX = maxTileX * tileSize;
            int minY = minTileY * tileSize;
            int maxY = maxTileY * tileSize;
            
            int enemyX, enemyY;
            do {
                enemyX = random.Next(minX, maxX);
                enemyY = random.Next(minY, maxY);
            } while (Vector2.Distance(new Vector2(enemyX, enemyY), _droneWorldPosition) < 128 / _gameScale);
            
            _enemyWorldPosition = new Vector2(enemyX, enemyY);
            _enemyDronePosition = _enemyWorldPosition * _gameScale;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _asphaltTexture = Content.Load<Texture2D>("asphalt");
            _borderTexture = Content.Load<Texture2D>("border");
            _droneTexture = Content.Load<Texture2D>("drone");
            _enemyDroneTexture = Content.Load<Texture2D>("drone_enemy");
            
            _explosionTextures = new Texture2D[_explosionFrameCount];
            _explosionTextures[0] = Content.Load<Texture2D>("explosion1");
            _explosionTextures[1] = Content.Load<Texture2D>("explosion2"); 
            _explosionTextures[2] = Content.Load<Texture2D>("explosion3");

            _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            _letterTextures = new Dictionary<char, Texture2D>();
            for (char c = 'a'; c <= 'z'; c++)
            {
                _letterTextures[c] = Content.Load<Texture2D>(c.ToString());
            }
            for (char c = 'A'; c <= 'Z'; c++)
            {
                _letterTextures[c] = Content.Load<Texture2D>(c.ToString().ToLower());
            }
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.Add) && _previousKeyboardState.IsKeyUp(Keys.Add))
            {
                _gameScale += 0.5f;
                _dronePosition = _droneWorldPosition * _gameScale;
                _enemyDronePosition = _enemyWorldPosition * _gameScale;
            }
            if (keyboard.IsKeyDown(Keys.Subtract) && _previousKeyboardState.IsKeyUp(Keys.Subtract))
            {
                _gameScale = Math.Max(0.5f, _gameScale - 0.5f);
                _dronePosition = _droneWorldPosition * _gameScale;
                _enemyDronePosition = _enemyWorldPosition * _gameScale;
            }

            if (keyboard.IsKeyDown(Keys.LeftControl) && 
                keyboard.IsKeyDown(Keys.LeftShift) && 
                keyboard.IsKeyDown(Keys.R) &&
                _previousKeyboardState.IsKeyUp(Keys.R))
            {
                ResetGame();
                _isPlayerDead = false;
            }
            
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (!_isPlayerDead)
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
                _dronePosition = _droneWorldPosition * _gameScale;

                float bufferFromWall = 16;
                int tileSize = 32;
                float minX = tileSize + bufferFromWall;
                float minY = tileSize + bufferFromWall;
                float maxX = (MAP_WIDTH_TILES * tileSize) - bufferFromWall;
                float maxY = (MAP_HEIGHT_TILES * tileSize) - bufferFromWall;
                
                _droneWorldPosition.X = MathHelper.Clamp(_droneWorldPosition.X, minX, maxX);
                _droneWorldPosition.Y = MathHelper.Clamp(_droneWorldPosition.Y, minY, maxY);
                _dronePosition = _droneWorldPosition * _gameScale;
                
                Rectangle playerRect = new Rectangle(
                    (int)_dronePosition.X, 
                    (int)(_dronePosition.Y + _hoverOffset) + (int)(2.5 * _gameScale),
                    (int)(32 * _gameScale),                     
                    (int)((32 - 6) * _gameScale));              
                            
                Rectangle enemyRect = new Rectangle(
                    (int)_enemyDronePosition.X,
                    (int)(_enemyDronePosition.Y + _enemyHoverOffset) + (int)(2.5 * _gameScale),
                    (int)(32 * _gameScale),                      
                    (int)((32 - 6) * _gameScale));               
                    
                if (playerRect.Intersects(enemyRect))
                {
                    _isPlayerDead = true;
                    _explosionPosition = (_dronePosition + _enemyDronePosition) / 2;
                    _explosionActive = true;
                    _currentExplosionFrame = 0;
                    _frameTimer = 0f;
                }
            }
            else
            {
                if (_explosionActive)
                {
                    _frameTimer += delta;
                    if (_frameTimer >= _frameTime)
                    {
                        _frameTimer = 0f;
                        _currentExplosionFrame++;
                        
                        if (_currentExplosionFrame >= _explosionFrameCount)
                        {
                            _explosionActive = false;
                        }
                    }
                }
                
                _droneVelocity = Vector2.Zero;
            }
            
            _hoverOffset = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 5f) * 15f;
            _enemyHoverOffset = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 2.5f) * 10f;
            
            _previousKeyboardState = keyboard;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
            
            int tileSize = 32;
            int scaledTileSize = (int)(tileSize * _gameScale);
            
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;
            int mapWidthPixels = (MAP_WIDTH_TILES + 2) * scaledTileSize;
            int mapHeightPixels = (MAP_HEIGHT_TILES + 2) * scaledTileSize;
            int startX = (viewportWidth - mapWidthPixels) / 2;
            int startY = (viewportHeight - mapHeightPixels) / 2;
            
            for (int y = 0; y < MAP_HEIGHT_TILES + 2; y++)
            {
                for (int x = 0; x < MAP_WIDTH_TILES + 2; x++)
                {
                    Texture2D tileTexture;
                    if (x == 0 || y == 0 || x == MAP_WIDTH_TILES + 1 || y == MAP_HEIGHT_TILES + 1)
                    {
                        tileTexture = _borderTexture;
                    }
                    else
                    {
                        tileTexture = _asphaltTexture;
                    }
                    
                    _spriteBatch.Draw(
                        tileTexture,
                        new Vector2(startX + x * scaledTileSize, startY + y * scaledTileSize),
                        null,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        _gameScale,
                        SpriteEffects.None,
                        0f);
                }
            }
            
            _spriteBatch.Draw(_droneTexture, 
                new Vector2(startX, startY) + _dronePosition + new Vector2(0, _hoverOffset), 
                null, Color.White, 0f, Vector2.Zero, _gameScale, SpriteEffects.None, 0f);
                
            _spriteBatch.Draw(_enemyDroneTexture, 
                new Vector2(startX, startY) + _enemyDronePosition + new Vector2(0, _enemyHoverOffset), 
                null, Color.White, 0f, Vector2.Zero, _gameScale, SpriteEffects.None, 0f);
            
            if (_isPlayerDead)
            {
                _spriteBatch.Draw(
                    _pixelTexture,
                    new Rectangle(0, 0, viewportWidth, viewportHeight),
                    null,
                    new Color(255, 0, 0, 150),
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0f);
                    
                if (_explosionActive && _currentExplosionFrame < _explosionFrameCount)
                {
                    Texture2D currentTexture = _explosionTextures[_currentExplosionFrame];
                    
                    _spriteBatch.Draw(
                        currentTexture,
                        _explosionPosition,
                        null,
                        Color.White,
                        0f,
                        new Vector2(currentTexture.Width / 2, currentTexture.Height / 2),
                        4f,
                        SpriteEffects.None,
                        0);
                }
                
                string text = "DEATH";
                float textScale = 4f;
                
                float totalTextWidth = text.Length * 32 * textScale;
                
                Vector2 textPosition = new Vector2(
                    viewportWidth / 2 - totalTextWidth / 2,
                    viewportHeight / 2 - (32 * textScale) / 2);
                
                DrawText(text, textPosition, Color.White, textScale);
            }
            
            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawText(string text, Vector2 position, Color color, float scale)
        {
            float spacing = 32 * scale;
            Vector2 pos = position;
            
            foreach (char c in text)
            {
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
    }
}
