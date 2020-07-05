using System.Collections.Generic;
using Alex.API.Utils;
using Alex.Networking.Java.Packets.Play;

namespace Alex.Entities.Properties
{
	public class MovementSpeedProperty : EntityProperty
	{
		private Entity _entity;
		public MovementSpeedProperty(double value = 0.7, Modifier[] modifiers = null) : this(null, value, modifiers)
		{
			
		}

		public MovementSpeedProperty(Entity entity, double value = 0.7, Modifier[] modifiers = null) : base(
			EntityProperties.MovementSpeed, value, modifiers)
		{
			_entity = entity;
		}
		
		protected MovementSpeedProperty(string key, Entity entity, double value = 0.7, Modifier[] modifiers = null) : base(
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
				if (_entity.IsSprinting)
				{
					baseModifiers.Add(
						new Modifier(new UUID("662A6B8D-DA3E-4C1C-8813-96EA6097278D"), 0.29997683577f, ModifierMode.Multiply));
				}
				else if (_entity.IsSneaking)
				{
					baseModifiers.Add(
						new Modifier(new UUID("662A6B8D-DA3E-4C1C-8813-96EA6097278D"), -0.29997683576, ModifierMode.Multiply));
				}
			}

			return baseModifiers;
		}
	}
}