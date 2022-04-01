using Alex.Networking.Java.Packets.Play;

namespace Alex.Entities.Properties;

public class AttackSpeedProperty : MovementSpeedProperty
{
	public AttackSpeedProperty(double value = 0.4, Modifier[] modifiers = null) : this(null, value, modifiers) { }

	public AttackSpeedProperty(Entity entity, double value = 0.4, Modifier[] modifiers = null) : base(
		EntityProperties.AttackSpeed, entity, value, modifiers) { }
}