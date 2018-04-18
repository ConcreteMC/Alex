using System;

namespace Alex.API.Gui
{
    [Flags]

    public enum Alignment
    {
        None = 0x00,

        NoneX = 0x01,
        NoneY = 0x10,

        MinX = 0x02,
        MinY = 0x20,

        MaxX = 0x04,
        MaxY = 0x40,

        CenterX = 0x08,
        CenterY = 0x80,
        
        FillX = MinX | MaxX,
        FillY = MinY | MaxY,
        
        Default = None | NoneX | NoneY,

        TopLeft   = MinY | MinX,
        TopCenter = MinY | CenterX,
        TopRight  = MinY | MaxX,
        TopFill   = MinY | FillX,
		
        MiddleLeft   = CenterY | MinX,
        MiddleCenter = CenterY | CenterX,
        MiddleRight  = CenterY | MaxX,
        MiddleFill   = CenterY | FillX,

        BottomLeft   = MaxY | MinX,
        BottomCenter = MaxY | CenterX,
        BottomRight  = MaxY | MaxX,
        BottomFill   = MaxY | FillX,
        
        FillLeft   = FillY | MinX,
        FillCenter = FillY | CenterX,
        FillRight  = FillY | MaxX,
        Fill       = FillY | FillX,

        OrientationX = NoneX | MinX | MaxX | CenterX,
        OrientationY = NoneY | MinY | MaxY | CenterY,
    }
}