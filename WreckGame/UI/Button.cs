using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WreckGame.Managers;
using WreckGame.Entities;
using WreckGame.States;

namespace WreckGame.UI
{
    public class Button : Entity
    {
        // In-game knop
        public Texture2D PressedTexture { get; set; }
        public bool IsPressed { get; set; } = false;
        public float PressedTimer { get; set; } = 0f;
        public bool ShowPrompt { get; set; } = false;

        // UI knop
        private readonly string _text;
        private readonly float _scale;
        private Rectangle _bounds;
        private readonly GraphicsManager _graphicsManager;

        // References for the abstract implementation
        private Player _player;
        private InputManager _inputManager;

        // Constructor voor in-game knop
        public Button(GraphicsManager graphicsManager, Vector2 position)
        {
            _graphicsManager = graphicsManager;
            Texture = _graphicsManager.LoadTexture("interactives/button");
            PressedTexture = _graphicsManager.LoadTexture("interactives/button_pressed");
            WorldPosition = position;
            Position = WorldPosition;
            Hitbox = new Rectangle((int)WorldPosition.X, (int)WorldPosition.Y, Texture.Width, Texture.Height);
        }

        // Constructor voor UI knop
        public Button(GraphicsManager graphicsManager, string text, float scale, int y)
        {
            _graphicsManager = graphicsManager;
            _text = text;
            _scale = scale;
            Vector2 textSize = Utilities.Utilities.MeasureText(text, scale, 8.0f);
            int width = (int)textSize.X;
            int height = 60;
            int x = _graphicsManager.SpriteBatch.GraphicsDevice.Viewport.Width / 2 - width / 2;
            _bounds = new Rectangle(x, y, width, height);
        }

        public void UpdatePosition()
        {
            if (_text == null) return;
            
            Vector2 textSize = Utilities.Utilities.MeasureText(_text, _scale, 8.0f);
            int width = (int)textSize.X;
            
            // Center horizontally
            int x = _graphicsManager.SpriteBatch.GraphicsDevice.Viewport.Width / 2 - width / 2;
            
            // Keep the same relative Y position (don't change the vertical position)
            _bounds = new Rectangle(x, _bounds.Y, width, _bounds.Height);
        }

        public void UpdateVerticalPosition(int y)
        {
            if (_text == null) return;
            
            // Keep the same width and height, just update Y position
            _bounds = new Rectangle(_bounds.X, y, _bounds.Width, _bounds.Height);
        }

        // Required implementation of abstract method from Entity
        public override void Update(GameTime gameTime)
        {
            // Empty implementation - use the other Update method instead
        }

        // This method preserves your existing functionality
        public void Update(GameTime gameTime, Player player, InputManager inputManager)
        {
            if (!Active) return;
            _player = player;  // Store for later use
            _inputManager = inputManager;  // Store for later use
            
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Always update the hitbox
            Hitbox = new Rectangle((int)WorldPosition.X, (int)(WorldPosition.Y + HoverOffset), Texture.Width, Texture.Height);
            
            // Skip interaction in edit mode
            if (GameState.EditMode) return;
            
            Vector2 playerToButton = WorldPosition - player.WorldPosition;
            ShowPrompt = playerToButton.Length() < 64f;

            if (IsPressed)
            {
                PressedTimer += delta;
                if (PressedTimer >= 1.0f)
                {
                    IsPressed = false;
                    PressedTimer = 0f;
                }
            }

            if (!IsPressed && ShowPrompt && (inputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.E) || (inputManager.IsLeftMousePressed() && Hitbox.Contains(inputManager.GetMousePosition()))))
            {
                IsPressed = true;
                PressedTimer = 0f;
                player.GameState.ActivateButtonNotification();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Active) return;
            Texture2D currentTexture = IsPressed ? PressedTexture : Texture;
            spriteBatch.Draw(currentTexture, new Vector2(WorldPosition.X, WorldPosition.Y), Color.White);

            if (GameState.EditMode)
            {
                Utilities.Utilities.DrawRectangleOutline(spriteBatch, Hitbox, Color.Purple, 2);
            }

            if (ShowPrompt && !IsPressed)
            {
                Vector2 promptPosition = new Vector2(WorldPosition.X + Texture.Width - 10, WorldPosition.Y + Texture.Height - 10);
                Texture2D eTexture = _graphicsManager.LoadTexture("font/E");
                float textScale = 0.4f;
                float backgroundScale = textScale * 1.3f;
                Vector2 backgroundPosition = new Vector2(
                    promptPosition.X - ((_graphicsManager.LoadTexture("font/background").Width * backgroundScale - eTexture.Width * textScale) / 2),
                    promptPosition.Y - ((_graphicsManager.LoadTexture("font/background").Height * backgroundScale - eTexture.Height * textScale) / 2)
                );
                spriteBatch.Draw(_graphicsManager.LoadTexture("font/background"), backgroundPosition, null, new Color(40, 40, 40), 0f, Vector2.Zero, backgroundScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(eTexture, promptPosition, null, Color.White, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
            }
        }

        // This is a different method with a different signature
        public void Draw(SpriteBatch spriteBatch, InputManager inputManager)
        {
            if (_text == null) return;

            UpdatePosition();

            Point mousePoint = inputManager.GetMousePosition();
            bool isHovered = _bounds.Contains(mousePoint);
            Color textColor = isHovered ? new Color(204, 204, 204) : Color.White;
            Color backgroundColor = isHovered ? Color.DarkGreen : Color.Transparent;
            Vector2 textSize = Utilities.Utilities.MeasureText(_text, _scale, 8.0f);
            Vector2 textPos = new Vector2(_bounds.X + (_bounds.Width - textSize.X) / 2, _bounds.Y + (_bounds.Height - textSize.Y) / 2);
            Utilities.Utilities.DrawColoredText(spriteBatch, _text, textPos, textColor, backgroundColor, _scale, isHovered, 8.0f);
        
            if (GameState.EditMode)
            {
                Utilities.Utilities.DrawRectangleOutline(spriteBatch, _bounds, Color.Yellow, 2);
            }

        }

        public bool Contains(Point point) => _bounds.Contains(point);
    }
}