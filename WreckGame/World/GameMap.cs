using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WreckGame.World
{
    public class GameMap
    {
        private Texture2D _asphaltTexture;
        private Texture2D _borderTexture;
        private int _mapWidth;
        private int _mapHeight;
        private float _gameScale;
        private const int TILE_SIZE = 32;
        
        public int WidthInTiles => _mapWidth;
        public int HeightInTiles => _mapHeight;
        
        public GameMap(Texture2D asphaltTexture, Texture2D borderTexture, int mapWidth, int mapHeight, float gameScale)
        {
            _asphaltTexture = asphaltTexture;
            _borderTexture = borderTexture;
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            _gameScale = gameScale;
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            // Calculate map dimensions in world space
            int mapWidthPixels = (_mapWidth + 2) * TILE_SIZE;
            int mapHeightPixels = (_mapHeight + 2) * TILE_SIZE;
            
            // Center the map at (0,0) in world space
            float startX = -mapWidthPixels / 2;
            float startY = -mapHeightPixels / 2;
            
            for (int y = 0; y < _mapHeight + 2; y++)
            {
                for (int x = 0; x < _mapWidth + 2; x++)
                {
                    Texture2D tileTexture = (x == 0 || y == 0 || x == _mapWidth + 1 || y == _mapHeight + 1) 
                        ? _borderTexture 
                        : _asphaltTexture;
                    
                    Vector2 position = new Vector2(
                        startX + (x * TILE_SIZE), 
                        startY + (y * TILE_SIZE)
                    );
                    
                    spriteBatch.Draw(
                        tileTexture,
                        position,
                        null,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        _gameScale,
                        SpriteEffects.None,
                        0f
                    );
                }
            }
        }
        
        public Vector2 GetMapOffset(int viewportWidth, int viewportHeight)
        {
            int scaledTileSize = (int)(TILE_SIZE * _gameScale);
            int mapWidthPixels = (_mapWidth + 2) * scaledTileSize;
            int mapHeightPixels = (_mapHeight + 2) * scaledTileSize;
            int startX = (viewportWidth - mapWidthPixels) / 2;
            int startY = (viewportHeight - mapHeightPixels) / 2;
            
            return new Vector2(startX, startY);
        }
    }
}