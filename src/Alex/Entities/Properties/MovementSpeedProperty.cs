using System.Collections.Generic;
using Alex.API.Utils;
using Alex.Entities.Effects;
using Alex.Networking.Java.Packets.Play;

namespace Alex.Entities.Properties
{
	public class MovementSpeedProperty : EntityProperty
	{
		private Entity _entity;
		public MovementSpeedProperty(double value = 0.699999988079071, Modifier[] modifiers = null) : this(null, value, modifiers)
		{
			
		}

		public MovementSpeedProperty(Entity entity, double value = 0.699999988079071, Modifier[] modifiers = null) : base(
			EntityProperties.MovementSpeed, value, modifiers)
		{
			_entity = entity;
		}
		
		protected MovementSpeedProperty(string key, Entity entity, double value = 0.699999988079071, Modifier[] modifiers = null) : base(
			key, value, modifiers)
		{
			_entity = entity;
		}

		/// <inheritdoc />
		protected override IEnumerable<Modifier> GetAppliedModifiers()
		{
			List<Modifier> baseModifiers = new List<Modifier>();
			baseModifiers.AddRange(base.GetAppliedModifiers());

			if (_entity != null)
			{
				/*if (_entity.IsSprinting)
				{
					baseModifiers.Add(
						new Modifier(new UUID("662A6B8D-DA3E-4C1C-8813-96EA6097278D"), 0.29997683577f, ModifierMode.Multiply));
				}
				else if (_entity.IsSneaking)
				{
					baseModifiers.Add(
						new Modifier(new UUID("662A6B8D-DA3E-4C1C-8813-96EA6097278D"), -1f + 0.29997683576f, ModifierMode.Multiply));
				}*/

				/*foreach (var effect in _entity.AppliedEffects())
				{
					switch (effect)
					{
						case SpeedEffect speedEffect:
						{
							baseModifiers.Add(new Modifier(new UUID("91AEAA56-376B-4498-935B-2F7F68070635"), 0.2 * (speedEffect.Level + 1), ModifierMode.Multiply));
							break;
						}
					}
				}*/
			}

			return baseModifiers;
		}
	}
}