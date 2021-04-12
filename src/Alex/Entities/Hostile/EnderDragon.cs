using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class EnderDragon : HostileMob
	{
		public EnderDragon(World level) : base(level)
		{
			Height = 8;
			Width = 16;
		}
		
		/// <inheritdoc />
		public override void EntityDied()
		{
			base.EntityDied();
			Alex.Instance.AudioEngine.PlaySound("mob.enderdragon.death", RenderLocation, 1f, 1f);
		}

		/// <inheritdoc />
		public override void EntityHurt()
		{
			base.EntityHurt();
			Alex.Instance.AudioEngine.PlaySound("mob.enderdragon.hit", RenderLocation, 1f, 1f);
		}
	}
}
