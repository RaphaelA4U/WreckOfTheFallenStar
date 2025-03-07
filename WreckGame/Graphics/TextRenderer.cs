using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WreckGame.Graphics
{
    public class TextRenderer
    {
        private Dictionary<char, Texture2D> _letterTextures;
        
        public TextRenderer(Dictionary<char, Texture2D> letterTextures)
        {
            _letterTextures = letterTextures;
        }
        
        public void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float scale)
        {
            float spacing = 32 * scale;
            Vector2 pos = position;
            
            foreach (char c in text)
            {
                if (_letterTextures.ContainsKey(c))
                {
                    Texture2D letterTexture = _letterTextures[c];
                    spriteBatch.Draw(
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
        
        public float GetTextWidth(string text, float scale)
        {
            return text.Length * 32 * scale;
        }
    }
}