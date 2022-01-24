using Alex.Common.Resources;
using Alex.Worlds;

namespace Alex.Entities.BlockEntities;

public class HopperBlockEntity : BlockEntity
{
	public HopperBlockEntity(World level) : base(level)
	{
		Type = new ResourceLocation("minecraft:hopper");
	}
}