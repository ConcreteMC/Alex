using Alex.Items;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Projectiles
{
	public class ThrownEnderPearlEntity: ThrowableItemEntity
	{
		/// <inheritdoc />
		public ThrownEnderPearlEntity(World level) : base(level, "minecraft:ender_pearl")
		{
			Height = 0.25;
			Width = 0.25;
			
			Gravity = 0.03;
			Drag = 0.01;

			DespawnOnImpact = true;
		}
	}
	
	public class ThrownEyeOfEnderEntity: ThrowableItemEntity
	{
		/// <inheritdoc />
		public ThrownEyeOfEnderEntity(World level) : base(level, "minecraft:ender_eye")
		{
			Height = 0.25;
			Width = 0.25;
			
			Gravity = 0.03;
			Drag = 0.01;

			DespawnOnImpact = true;
		}
	}
}