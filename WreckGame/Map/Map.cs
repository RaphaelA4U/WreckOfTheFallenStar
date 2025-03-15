using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WreckGame.Managers;

namespace WreckGame.Map
{
    public class Map
    {
        public const int MAP_WIDTH_TILES = 32;
        public const int MAP_HEIGHT_TILES = 18;
        public const int TILE_SIZE = 32;

        private Texture2D _asphaltTexture;
        private Texture2D _borderTexture;

        public Map(GraphicsManager graphicsManager)
        {
            _asphaltTexture = graphicsManager.LoadTexture("tiles/asphalt");
            _borderTexture = graphicsManager.LoadTexture("tiles/border");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int y = 0; y < MAP_HEIGHT_TILES + 2; y++)
            {
                for (int x = 0; x < MAP_WIDTH_TILES + 2; x++)
                {
                    Texture2D tileTexture = (x == 0 || y == 0 || x == MAP_WIDTH_TILES + 1 || y == MAP_HEIGHT_TILES + 1) ? _borderTexture : _asphaltTexture;
                    Vector2 tileWorldPosition = new Vector2(x * TILE_SIZE, y * TILE_SIZE);
                    spriteBatch.Draw(tileTexture, tileWorldPosition, null, Color.White, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0f);
                }
            }
        }
    }
}