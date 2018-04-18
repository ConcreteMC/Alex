using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Alex.API.Utils
{
    public static class RectangleExtensions
    {

        public static Vector2 CenterLeft(this Rectangle rectangle)
        {
            return new Vector2(rectangle.Left, rectangle.Center.Y);
        }

        public static Vector2 CenterRight(this Rectangle rectangle)
        {
            return new Vector2(rectangle.Right, rectangle.Center.Y);
        }

        public static Vector2 CenterTop(this Rectangle rectangle)
        {
            return new Vector2(rectangle.Center.X, rectangle.Top);
        }

        public static Vector2 CenterBottom(this Rectangle rectangle)
        {
            return new Vector2(rectangle.Center.X, rectangle.Bottom);
        }

    }
}
