using System.Linq;
using Alex.API.Resources;
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

        private void RegisterBuiltIn()
        {
            
        }

        public int LoadResourcePack(IProgressReceiver progressReceiver, McResourcePack resourcePack)
        {
            int imported = 0;
            var total    = resourcePack.BlockModels.Count;
            progressReceiver?.UpdateProgress(0, total, "Loading block models...");
            
            foreach (var blockmodel in resourcePack.BlockModels.Where(x => x.Value.Elements.Length > 0))
            {   
                progressReceiver?.UpdateProgress(imported, total, null, blockmodel.Key.ToString());
                Register(blockmodel.Key, new BlockModelEntry(blockmodel.Value));
                imported++;
            }

            return imported;
        }
    }
}