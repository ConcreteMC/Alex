using Alex.Items;
using Alex.Worlds;

namespace Alex.Entities.Generic
{
	public sealed class Fireball : ItemBaseEntity
	{
		/// <inheritdoc />
		public Fireball(World level) : base(EntityType.Fireball, level)
		{
			Height = 1.0;
			Width = 1.0;
			
			if (ItemFactory.TryGetItem("minecraft:fire_charge", out var item))
			{
				SetItem(item);
			}
		}
	}
}