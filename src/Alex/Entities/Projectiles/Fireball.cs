using Alex.Worlds;

namespace Alex.Entities.Projectiles
{
	public sealed class Fireball : ThrowableItemEntity
	{
		/// <inheritdoc />
		public Fireball(World level) : base(level, "minecraft:fire_charge")
		{
			Height = 0.31;
			Width = 0.31;
			Gravity = 0;
			DespawnOnImpact = true;
		}
	}
}