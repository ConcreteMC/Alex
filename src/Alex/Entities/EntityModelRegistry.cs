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

		private void RegisterBuiltIn()
		{
			
		}
		
		public int LoadResourcePack(IProgressReceiver progressReceiver, BedrockResourcePack resourcePack, bool replace)
		{
			int imported = 0;
			var total    = resourcePack.EntityModels.Count;
			
			progressReceiver?.UpdateProgress(0, total, "Loading entity models...");
			foreach (var blockmodel in resourcePack.EntityModels)
			{   
				progressReceiver?.UpdateProgress(imported, total, null, blockmodel.Key.ToString());
				var key = blockmodel.Key;

				if (ContainsKey(key))
				{
					if (replace)
					{
						var entry = new EntityModelEntry(blockmodel.Value);
						Set(key, () => entry);
					}
				}
				else
				{
					Register(blockmodel.Key, new EntityModelEntry(blockmodel.Value));
				}

				imported++;
			}

			return imported;
		}
	}
}