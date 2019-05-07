using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Utils
{
    public static class PointExtensions
    {

        public static Point Max(this Point a, Point b)
        {
            return new Point(
                Math.Max(a.X, b.X),
                Math.Max(a.Y, b.Y)
                );
        }

        public static Point Min(this Point a, Point b)
        {
            return new Point(
                Math.Min(a.X, b.X),
                Math.Min(a.Y, b.Y)
            );
        }
    }
}
