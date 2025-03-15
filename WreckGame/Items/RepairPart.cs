using Microsoft.Xna.Framework;
using WreckGame.Managers;
using WreckGame.Entities;
using System;

namespace WreckGame.Items
{
    public class RepairPart : Collectible
    {
        private readonly int _index;

        public RepairPart(GraphicsManager graphicsManager, Vector2 position, int index) : base(graphicsManager, "items/parts", position)
        {
            _index = index;
        }

        public override void OnCollect(Player player)
        {
            player.HP = Math.Min(player.HP + 25, 100);
        }

        protected override float GetHoverOffset(double time)
        {
            return (float)Math.Sin(time * 2.7f + _index * 0.3f) * 8f;
        }
    }
}