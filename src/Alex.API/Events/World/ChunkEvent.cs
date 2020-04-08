using Alex.API.Utils;

namespace Alex.API.Events.World
{
    public class ChunkEvent : Event
    {
        public ChunkCoordinates Coordinates { get; set; }
        public bool DoUpdates { get; set; } = true;
        public ChunkEvent(ChunkCoordinates coordinates)
        {
            Coordinates = coordinates;
        }
    }
}