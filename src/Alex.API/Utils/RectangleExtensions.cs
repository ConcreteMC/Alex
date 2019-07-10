using Microsoft.Xna.Framework;

namespace Alex.API.Utils
{
    public static class RectangleExtensions
    {
        public static Vector2 TopLeft(this Rectangle rectangle)      => new Vector2(    rectangle.Left,      rectangle.Top);
        public static Vector2 TopCenter(this Rectangle rectangle)    => new Vector2(rectangle.Center.X,      rectangle.Top);
        public static Vector2 TopRight(this Rectangle rectangle)     => new Vector2(   rectangle.Right,      rectangle.Top);

        public static Vector2 CenterLeft(this Rectangle rectangle)   => new Vector2(    rectangle.Left, rectangle.Center.Y);
        public static Vector2 CenterRight(this Rectangle rectangle)  => new Vector2(   rectangle.Right, rectangle.Center.Y);

        public static Vector2 BottomLeft(this Rectangle rectangle)   => new Vector2(    rectangle.Left,   rectangle.Bottom);
        public static Vector2 BottomCenter(this Rectangle rectangle) => new Vector2(rectangle.Center.X,   rectangle.Bottom);
        public static Vector2 BottomRight(this Rectangle rectangle)  => new Vector2(   rectangle.Right,   rectangle.Bottom);
    }
}
