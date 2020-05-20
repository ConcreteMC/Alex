using System;

namespace Alex.API.World
{
    [Flags]
    public enum ScheduleType
    {
        Unscheduled = 1,
        Full = 2,
        Border = 4,
        Scheduled = 8,
        Lighting = 16
    }
}