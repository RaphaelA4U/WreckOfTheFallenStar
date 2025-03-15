using Microsoft.Xna.Framework;
using WreckGame.Managers;
using WreckGame.Entities;
using System;

namespace WreckGame.Items
{
    public class DataShard : Collectible
    {
        private readonly int _index;

        public DataShard(GraphicsManager graphicsManager, Vector2 position, int index) : base(graphicsManager, "items/data_shard", position)
        {
            _index = index;
        }

        public override void OnCollect(Player player) { /* No effect */ }

        protected override float GetHoverOffset(double time)
        {
            return (float)Math.Sin(time * 3f + _index * 0.5f) * 6f;
        }
    }
}