using Alex.Common.Resources;
using Alex.Interfaces.Resources;
using Alex.Worlds;

namespace Alex.Entities.BlockEntities;

public class BarrelBlockEntity : BlockEntity
{
	/// <inheritdoc />
	public BarrelBlockEntity(World level) : base(level)
	{
		Type = new ResourceLocation("minecraft:barrel");
	}
}