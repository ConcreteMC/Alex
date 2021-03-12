using Alex.Items;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Projectiles
{
	public sealed class SmallFireball : ThrowableItemEntity
	{
		/// <inheritdoc />
		public SmallFireball(Worlds.World level) : base(level, "minecraft:fire_charge")
		{
			Width = 0.3125;
			Height = 0.3125;
			Gravity = 0;
			DespawnOnImpact = true;
		}
	}
}