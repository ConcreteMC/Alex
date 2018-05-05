using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RocketUI
{
    [Flags]
    [Serializable]
    public enum Anchor
    {
        None    = 0b00000000,

        NoneX   = 0b00000001, 
        NoneY   = 0b00010000,

        MinX    = 0b00000010,
        MinY    = 0b00100000,

        MaxX    = 0b00000100,
        MaxY    = 0b01000000,

        CenterX = 0b00001000,
        CenterY = 0b10000000,
        
        //FillX = 0x0800,
        //FillY = 0x8000,
        FillX = MinX | MaxX,
        FillY = MinY | MaxY,
        
        //JustifyX = MinX | MaxX,
        //JustifyY = MinY | MaxY,
        
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

        //OrientationX = None | NoneX | MinX | MaxX | CenterX | FillX,
        //OrientationY = None | NoneY | MinY | MaxY | CenterY | FillY,
        OrientationX = 0b00001111,
        OrientationY = 0b11110000,
                       
    }

    public static class AlignmentExtensions
    {
        private static Anchor[] _baseAnchors = new Anchor[]
        {
            Anchor.NoneX,
            Anchor.MinX,
            Anchor.MaxX,
            Anchor.CenterX,
            Anchor.FillX,

            Anchor.NoneY,
            Anchor.MinY,
            Anchor.MaxY,
            Anchor.CenterY,
            Anchor.FillY
        };

        public static Anchor SwapXY(this Anchor anchor)
        {
            var vertical = (anchor & Anchor.OrientationY);
            var horizontal = (anchor & Anchor.OrientationX);

            var newVertical   = (Anchor)((int)horizontal << 4);
            var newHorizontal = (Anchor)((int)vertical   >> 4);

            return (newVertical | newHorizontal);
        }

        public static string ToFullString(this Anchor anchor)
        {
            var vertical   = (anchor & Anchor.OrientationY);
            var horizontal = (anchor & Anchor.OrientationX);

            return $"({ToFullStringParts(horizontal)}) x ({ToFullStringParts(vertical)}) | {anchor.ToBinary()}";
        }
        public static string ToBinary(this Anchor anchor)
        {
            var binaryStr = Convert.ToString((int) anchor, 2).PadLeft(8, '0');

            var parts = new List<string>();
            var partCount = Math.Ceiling(binaryStr.Length / 4f);
            for (int i = 0; i < partCount; i++)
            {
                parts.Add(binaryStr.Substring(i * 4, 4));
            }

            return string.Join(" ", parts);
        }

        private static string ToFullStringParts(Anchor anchor, Anchor[] checkAnchors = null)
        {
            checkAnchors = checkAnchors ?? _baseAnchors;

            var parts = new List<string>();

            foreach (var check in checkAnchors)
            {
                if ((anchor & check) != 0b0)
                {
                    parts.Add(check.ToString());
                }
            }

            return string.Join(" | ", parts);
        }
    }
}