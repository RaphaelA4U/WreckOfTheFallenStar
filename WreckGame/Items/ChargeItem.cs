using Microsoft.Xna.Framework;
using WreckGame.Managers;
using WreckGame.Entities;
using System;

namespace WreckGame.Items
{
    public class ChargeItem : Collectible
    {
        private readonly int _index;

        public ChargeItem(GraphicsManager graphicsManager, Vector2 position, int index) : base(graphicsManager, "items/charge", position)
        {
            _index = index;
        }

        public override void OnCollect(Player player)
        {
            player.Charge = Math.Min(player.Charge + 25, 100);
        }

        protected override float GetHoverOffset(double time)
        {
            return (float)Math.Sin(time * 3.3f + _index * 0.7f) * 10f;
        }
    }
}