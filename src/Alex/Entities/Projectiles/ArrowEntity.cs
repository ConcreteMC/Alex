using Alex.Net;
using Alex.Worlds;
using ConcreteMC.MolangSharp.Attributes;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Projectiles
{
	public class ArrowEntity : ThrowableEntity
	{
		/// <inheritdoc />
		public ArrowEntity(World level) : base(level)
		{
			Width = 0.15;
			//Length = 0.15;
			Height = 0.15;

			Gravity = 0.05;
			Drag = 0.01;

			StopOnImpact = true;
			DespawnOnImpact = false;
		}

		[MoProperty("shake_time")] public double ShakeTime { get; set; } = 0;
	}
}