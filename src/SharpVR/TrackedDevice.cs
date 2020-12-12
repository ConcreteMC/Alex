using System;
using Valve.VR;

namespace SharpVR
{
    public abstract class TrackedDevice
    {
        protected readonly VrContext Context;
        public int Index { get; }

        public bool IsConnected => Context.System.IsTrackedDeviceConnected((uint) Index);

        internal TrackedDevice(VrContext context, int index)
        {
            Context = context;
            Index = index;
        }

        public HmdMatrix34_t GetPose()
        {
            return Context.ValidDevicePoses[Index].mDeviceToAbsoluteTracking;
        }

        public HmdMatrix34_t GetNextPose()
        {
            return Context.ValidNextDevicePoses[Index].mDeviceToAbsoluteTracking;
        }

        public HmdVector3_t GetVelocity()
        {
            return Context.ValidDevicePoses[Index].vVelocity;
        }

        public HmdVector3_t GetAngularVelocity()
        {
            return Context.ValidDevicePoses[Index].vAngularVelocity;
        }

        internal static TrackedDevice Create(VrContext context, int index)
        {
            var cls = context.System.GetTrackedDeviceClass((uint) index);
            switch (cls)
            {
                case ETrackedDeviceClass.Invalid:
                    return null;
                case ETrackedDeviceClass.HMD:
                    return new HeadMountedDisplay(context, index);
                case ETrackedDeviceClass.Controller:
                    return new Controller(context, index);
                case ETrackedDeviceClass.GenericTracker:
                    return new GenericTracker(context, index);
                case ETrackedDeviceClass.TrackingReference:
                    return new TrackingReference(context, index);
                case ETrackedDeviceClass.DisplayRedirect:
                    return new DisplayRedirect(context, index);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected Exception CreateException(ETrackedPropertyError error)
        {
            var str = Context.System.GetPropErrorNameFromEnum(error);
            return new SharpVRException("Get property error: " + str, (int) error);
        }
    }
}