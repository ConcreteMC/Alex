using Alex.Items;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Projectiles
{
	public sealed class Fireball : ItemBaseEntity
	{
		/// <inheritdoc />
		public Fireball(World level) : base(EntityType.Fireball, level)
		{
			Height = 0.31;
			Width = 0.31;
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