using System;
using Alex.Rendering;
using Microsoft.Xna.Framework;

namespace Alex.Terrain
{
    public class TerrainGenerator
    {
        public TerrainGenerator(string seed)
        {
            Seed = seed.GetHashCode();
        }

        public long Seed { get; set; }

        public virtual Chunk GenerateChunk(Vector3 position)
        {
            throw new NotImplementedException();
        }

        public virtual Vector3 GetSpawnPoint()
        {
            return new Vector3(0f, 32f, 0f);
        }
    }
}
