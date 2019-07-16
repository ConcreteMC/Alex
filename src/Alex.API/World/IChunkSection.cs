using Alex.API.Blocks.State;

namespace Alex.API.World
{
    public interface IChunkSection
    {
        bool SolidBorder { get; }
        bool HasAirPockets { get; }
        bool IsDirty { get; set; }
        void ResetSkyLight();
        bool IsScheduled(int x, int y, int z);
        void SetScheduled(int x, int y, int z, bool value);
        bool IsLightingScheduled(int x, int y, int z);
        bool SetLightingScheduled(int x, int y, int z, bool value);
        IBlockState Get(int x, int y, int z);
        void Set(int x, int y, int z, IBlockState state);
        bool IsTransparent(int x, int y, int z);
        bool IsSolid(int x, int y, int z);
        void GetBlockData(int bx, int by, int bz, out bool transparent, out bool solid);
        bool IsEmpty();
        bool NeedsRandomTick();
        int GetYLocation();
        void SetSkylight(int x, int y, int z, int value);
        byte GetSkylight(int x, int y, int z);
        void SetBlocklight(int x, int y, int z, byte value);
        int GetBlocklight(int x, int y, int z);
        void RemoveInvalidBlocks();
      //  bool IsFaceSolid(BlockFace face);
    }
}
