using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace WreckGame.Utilities
{
    public static class Utilities
    {
        private static Dictionary<char, Texture2D> _fontTextures;
        private static Texture2D _fontBackgroundTexture;
        private static Texture2D _pixelTexture;

        public static void Initialize(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _fontTextures = new Dictionary<char, Texture2D>();
            string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            foreach (char c in chars)
            {
                _fontTextures[c] = content.Load<Texture2D>($"font/{c}");
                if (char.IsLetter(c))
                {
                    _fontTextures[char.ToUpper(c)] = content.Load<Texture2D>($"font/{c}");
                }
            }
            _fontBackgroundTexture = content.Load<Texture2D>("font/background");
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        public static void DrawColoredText(SpriteBatch spriteBatch, string text, Vector2 position, Color textColor, Color backgroundColor, float scale, bool showBackground = false, float letterSpacing = 0f)
        {
            float baseSpacing = 32 * scale;
            float spacing = baseSpacing + letterSpacing * scale;
            Vector2 pos = position;

            foreach (char c in text)
            {
                if (c == ' ') { pos.X += spacing; continue; }
                if (_fontTextures.TryGetValue(c, out Texture2D letterTexture))
                {
                    if (showBackground)
                    {
                        float backgroundScale = scale * 1.3f;
                        Vector2 backgroundPosition = new Vector2(
                            pos.X - ((_fontBackgroundTexture.Width * backgroundScale - letterTexture.Width * scale) / 2),
                            pos.Y - ((_fontBackgroundTexture.Height * backgroundScale - letterTexture.Height * scale) / 2)
                        );
                        spriteBatch.Draw(_fontBackgroundTexture, backgroundPosition, null, backgroundColor, 0f, Vector2.Zero, backgroundScale, SpriteEffects.None, 0f);
                    }
                    spriteBatch.Draw(letterTexture, pos, null, textColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
                pos.X += spacing;
            }
        }

        public static Vector2 MeasureText(string text, float scale, float letterSpacing = 0f)
        {
            float baseSpacing = 32 * scale;
            float spacing = baseSpacing + letterSpacing * scale;
            float width = 0;
            foreach (char c in text)
            {
                if (c == ' ') width += baseSpacing;
                else if (_fontTextures.TryGetValue(c, out Texture2D texture)) width += texture.Width * scale;
                width += letterSpacing * scale;
            }
            if (text.Length > 0) width -= letterSpacing * scale; // Don't add letter spacing after the last letter
            return new Vector2(width, 32 * scale);
        }

        public static void DrawRectangleOutline(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness = 1)
        {
            spriteBatch.Draw(_pixelTexture, new Rectangle(rect.Left, rect.Top, rect.Width, thickness), color);
            spriteBatch.Draw(_pixelTexture, new Rectangle(rect.Left, rect.Bottom - thickness, rect.Width, thickness), color);
            spriteBatch.Draw(_pixelTexture, new Rectangle(rect.Left, rect.Top, thickness, rect.Height), color);
            spriteBatch.Draw(_pixelTexture, new Rectangle(rect.Right - thickness, rect.Top, thickness, rect.Height), color);
        }
    }
}