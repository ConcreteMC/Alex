using System;
using Microsoft.Xna.Framework;
using Valve.VR;

namespace Alex.API.Input.Listeners
{
    public interface IVRControllerInputListener
    {
        
    }

    public struct VRControllerPairState
    {
        public VRControllerState Left { get; set; }
        public VRControllerState Right { get; set; }
        
    }
    
    public struct VRControllerState
    {
        public static readonly VRControllerState Default;

        public ulong ButtonPressed;
        public ulong ButtonTouched;

        public Vector2 Axis0;
        public Vector2 Axis1;
        public Vector2 Axis2;
        public Vector2 Axis3;
        public Vector2 Axis4;
    }

    [Flags]
    public enum VRButtons
    {
        LeftSystem = Left | EVRButtonId.System,
        LeftApplicationMenu = Left | EVRButtonId.ApplicationMenu,
        LeftGrip = Left | EVRButtonId.Grip,
        LeftDPadLeft = Left | EVRButtonId.DPadLeft,
        LeftDPadUp = Left | EVRButtonId.DPadUp,
        LeftDPadRight = Left | EVRButtonId.DPadRight,
        LeftDPadDown = Left | EVRButtonId.DPadDown,
        LeftA = Left | EVRButtonId.A,
        LeftProximitySensor = Left | EVRButtonId.ProximitySensor,
        LeftAxis0 = Left | EVRButtonId.Axis0,
        LeftAxis1 = Left | EVRButtonId.Axis1,
        LeftAxis2 = Left | EVRButtonId.Axis2,
        LeftAxis3 = Left | EVRButtonId.Axis3,
        LeftAxis4 = Left | EVRButtonId.Axis4,
        LeftSteamVRTouchpad = Left | EVRButtonId.SteamVRTouchpad,
        LeftSteamVRTrigger = Left | EVRButtonId.SteamVRTrigger,
        LeftDashboardBack = Left | EVRButtonId.DashboardBack,
        LeftMax = Left | EVRButtonId.Max,
        
        RightSystem = Right | EVRButtonId.System,
        RightApplicationMenu = Right | EVRButtonId.ApplicationMenu,
        RightGrip = Right | EVRButtonId.Grip,
        RightDPadLeft = Right | EVRButtonId.DPadLeft,
        RightDPadUp = Right | EVRButtonId.DPadUp,
        RightDPadRight = Right | EVRButtonId.DPadRight,
        RightDPadDown = Right | EVRButtonId.DPadDown,
        RightA = Right | EVRButtonId.A,
        RightProximitySensor = Right | EVRButtonId.ProximitySensor,
        RightAxis0 = Right | EVRButtonId.Axis0,
        RightAxis1 = Right | EVRButtonId.Axis1,
        RightAxis2 = Right | EVRButtonId.Axis2,
        RightAxis3 = Right | EVRButtonId.Axis3,
        RightAxis4 = Right | EVRButtonId.Axis4,
        RightSteamVRTouchpad = Right | EVRButtonId.SteamVRTouchpad,
        RightSteamVRTrigger = Right | EVRButtonId.SteamVRTrigger,
        RightDashboardBack = Right | EVRButtonId.DashboardBack,
        RightMax = Right | EVRButtonId.Max,
        
        Left = 0b00000000,
        Right = 0b10000000
        
        
    }

    
    public class VRControllerInputListener : InputListenerBase<VRControllerPairState, VRButtons>, ICursorInputListener
    {

        public VRControllerInputListener(PlayerIndex playerIndex) : base(playerIndex)
        {
            RegisterMap(InputCommand.MoveForwards, VRButtons.LeftDPadUp);
            RegisterMap(InputCommand.MoveBackwards, VRButtons.LeftDPadDown);
            RegisterMap(InputCommand.MoveLeft, VRButtons.LeftDPadLeft);
            RegisterMap(InputCommand.MoveRight, VRButtons.LeftDPadRight);
            
            RegisterMap(InputCommand.LookUp, VRButtons.RightDPadUp);
            RegisterMap(InputCommand.LookDown, VRButtons.RightDPadDown);
            RegisterMap(InputCommand.LookLeft, VRButtons.RightDPadLeft);
            RegisterMap(InputCommand.LookRight, VRButtons.RightDPadRight);
            
        }

        protected override VRControllerPairState GetCurrentState()
        {
            throw new NotImplementedException();
        }

        protected override bool IsButtonDown(VRControllerPairState state, VRButtons buttons)
        {
            throw new NotImplementedException();
        }

        protected override bool IsButtonUp(VRControllerPairState state, VRButtons buttons)
        {
            throw new NotImplementedException();
        }

        public Vector2 GetCursorPositionDelta()
        {
            throw new NotImplementedException();
        }

        public Vector2 GetCursorPosition()
        {
            throw new NotImplementedException();
        }
    }
}