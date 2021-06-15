using Microsoft.Xna.Framework;

namespace Alex.Entities.Components.Effects
{
	public class JumpBoostEffect : Effect
	{
		/// <inheritdoc />
		public JumpBoostEffect() : base(EffectType.JumpBoost)
		{
			Particles = false;
			ParticleColor = new Color(0x22, 0xFF, 0x4C);
		}

		/// <inheritdoc />
		public override float Modify(float modifier)
		{
			return modifier + ((modifier * 0.5f) * Level);
		}
	}
}