using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.Networking.Java.Util;
using Alex.Worlds.Chunks;

namespace Alex.Worlds.Multiplayer.Java
{
    public class JavaChunkSection : ChunkSection
    {
        public JavaChunkSection(ChunkColumn owner, int y, bool storeSkylight, int sections = 2) : base(owner, y, storeSkylight, sections)
        {
            
        }

        public override void RemoveInvalidBlocks()
        {
            this._blockRefCount = 0;
            this._tickRefCount = 0;

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
                        TransparentBlocks.Set(idx, block.Transparent);
                        SolidBlocks.Set(idx, block.Solid);

                        if (!(block is Air))
                        {
                            ++this._blockRefCount;

                            if (block.RandomTicked)
                            {
                                ++this._tickRefCount;
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
        
        public void Read(MinecraftStream ms)
        {
            _blockStorages[0].Read(ms);
        }
    }
}