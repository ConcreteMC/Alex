using System;

namespace SharpVR
{
    public class TrackedDeviceActivateEventArgs : EventArgs
    {
        public readonly TrackedDevice Device;

        public TrackedDeviceActivateEventArgs(TrackedDevice device)
        {
            Device = device;
        }
    }
}