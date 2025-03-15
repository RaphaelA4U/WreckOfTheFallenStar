using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace WreckGame.Managers
{
    public class GraphicsManager
    {
        private ContentManager _content;
        private SpriteBatch _spriteBatch;
        private Dictionary<string, Texture2D> _textures;

        public GraphicsManager(ContentManager content, SpriteBatch spriteBatch)
        {
            _content = content;
            _spriteBatch = spriteBatch;
            _textures = new Dictionary<string, Texture2D>();
        }

        public Texture2D LoadTexture(string assetName)
        {
            if (!_textures.ContainsKey(assetName))
            {
                _textures[assetName] = _content.Load<Texture2D>(assetName);
            }
            return _textures[assetName];
        }

        public Texture2D CreateTexture(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(_spriteBatch.GraphicsDevice, width, height);
            Color[] colorData = new Color[width * height];
            for (int i = 0; i < colorData.Length; i++)
            {
                colorData[i] = color;
            }
            texture.SetData(colorData);
            return texture;
        }

        public SpriteBatch SpriteBatch => _spriteBatch;
    }
}