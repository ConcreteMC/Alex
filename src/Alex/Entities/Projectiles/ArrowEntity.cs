using Alex.Net;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Projectiles
{
	public class ArrowEntity : ThrowableEntity
	{
		/// <inheritdoc />
		public ArrowEntity(World level, NetworkProvider network) : base((int) EntityType.Arrow, level, network)
		{
			Width = 0.15;
			//Length = 0.15;
			Height = 0.15;

			Gravity = 0.05;
			Drag = 0.01;

			StopOnImpact = true;
			DespawnOnImpact = false;
		}
	}
}