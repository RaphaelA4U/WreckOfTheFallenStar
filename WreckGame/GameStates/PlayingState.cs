using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WreckGame.Entities;
using WreckGame.Graphics;
using WreckGame.Input;
using WreckGame.World;

namespace WreckGame.GameStates
{
    public class PlayingState : GameState
    {
        private Player _player;
        private Enemy _enemy;
        private GameMap _gameMap;
        private Animation _explosionAnimation;
        private TextRenderer _textRenderer;
        private Camera _camera;
        private const float GAME_SCALE = 2.5f;
        private bool _isPlayerDead = false;
        private Texture2D _pixelTexture;
        private Dictionary<char, Texture2D> _letterTextures;
        private float _deathTimer = 0;
        private const float DEATH_TRANSITION_TIME = 3f;
        
        // Map constants
        private const int MAP_WIDTH_TILES = 50;
        private const int MAP_HEIGHT_TILES = 20;
        
        public PlayingState(Game1 game, InputManager inputManager) 
            : base(game, inputManager)
        {
        }

        public override void OnStateEnter()
        {
            ResetGame();
        }
        
        public override void Initialize()
        {
            _camera = new Camera(_game.GraphicsDevice.Viewport);
            base.Initialize();
        }
        
        public override void LoadContent()
        {
            // Load textures
            Texture2D asphaltTexture = _content.Load<Texture2D>("asphalt");
            Texture2D borderTexture = _content.Load<Texture2D>("border");
            Texture2D droneTexture = _content.Load<Texture2D>("drone");
            Texture2D enemyTexture = _content.Load<Texture2D>("drone_enemy");
            
            // Load explosion frames
            Texture2D[] explosionTextures = new Texture2D[3];
            explosionTextures[0] = _content.Load<Texture2D>("explosion1");
            explosionTextures[1] = _content.Load<Texture2D>("explosion2");
            explosionTextures[2] = _content.Load<Texture2D>("explosion3");
            
            // Create pixel texture
            _pixelTexture = new Texture2D(_game.GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            // Load font textures
            _letterTextures = new Dictionary<char, Texture2D>();
            for (char c = 'a'; c <= 'z'; c++)
            {
                _letterTextures[c] = _content.Load<Texture2D>(c.ToString());
            }
            for (char c = 'A'; c <= 'Z'; c++)
            {
                _letterTextures[c] = _content.Load<Texture2D>(c.ToString().ToLower());
            }
            
            // Create game objects
            int tileSize = 32;
            Vector2 playerStartPos = new Vector2(
                MAP_WIDTH_TILES * tileSize / 2, 
                MAP_HEIGHT_TILES * tileSize / 2);
                
            _player = new Player(droneTexture, playerStartPos, GAME_SCALE);
            
            // Enemy starts at a random position away from player
            Vector2 enemyStartPos = GetRandomEnemyPosition(playerStartPos);
            _enemy = new Enemy(enemyTexture, enemyStartPos, GAME_SCALE);
            
            _gameMap = new GameMap(asphaltTexture, borderTexture, MAP_WIDTH_TILES, MAP_HEIGHT_TILES, GAME_SCALE);
            _explosionAnimation = new Animation(explosionTextures, 0.1f, 4f);
            _textRenderer = new TextRenderer(_letterTextures);
            
            ResetGame();
        }
        
        private void ResetGame()
        {
            // Position player at the center of the map (0,0) in world space
            Vector2 playerStartPos = Vector2.Zero;
            
            _player.Reset(playerStartPos);
            
            // Position enemy away from player
            Vector2 enemyStartPos = GetRandomEnemyPosition(playerStartPos);
            _enemy.Reset(enemyStartPos);
            
            _isPlayerDead = false;
            _deathTimer = 0;
        }

        private Vector2 GetRandomEnemyPosition(Vector2 playerPos)
        {
            Random random = new Random();
            int tileSize = 32;
            
            // Calculate map boundaries in world coordinates
            int mapWidthPixels = (MAP_WIDTH_TILES + 2) * tileSize;
            int mapHeightPixels = (MAP_HEIGHT_TILES + 2) * tileSize;
            float startX = -mapWidthPixels / 2;
            float startY = -mapHeightPixels / 2;
            
            float worldWidth = MAP_WIDTH_TILES * tileSize;
            float worldHeight = MAP_HEIGHT_TILES * tileSize;
            
            // Border buffer
            int borderBuffer = 2;
            
            // Calculate min/max coordinates in world space
            float minX = startX + (borderBuffer * tileSize);
            float maxX = startX + ((MAP_WIDTH_TILES + 2 - borderBuffer) * tileSize);
            float minY = startY + (borderBuffer * tileSize);
            float maxY = startY + ((MAP_HEIGHT_TILES + 2 - borderBuffer) * tileSize);
            
            // Generate random position within bounds
            float enemyX, enemyY;
            do {
                enemyX = random.Next((int)minX, (int)maxX);
                enemyY = random.Next((int)minY, (int)maxY);
            } while (Vector2.Distance(new Vector2(enemyX, enemyY), playerPos) < 128);
            
            return new Vector2(enemyX, enemyY);
        }
        
        public override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update player
            Vector2 movementDirection = _inputManager.GetMovementDirection();
            _player.HandleInput(_inputManager, delta);
            _player.Update(gameTime);
            _player.ConstrainToMap(MAP_WIDTH_TILES, MAP_HEIGHT_TILES, 16);
            
            // Make camera follow player - ADD THIS LINE
            _camera.SetPosition(_player.WorldPosition);
            
            // Update enemy
            _enemy.Update(gameTime);

            if (_isPlayerDead)
            {
                _deathTimer += delta;
                if (_deathTimer >= DEATH_TRANSITION_TIME)
                {
                    _game.StateManager.SwitchState<GameOverState>();
                }
            }

            // Check for reset
            if (_inputManager.IsKeyDown(Keys.LeftControl) && 
                _inputManager.IsKeyDown(Keys.LeftShift) && 
                _inputManager.IsKeyPressed(Keys.R))
            {
                ResetGame();
            }
            
            if (!_isPlayerDead)
            {
                // Update player
                movementDirection = _inputManager.GetMovementDirection();
                _player.HandleInput(_inputManager, delta);
                _player.Update(gameTime);
                _player.ConstrainToMap(MAP_WIDTH_TILES, MAP_HEIGHT_TILES, 16);
                
                // Update camera
                _camera.SetPosition(_player.WorldPosition);

                // Update enemy
                _enemy.Update(gameTime);
                
                // Check for collision
                if (_player.Collides(_enemy))
                {
                    _isPlayerDead = true;
                    _deathTimer = 0;
                    
                    Vector2 midPoint = (_player.ScreenPosition + _enemy.ScreenPosition) / 2;
                    _explosionAnimation.Start(midPoint);
                }
            }
            else
            {
                _explosionAnimation.Update(delta);
                
                // Check for restart key
                if (_inputManager.IsKeyPressed(Keys.Space) || _inputManager.IsKeyPressed(Keys.Enter))
                {
                    ResetGame();
                }
            }
        }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            int viewportWidth = _game.GraphicsDevice.Viewport.Width;
            int viewportHeight = _game.GraphicsDevice.Viewport.Height;
            
