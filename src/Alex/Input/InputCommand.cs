using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.Input
{
    public enum InputCommand
    {
        MoveForwards,
        MoveBackwards,
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,

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
