using Alex.Networking.Java.Packets.Play;

namespace Alex.Entities.Properties
{
	public class FlyingSpeedProperty : MovementSpeedProperty
	{
		public FlyingSpeedProperty(double value = 0.4, Modifier[] modifiers = null) : this(null, value, modifiers)
		{
			
		}

		public FlyingSpeedProperty(Entity entity, double value = 0.4, Modifier[] modifiers = null) : base(
			EntityProperties.FlyingSpeed, entity, value, modifiers)
		{
			
		}
	}
}