using Alex.API.Utils;

namespace Alex.API.Events.World
{
    public class ChunkUnloadEvent : ChunkEvent
    {
        public ChunkUnloadEvent(ChunkCoordinates coordinates) : base(coordinates)
        {
            
        }
    }
}