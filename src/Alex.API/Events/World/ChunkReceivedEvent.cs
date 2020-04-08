using Alex.API.Utils;
using Alex.API.World;

namespace Alex.API.Events.World
{
    public class ChunkReceivedEvent : ChunkEvent
    {
        public IChunkColumn Chunk { get; }
        public ChunkReceivedEvent(ChunkCoordinates coordinates, IChunkColumn chunk) : base(coordinates)
        {
            Chunk = chunk;
        }
    }
}