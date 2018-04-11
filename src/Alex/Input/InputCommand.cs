using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.Input
{
    public enum InputCommand
    {
        MoveForwardy,
        MoveBackie,
        MoveUppy,
        MoveDownie,
        MoveLeftie,
        MoveRightie,

        CameraUp,
        CameraDown,
        CameraLeft,
        CameraRight,

        MoveSpeedIncrease,
        MoveSpeedDecrease,
        MoveSpeedReset,

        ToggleFog,
        ToggleMenu,
        ToggleDebugInfo,
        ToggleChat,
        ToggleCamera,
        ToggleCameraFree,
        ToggleWireframe,
        
        RebuildChunks,

        A,
        B,
        X,
        Y,

        Start,


    }
}
