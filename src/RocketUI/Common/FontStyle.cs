using System;

namespace RocketUI
{
    [Flags]
    public enum FontStyle
    {
        None            = 1 << 0,

        DropShadow      = 1 << 1,
        Bold            = 1 << 2,
        Italic          = 1 << 3,
        StrikeThrough   = 1 << 4,
        Underline       = 1 << 5,
    }
}
