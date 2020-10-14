using System;

namespace Alex.API.World
{
    [Flags]
    public enum ScheduleType
    {
        Unscheduled = 1,
        Border      = 4,
        Scheduled   = 8,
        Lighting    = 16,
        LowPriority = 32,
        
        Full = Lighting | Border | Scheduled
    }
}