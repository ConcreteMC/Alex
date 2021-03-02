using Alex.Net;
using Alex.Worlds;

namespace Alex.Entities.Projectiles
{
	public class FireworkRocket : ThrowableEntity
	{
		/// <inheritdoc />
		public FireworkRocket(World level) : base(level)
		{
			Width = 0.25;
			Height = 0.25;
			
			Gravity = 0.0;
			Drag = 0.01;
			
			HasCollision = true;
			IsAffectedByGravity = true;
			StopOnImpact = false;
		}

		/// <inheritdoc />
		public override void OnTick()
		{
			base.OnTick();
		}
	}
}