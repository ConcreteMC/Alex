using Alex.Common.Entities.Properties;
using Alex.Networking.Java.Packets.Play;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Components.Effects
{
	public class SlownessEffect : Effect
	{
		private double _multiplier = -0.15000000596046448;

		private Modifier _modifier = new Modifier(
			new MiNET.Utils.UUID("7107de5e-7ce8-4030-940e-514c1f160890"), 0d, ModifierMode.Multiply);

		/// <inheritdoc />
		public SlownessEffect() : base(EffectType.Slowness)
		{
			Particles = false;
			ParticleColor = new Color(0x7c, 0xaf, 0xc6);
		}

		public override void ApplyTo(Entity entity)
		{
			_modifier.Amount = (Level + 1) * _multiplier;
			entity.EntityProperties[EntityProperties.MovementSpeed].ApplyModifier(_modifier);
			//entity.MovementSpeed = (float) (0.1 + (Level + 1) * _multiplier);
		}

		public override void Remove(Entity entity)
		{
			entity.EntityProperties[EntityProperties.MovementSpeed].RemoveModifier(_modifier.Uuid);
			//entity.MovementSpeed = 0.1f;
		}
	}
}