            // Camera-transformed world rendering
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, _camera.TransformMatrix);
            
            // Draw the map and entities
            _gameMap.Draw(spriteBatch);
            _player.Draw(spriteBatch, Vector2.Zero);
            _enemy.Draw(spriteBatch, Vector2.Zero);
            
            spriteBatch.End();
            
            // UI rendering without camera transform
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
            
            // Handle death state UI
            if (_isPlayerDead)
            {
                // Draw red overlay
                spriteBatch.Draw(
                    _pixelTexture,
                    new Rectangle(0, 0, viewportWidth, viewportHeight),
                    null,
                    new Color(255, 0, 0, 150),
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0f);
                
                // Draw death text and restart instructions
                string text = "DEATH";
                float textScale = 4f;
                
                float totalTextWidth = _textRenderer.GetTextWidth(text, textScale);
                
                Vector2 textPosition = new Vector2(
                    viewportWidth / 2 - totalTextWidth / 2,
                    viewportHeight / 2 - (32 * textScale) / 2);
                
                _textRenderer.DrawText(spriteBatch, text, textPosition, Color.White, textScale);
                
                // Draw restart instruction
                string restartText = "PRESS SPACE TO RESTART";
                float restartTextScale = 1.5f;
                
                float restartTextWidth = _textRenderer.GetTextWidth(restartText, restartTextScale);
                
                Vector2 restartTextPosition = new Vector2(
                    viewportWidth / 2 - restartTextWidth / 2,
                    viewportHeight / 2 + 50);
                
                _textRenderer.DrawText(spriteBatch, restartText, restartTextPosition, Color.White, restartTextScale);
            }
            
            spriteBatch.End();
        }

        public void UpdateCameraViewport(Viewport viewport)
        {
            if (_camera != null)
            {
                _camera.UpdateViewport(viewport);
            }
        }
    }
}