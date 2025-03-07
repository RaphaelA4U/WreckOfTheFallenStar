using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WreckGame.Content
{
    public class GameContentManager
    {
        private ContentManager _contentManager;
        private GraphicsDevice _graphicsDevice;
        private Dictionary<string, Texture2D> _textureCache;
        
        public GameContentManager(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            _contentManager = contentManager;
            _graphicsDevice = graphicsDevice;
            _textureCache = new Dictionary<string, Texture2D>();
        }
        
        public Texture2D LoadTexture(string assetName)
        {
            if (!_textureCache.ContainsKey(assetName))
            {
                _textureCache[assetName] = _contentManager.Load<Texture2D>(assetName);
            }
            
            return _textureCache[assetName];
        }
        
        public Texture2D CreatePixelTexture()
        {
            Texture2D pixel = new Texture2D(_graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            return pixel;
        }
        
        public Dictionary<char, Texture2D> LoadFontTextures()
        {
            Dictionary<char, Texture2D> letterTextures = new Dictionary<char, Texture2D>();
            
            // Load lowercase letters
            for (char c = 'a'; c <= 'z'; c++)
            {
                letterTextures[c] = LoadTexture(c.ToString());
            }
            
            // Map uppercase to lowercase textures
            for (char c = 'A'; c <= 'Z'; c++)
            {
                letterTextures[c] = letterTextures[char.ToLower(c)];
            }
            
            return letterTextures;
        }
        
        public void UnloadContent()
        {
            _contentManager.Unload();
            _textureCache.Clear();
        }
    }
}