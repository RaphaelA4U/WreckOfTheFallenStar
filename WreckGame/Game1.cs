using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WreckGame.Managers;
using WreckGame.States;
using WreckGame.Utilities;

namespace WreckGame
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private readonly InputManager _inputManager;
        private GraphicsManager _graphicsManager;
        private GameState _currentState;
        private Texture2D _cursorTexture;
        private readonly Vector2 _cursorHotspot = new Vector2(16, 16);
        private readonly Color _cursorColor = Color.DarkSlateGray;
        public readonly Point ReferenceResolution = new Point(1920, 1080);
        public float GameScale = 1f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            Window.AllowUserResizing = true;
            _inputManager = new InputManager();
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _graphicsManager = new GraphicsManager(Content, _spriteBatch);
            
            base.Initialize();
            
            Utilities.Utilities.Initialize(Content, GraphicsDevice);
            _currentState = new StartScreenState(this, _inputManager, _graphicsManager);
        }

        protected override void LoadContent()
        {
            _cursorTexture = Content.Load<Texture2D>("misc/cursor");
            _graphicsManager.LoadTexture("misc/pixel");
        }

        protected override void Update(GameTime gameTime)
        {
            _inputManager.Update();
            _currentState.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _currentState.Draw(gameTime);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            _spriteBatch.Draw(_cursorTexture, _inputManager.GetMousePosition().ToVector2(), null, _cursorColor, 0f, _cursorHotspot, 1.0f, SpriteEffects.None, 0f);
            _spriteBatch.End();
            base.Draw(gameTime);
        }

        public void SetState(GameState newState)
        {
            _currentState = newState;
        }
    }
}