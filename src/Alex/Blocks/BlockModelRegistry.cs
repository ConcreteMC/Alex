using Alex.API.Resources;
using Alex.Graphics.Models.Blocks;

namespace Alex.Blocks
{
    public class BlockModelEntry : IRegistryEntry<BlockModel>
    {
        public BlockModel Value { get; }

        public BlockModelEntry(BlockModel model)
        {
            Value = model;
        }
        
        public ResourceLocation Location { get; private set; }
        public IRegistryEntry<BlockModel> WithLocation(ResourceLocation location)
        {
            Location = location;
            return this;
        }
    }
    
    public class BlockModelRegistry : RegistryBase<BlockModel>
    {
        public BlockModelRegistry() : base("BlockModel")
        {
            RegisterBuiltIn();
        }

        private void RegisterBuiltIn()
        {
            Register("minecraft:water", new BlockModelEntry(new LiquidBlockModel()
            {
                //IsFlowing = false,
                IsLava = false,
               // Level = 8
            }));

            Register("minecraft:lava", new BlockModelEntry(new LiquidBlockModel()
            {
             //   IsFlowing = false,
                IsLava = true,
             //   Level = 8
            }));
        }
    }
}