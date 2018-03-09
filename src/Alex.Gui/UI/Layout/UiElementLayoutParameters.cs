using System;
using System.Collections.Generic;
using System.Text;
using Alex.Graphics.UI.Common;
using Alex.Graphics.UI.Themes;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.UI.Layout
{
    public class UiElementLayoutParameters
    {

        //public Rectangle OuterBounds => Bounds + Margin;
        //public Rectangle Bounds => InnerBounds + Padding;
        //public Rectangle InnerBounds => new Rectangle(new Point(AbsolutePosition.X + Padding.Left + Margin.Left, AbsolutePosition.Y + Padding.Top + Margin.Left), new Point((int)Math.Round(Size.X), (int)Math.Round(Size.Y)));

        public Rectangle OuterBounds => new Rectangle(new Point(Location.X, Location.Y), Size.ToPoint() + Padding.ToPoint() + Margin.ToPoint());
        public Rectangle Bounds      => OuterBounds - Margin;
        public Rectangle InnerBounds => Bounds - Padding;

        public Point Location => BasePosition + RelativePosition;

        public Point BasePosition { get; set; } = Point.Zero;
        public Point RelativePosition { get; set; } = Point.Zero;

        public Thickness BorderSize { get; set; } = Thickness.Zero;

        public Thickness Margin { get; set; }
        public Thickness Padding { get; set; }

        private Vector2 _size = Vector2.Zero;
        public Vector2 Size
        {
            get
            {
                var val = new Vector2(Math.Max(MinSize.X, Math.Max(ContentSize.X, AutoSize.X)), Math.Max(MinSize.Y, Math.Max(ContentSize.Y, AutoSize.Y)));

                if (MaxSize.HasValue)
                {
                    val = new Vector2(Math.Min(MaxSize.Value.X, val.X), Math.Min(MaxSize.Value.Y, val.Y));
                }

                return new Vector2((Math.Abs(_size.X) < 0.5f) ? val.X : _size.X,
                    (Math.Abs(_size.Y)                < 0.5f) ? val.Y : _size.Y);
            }
            set { _size = value; }
        }

        public Vector2 AutoSize { get; set; } = Vector2.Zero;

        public Vector2 ContentSize { get; set; } = Vector2.Zero;

        public Point MinSize { get; set; } = Point.Zero;
        public Point? MaxSize { get; set; } = null;

        public Vector2? PositionAnchor { get; set; } = null;
        public Vector2? SizeAnchor { get; set; } = null;
        public Vector2 SizeAnchorOrigin { get; set; } = Vector2.Zero;
        public Vector2 PositionAnchorOrigin { get; set; } = Vector2.Zero;

        public static UiElementLayoutParameters FromStyle(UiElementStyle style)
        {
            return new UiElementLayoutParameters()
            {
                Margin = style.Margin.GetValueOrDefault(Thickness.Zero),
                Padding = style.Padding.GetValueOrDefault(Thickness.Zero),
                MinSize = new Point(style.MinWidth.GetValueOrDefault(0), style.MinHeight.GetValueOrDefault(0)),
                MaxSize = style.MaxWidth.HasValue || style.MaxHeight.HasValue ? new Point(style.MaxWidth.GetValueOrDefault(int.MaxValue), style.MaxHeight.GetValueOrDefault(int.MaxValue)) : (Point?) null,
                Size = new Vector2(style.Width.GetValueOrDefault(0), style.Height.GetValueOrDefault(0)),
                PositionAnchor = style.PositionAnchor,
                PositionAnchorOrigin = style.PositionAnchorOrigin.GetValueOrDefault(Vector2.Zero),
                SizeAnchor = style.SizeAnchor,
                SizeAnchorOrigin = style.SizeAnchorOrigin.GetValueOrDefault(Vector2.Zero)
            };
        }

    }
}
