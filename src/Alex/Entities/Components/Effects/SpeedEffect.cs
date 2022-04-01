using System;
using Alex.Networking.Java.Packets.Play;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Components.Effects
{
	public class SpeedEffect : Effect
	{
		private double _multiplier = 0.02;

		//private MiNET.Utils.UUID _uuid = new MiNET.Utils.UUID("91AEAA56-376B-4498-935B-2F7F68070635");

		private Modifier _modifier = new Modifier(
			Guid.Parse("91AEAA56-376B-4498-935B-2F7F68070635"), 0d, ModifierMode.Multiply);

		public SpeedEffect() : base(EffectType.Speed)
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