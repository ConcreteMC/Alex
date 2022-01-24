using Alex.Common.Resources;

namespace Alex.Items
{
	public class ItemRegistry : RegistryBase<Item>
	{
		/// <inheritdoc />
		public ItemRegistry() : base("item")
		{
			/*Register("minecraft:bow", () =>
			{
				return new ItemBow();
			});*/
		}
	}
}