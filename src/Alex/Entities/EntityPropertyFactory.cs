using Alex.Entities.Properties;
using Alex.Networking.Java.Packets.Play;

namespace Alex.Entities
{
	public class AlexPropertyFactory : EntityPropertyFactory
	{
		/// <inheritdoc />
		public override EntityProperty Create(string key, double value, Modifier[] modifiers)
		{
			switch (key)
			{
				case EntityProperties.MovementSpeed:
					return new MovementSpeedProperty(value, modifiers);

				case EntityProperties.FlyingSpeed:
					return new FlyingSpeedProperty(value, modifiers);
			}

			return base.Create(key, value, modifiers);
		}
	}
}