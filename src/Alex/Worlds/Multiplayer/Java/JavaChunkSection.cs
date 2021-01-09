using Alex.API.Utils;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Networking.Java.Util;
using Alex.Worlds.Chunks;

namespace Alex.Worlds.Multiplayer.Java
{
    public class JavaChunkSection : ChunkSection
    {
        public JavaChunkSection(bool storeSkylight, int sections = 2) : base(storeSkylight, sections)
        {
            
        }

        public override void RemoveInvalidBlocks()
        {
            this.BlockRefCount = 0;
            this.TickRefCount = 0;

            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        var idx = GetCoordinateIndex(x, y, z);

                        var blockstate = this.Get(x, y, z, 0);//.Block;
                        if (blockstate == null)
                            continue;

                        var block = blockstate.Block;
                        //TransparentBlocks.Set(idx, block.Transparent);
                       // SolidBlocks.Set(idx, block.Solid);

                        if (!(block is Air))
                        {
                            ++this.BlockRefCount;

                            if (block.RandomTicked)
                            {
                                ++this.TickRefCount;
                            }

                            if (block.BlockMaterial.IsWatterLoggable)
                            {
                                Set(1, x, y, z, BlockFactory.GetBlockState("minecraft:water"));
                            }
                        }

                        if (block.LightValue > 0)
                        {
                            var coords = new BlockCoordinates(x, y, z);

                            if (!LightSources.Contains(coords))
                            {
                                LightSources.Add(coords);
                            }

                            if (GetBlocklight(x, y, z) != block.LightValue)
                            {
                                SetBlocklight(x, y, z, (byte) block.LightValue);
                                SetBlockLightScheduled(x, y, z, true);
                            }
                        }

                    }
                }
            }
        }

        /// <inheritdoc />
        protected override void OnBlockSet(int x, int y, int z, BlockState newState, BlockState oldState)
        {
            if (newState == null || oldState == null)
                return;

            if (oldState.Block.BlockMaterial.IsWatterLoggable && !newState.Block.BlockMaterial.IsWatterLoggable)
            {
                Set(1, x, y, z, BlockFactory.GetBlockState("minecraft:air"));
            }
            else if (!oldState.Block.BlockMaterial.IsWatterLoggable && newState.Block.BlockMaterial.IsWatterLoggable)
            {
                Set(1, x, y, z, BlockFactory.GetBlockState("minecraft:water"));
            }

            // base.OnBlockSet(x, y, z, newState, oldState);
        }

        public void Read(MinecraftStream ms)
        {
            BlockStorages[0].Read(ms);

            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    for (int y = 0; y < 16; y++)
                    {
                        SetScheduled(x, y, z, true);
                    }
                }
            }
        }
    }
}