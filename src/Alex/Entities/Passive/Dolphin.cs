using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Passive
{
	public class Dolphin : WaterMob
	{
		/// <inheritdoc />
		public Dolphin(World level) : base(level)
		{
			Width = 0.9;
			Height = 0.6;
		}

		/// <inheritdoc />
		public override void EntityDied()
		{
			base.EntityDied();
			Alex.Instance.AudioEngine.PlaySound("mob.dolphin.death", RenderLocation, 1f, 1f);
		}

		/// <inheritdoc />
		public override void EntityHurt()
		{
			base.EntityHurt();
			Alex.Instance.AudioEngine.PlaySound("mob.dolphin.hurt", RenderLocation, 1f, 1f);
		}
	}
}