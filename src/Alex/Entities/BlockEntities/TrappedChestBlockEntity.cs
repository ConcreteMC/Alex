using Alex.Common.Resources;
using Alex.Interfaces.Resources;
using Alex.Worlds;

namespace Alex.Entities.BlockEntities;

public class TrappedChestBlockEntity : ChestBlockEntity
{
	/// <inheritdoc />
	public TrappedChestBlockEntity(World level) : base(level)
	{
		Type = new ResourceLocation("minecraft:trapped_chest");
	}
}