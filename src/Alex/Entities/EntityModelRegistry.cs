using System.Linq;
using Alex.Blocks;
using Alex.Common.Resources;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Models.Entities;

namespace Alex.Entities
{
	public class EntityModelEntry : IRegistryEntry<EntityModel>
	{
		public EntityModel Value { get; }

		public EntityModelEntry(EntityModel model)
		{
			Value = model;
		}

		public ResourceLocation Location { get; private set; }

		public IRegistryEntry<EntityModel> WithLocation(ResourceLocation location)
		{
			Location = location;

			return this;
		}
	}

	public class EntityModelRegistry : RegistryBase<EntityModel>
	{
		/// <inheritdoc />
		public EntityModelRegistry() : base("EntityModel")
		{
			RegisterBuiltIn();
		}

		private void RegisterBuiltIn() { }
	}
}