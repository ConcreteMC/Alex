using Alex.Items;

namespace Alex.Entities.Generic
{
	public sealed class SmallFireball : ItemBaseEntity
	{
		/// <inheritdoc />
		public SmallFireball(Worlds.World level) : base(EntityType.SmallFireball, level)
		{
			Width = 0.3125;
			Height = 0.3125;
			
			if (ItemFactory.TryGetItem("minecraft:fire_charge", out var item))
			{
				SetItem(item);
			}
		}
	}
}