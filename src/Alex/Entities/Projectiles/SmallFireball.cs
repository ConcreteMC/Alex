using Alex.Items;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Projectiles
{
	public sealed class SmallFireball : ItemBaseEntity
	{
		/// <inheritdoc />
		public SmallFireball(Worlds.World level) : base(EntityType.SmallFireball, level)
		{
			Width = 0.3125;
			Height = 0.3125;
			Gravity = 0;

			if (ItemFactory.TryGetItem("minecraft:fire_charge", out var item))
			{
				SetItem(item);
			}
		}

		/// <inheritdoc />
		public override void CollidedWithWorld(Vector3 direction, Vector3 position)
		{
			base.CollidedWithWorld(direction, position);
			Velocity = Vector3.Zero;
			NoAi = true;
		}
	}
}