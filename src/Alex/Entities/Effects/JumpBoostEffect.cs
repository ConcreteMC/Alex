using Microsoft.Xna.Framework;

namespace Alex.Entities.Effects
{
	public class JumpBoostEffect : Effect
	{
		/// <inheritdoc />
		public JumpBoostEffect() : base(EffectType.JumpBoost)
		{
			Particles = false;
			ParticleColor = new Color(0x22, 0xFF, 0x4C);
		}
	}
}