using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using WreckGame.Managers;
using System;

namespace WreckGame.Map
{
    public class Map
    {
        public const int MAP_WIDTH_TILES = 32;
        public const int MAP_HEIGHT_TILES = 18;
        public const int TILE_SIZE = 32;

        private TiledMap _tiledMap;
        private TiledMapRenderer _mapRenderer;
        private readonly GraphicsManager _graphicsManager;

        public int TileSize => TILE_SIZE;
        public int MapWidthTiles => MAP_WIDTH_TILES;
        public int MapHeightTiles => MAP_HEIGHT_TILES;

        public Map(GraphicsManager graphicsManager)
        {
            _graphicsManager = graphicsManager;
            LoadTiledMap();
        }

        private void LoadTiledMap()
        {
            try
            {
                // Load the tiled map
                _tiledMap = _graphicsManager.Content.Load<TiledMap>("tilesets/WreckTiles");
                
                // Create the renderer
                _mapRenderer = new TiledMapRenderer(_graphicsManager.SpriteBatch.GraphicsDevice, _tiledMap);
                
                System.Diagnostics.Debug.WriteLine("Tiled map loaded successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading tiled map: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        public void Update(GameTime gameTime)
        {
            // Update the map renderer
            _mapRenderer?.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_mapRenderer != null && _tiledMap != null)
            {
                _mapRenderer.Draw();
            }
            else
            {
                FallbackDraw(spriteBatch);
            }
        }
        
        private void FallbackDraw(SpriteBatch spriteBatch)
        {
            // Original drawing code as fallback
            Texture2D asphaltTexture = _graphicsManager.LoadTexture("tiles/asphalt");
            Texture2D borderTexture = _graphicsManager.LoadTexture("tiles/border");

            for (int y = 0; y < MAP_HEIGHT_TILES + 2; y++)
            {
                for (int x = 0; x < MAP_WIDTH_TILES + 2; x++)
                {
                    Texture2D tileTexture = (x == 0 || y == 0 || x == MAP_WIDTH_TILES + 1 || y == MAP_HEIGHT_TILES + 1) ? 
                        borderTexture : asphaltTexture;
                    Vector2 tileWorldPosition = new Vector2(x * TILE_SIZE, y * TILE_SIZE);
                    spriteBatch.Draw(tileTexture, tileWorldPosition, null, Color.White, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0f);
                }
            }
        }

        public int GetIsometricMapWidth()
        {
            if (_tiledMap == null) return MAP_WIDTH_TILES * TILE_SIZE;
            
            // Voor isometrische maps is de werkelijke breedte anders
            return (_tiledMap.Width + _tiledMap.Height) * (_tiledMap.TileWidth / 2);
        }

        public int GetIsometricMapHeight()
        {
            if (_tiledMap == null) return MAP_HEIGHT_TILES * TILE_SIZE;
            
            // Voor isometrische maps is de werkelijke hoogte anders
            return (_tiledMap.Width + _tiledMap.Height) * (_tiledMap.TileHeight / 2);
        }

        // Conversie tussen normale (cartesiaanse) en isometrische coördinaten
        public Vector2 CartesianToIsometric(Vector2 cartesian)
        {
            if (_tiledMap == null) return cartesian;
            
            // Isometric formule: iso_x = (cart_x - cart_y), iso_y = (cart_x + cart_y) / 2
            float isoX = (cartesian.X - cartesian.Y);
            float isoY = (cartesian.X + cartesian.Y) / 2;
            return new Vector2(isoX, isoY);
        }

        public Vector2 IsometricToCartesian(Vector2 isometric)
        {
            if (_tiledMap == null) return isometric;
            
            // Inverse isometric formule
            float cartX = (isometric.X + 2 * isometric.Y) / 2;
            float cartY = (2 * isometric.Y - isometric.X) / 2;
            return new Vector2(cartX, cartY);
        }

        // Wereldpositie naar scherm transformeren
        public Vector2 GetWorldToScreenPosition(Vector2 worldPosition, Matrix viewMatrix)
        {
            if (_tiledMap == null) return worldPosition;
            
            // Convert to isometric first
            Vector2 isoPosition = CartesianToIsometric(worldPosition);
            
            // Apply view transformation
            return Vector2.Transform(isoPosition, viewMatrix);
        }

        // Map bounds voor isometrische map
        public Rectangle GetIsometricMapBounds()
        {
            if (_tiledMap == null)
            {
                return new Rectangle(0, 0, MAP_WIDTH_TILES * TILE_SIZE, MAP_HEIGHT_TILES * TILE_SIZE);
            }
            
            int tileWidth = _tiledMap.TileWidth;
            int tileHeight = _tiledMap.TileHeight;
            
            // Voor isometrische maps is de werkelijke breedte en hoogte anders
            int isometricWidth = (_tiledMap.Width + _tiledMap.Height) * (tileWidth / 2);
            int isometricHeight = (_tiledMap.Width + _tiledMap.Height) * (tileHeight / 2);
            
            return new Rectangle(0, 0, isometricWidth, isometricHeight);
        }
    }
}