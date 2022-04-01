using Alex.Worlds;

namespace Alex.Entities.Projectiles
{
	public class EggEntity : ThrowableItemEntity
	{
		/// <inheritdoc />
		public EggEntity(World level) : base(level, "minecraft:egg")
		{
			Width = 0.25;
			//Length = 0.25;
			Height = 0.25;

			Gravity = 0.03;
			Drag = 0.01;

			DespawnOnImpact = true;
		}
	}
}