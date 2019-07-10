using System;
using System.Collections.Generic;

namespace RocketUI
{
    [Flags]

    public enum Alignment
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
        private static Alignment[] BaseAlignments = new Alignment[]
        {
            Alignment.NoneX,
            Alignment.MinX,
            Alignment.MaxX,
            Alignment.CenterX,
            Alignment.FillX,

            Alignment.NoneY,
            Alignment.MinY,
            Alignment.MaxY,
            Alignment.CenterY,
            Alignment.FillY
        };

        public static Alignment SwapXY(this Alignment alignment)
        {
            var vertical = (alignment & Alignment.OrientationY);
            var horizontal = (alignment & Alignment.OrientationX);

            var newVertical   = (Alignment)((int)horizontal << 4);
            var newHorizontal = (Alignment)((int)vertical   >> 4);

            return (newVertical | newHorizontal);
        }

        public static string ToFullString(this Alignment alignment)
        {
            var vertical   = (alignment & Alignment.OrientationY);
            var horizontal = (alignment & Alignment.OrientationX);

            return $"({ToFullStringParts(horizontal)}) x ({ToFullStringParts(vertical)}) | {alignment.ToBinary()}";
        }
        public static string ToBinary(this Alignment alignment)
        {
            var binaryStr = Convert.ToString((int) alignment, 2).PadLeft(8, '0');

            var parts = new List<string>();
            var partCount = Math.Ceiling(binaryStr.Length / 4f);
            for (int i = 0; i < partCount; i++)
            {
                parts.Add(binaryStr.Substring(i * 4, 4));
            }

            return string.Join(" ", parts);
        }

        private static string ToFullStringParts(Alignment alignment, Alignment[] checkAlignments = null)
        {
            checkAlignments = checkAlignments ?? BaseAlignments;

            var parts = new List<string>();

            foreach (var check in checkAlignments)
            {
                if ((alignment & check) != 0b0)
                {
                    parts.Add(check.ToString());
                }
            }

            return string.Join(" | ", parts);
        }
    }
}