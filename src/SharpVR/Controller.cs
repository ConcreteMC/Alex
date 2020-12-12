using System.Runtime.InteropServices;
using Valve.VR;

namespace SharpVR
{
    public class Controller : TrackedDevice
    {
        public Hand Hand { get; }

        private VRControllerState_t _lastState;
        private VRControllerState_t _currentState;

        public bool ButtonPressed(EVRButtonId button)
        {
            return (_lastState.ulButtonPressed & (1ul << (int) button)) == 0 &&
                   (_currentState.ulButtonPressed & (1ul << (int) button)) != 0;
        }

        public void Update()
        {
            _lastState = _currentState;
            Context.System.GetControllerState((uint) Index, ref _currentState, (uint) Marshal.SizeOf<VRControllerState_t>());
                //throw new SharpVRException("Getting controller state failed");
        }

        internal Controller(VrContext context, int index) : base(context, index)
        {
            var role = context.System.GetControllerRoleForTrackedDeviceIndex((uint) index);
            switch (role)
            {
                case ETrackedControllerRole.LeftHand:
                    Hand = Hand.Left;
                    break;
                case ETrackedControllerRole.RightHand:
                    Hand = Hand.Right;
                    break;
                default:
                    Hand = Hand.None;
                    break;
            }
        }
    }
}