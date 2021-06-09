using Alex.Common.Entities.Properties;
using Alex.Networking.Java.Packets.Play;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Effects
{
	public class SpeedEffect: Effect
	{
		private double _multiplier = 0.02;

		private MiNET.Utils.UUID _uuid = new MiNET.Utils.UUID("91AEAA56-376B-4498-935B-2F7F68070635");
		public SpeedEffect() : base(EffectType.Speed)
		{
			Particles = false;
			ParticleColor = new Color(0x7c, 0xaf, 0xc6);
		}

		public override void Add(Entity entity)
		{
			entity.EntityProperties[EntityProperties.MovementSpeed].ApplyModifier(new Modifier(_uuid, (Level + 1) * _multiplier, ModifierMode.Multiply));
			//entity.MovementSpeed = (float) (0.1 + (Level + 1) * _multiplier);
		}

		public override void Remove(Entity entity)
		{
			entity.EntityProperties[EntityProperties.MovementSpeed].RemoveModifier(_uuid);
			//entity.MovementSpeed = 0.1f;
		}
	}
}