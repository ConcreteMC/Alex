using Alex.Net;
using Alex.Worlds;

namespace Alex.Entities.Projectiles
{
	public class SnowballEntity : ThrowableEntity
	{
		/// <inheritdoc />
		public SnowballEntity(World level, NetworkProvider network) : base((int) EntityType.Snowball, level, network)
		{
			Width = 0.25;
		//	Length = 0.25;
			Height = 0.25;

			Gravity = 0.03;
			Drag = 0.01;

			DespawnOnImpact = true;
		}
	}
}