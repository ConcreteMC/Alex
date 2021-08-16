using Alex.Common.Resources;

namespace Alex.Items.ComponentSystem
{
	public class ItemComponent
	{
		public ResourceLocation Name { get; }

		public ItemComponent(ResourceLocation name)
		{
			Name = name;
		}
	}
}