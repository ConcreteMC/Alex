using System.Linq;
using Alex.Common.Resources;
using Alex.Graphics.Models.Blocks;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Models;

namespace Alex.Blocks
{
	public class BlockModelEntry : IRegistryEntry<ResourcePackModelBase>
	{
		public ResourcePackModelBase Value { get; }

		public BlockModelEntry(ResourcePackModelBase model)
		{
			Value = model;
		}

		public ResourceLocation Location { get; private set; }

		public IRegistryEntry<ResourcePackModelBase> WithLocation(ResourceLocation location)
		{
			Location = location;

			return this;
		}
	}

	public class BlockModelRegistry : RegistryBase<ResourcePackModelBase>
	{
		public BlockModelRegistry() : base("BlockModel")
		{
			RegisterBuiltIn();
		}

		private void RegisterBuiltIn() { }

		/*  public int LoadResources(IProgressReceiver progressReceiver, ResourceManager resources, bool replace)
		  {
		      
		      int imported = 0;
		      var total    = resources.BlockModels.Count;
		      progressReceiver?.UpdateProgress(0, total, "Loading block models...");
		      
		      foreach (var blockmodel in resources.BlockModels.Where(x => x.Value.Elements.Length > 0))
		      {   
		          progressReceiver?.UpdateProgress(imported, total, null, blockmodel.Key.ToString());
		          var key = blockmodel.Key;
  
		          if (ContainsKey(key))
		          {
		              if (replace)
		              {
		                  var entry = new BlockModelEntry(blockmodel.Value);
		                  Set(key, () => entry);
		              }
		          }
		          else
		          {
		              Register(blockmodel.Key, new BlockModelEntry(blockmodel.Value));
		          }
  
		          imported++;
		      }
  
		      return imported;*
		  }*/
	}
}