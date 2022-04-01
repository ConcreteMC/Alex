using Alex.Networking.Java.Packets.Play;
using MiNET.Utils;

namespace Alex.Entities.Components.Effects;

public class HasteEffect : Effect
{
	public static readonly UUID DigSpeedUUID = new MiNET.Utils.UUID("AF8B6E3F-3328-4C0A-AA36-5BA2BB9DBEF3");

	/// <inheritdoc />
	public HasteEffect() : base(EffectType.Haste)
	{
		Particles = false;
	}

	public override void ApplyTo(Entity entity)
	{
		entity.EntityProperties[EntityProperties.AttackSpeed].ApplyModifier(
			new Modifier(DigSpeedUUID, (Level + 1) * 0.1, ModifierMode.Multiply));
	}

	public override void Remove(Entity entity)
	{
		entity.EntityProperties[EntityProperties.AttackSpeed].RemoveModifier(DigSpeedUUID);
	}
}