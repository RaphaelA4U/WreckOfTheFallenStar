using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WreckGame.GameStates;
using WreckGame.Input;

namespace WreckGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        
        // Game systems
        private InputManager _inputManager;
        private GameStateManager _stateManager;
        public GameStateManager StateManager => _stateManager;
        
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
            // Update camera viewport when window size changes
            PlayingState playingState = _stateManager.GetCurrentState() as PlayingState;
            if (playingState != null)
            {
                playingState.UpdateCameraViewport(GraphicsDevice.Viewport);
            }
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
            
            _inputManager = new InputManager();
            _stateManager = new GameStateManager();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Create game states
            var playingState = new PlayingState(this, _inputManager);
            var gameOverState = new GameOverState(this, _inputManager);
            
            // Initialize and load content for states
            playingState.Initialize();
            playingState.LoadContent();
            
            gameOverState.Initialize();
            gameOverState.LoadContent();
            
            // Register states with the state manager
            _stateManager.RegisterState(playingState);
            _stateManager.RegisterState(gameOverState);
            
            // Set the initial state
            _stateManager.SwitchState<PlayingState>();
        }

        protected override void Update(GameTime gameTime)
        {
            // Update input manager
            _inputManager.Update();
            
            // Update current state
            _stateManager.Update(gameTime);
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            _stateManager.Draw(_spriteBatch);
            
            base.Draw(gameTime);
        }
    }
}