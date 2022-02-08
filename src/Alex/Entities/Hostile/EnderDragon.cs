using Alex.Worlds;
using ConcreteMC.MolangSharp.Attributes;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class EnderDragon : HostileMob
	{
		[MoProperty("wing_flap_position")] public double WingFlapPosition { get; set; } = 0d;

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