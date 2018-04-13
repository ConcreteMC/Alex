using System;

namespace Alex.API.Input
{
    [Flags]
    public enum MouseButton
    {
        None,
        Left,
        Middle,
        Right,
        XButton1,
        XButton2,

        ScrollUp,
        ScrollDown,
    }
}
