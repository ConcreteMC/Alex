using Alex.Common.Entities.Properties;
using Alex.Networking.Java.Packets.Play;
using MiNET.Utils;

namespace Alex.Entities.Components.Effects;

public class MiningFatigueEffect : Effect
{
	public static readonly UUID DigSlowDownUUID = new MiNET.Utils.UUID("55FCED67-E92A-486E-9800-B47F202C4386");
	
	/// <inheritdoc />
	public MiningFatigueEffect() : base(EffectType.MiningFatigue)
	{
		Particles = false;
	}

	public override void ApplyTo(Entity entity)
	{
		entity.EntityProperties[EntityProperties.AttackSpeed].ApplyModifier(
			new Modifier(DigSlowDownUUID, (Level + 1) * -0.1, ModifierMode.Multiply));
	}

	public override void Remove(Entity entity)
	{
		entity.EntityProperties[EntityProperties.AttackSpeed].RemoveModifier(DigSlowDownUUID);
	}
}