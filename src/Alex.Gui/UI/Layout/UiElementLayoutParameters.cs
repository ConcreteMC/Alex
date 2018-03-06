using System;
using System.Collections.Generic;
using System.Text;
using Alex.Graphics.UI.Common;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.UI.Layout
{
    public class UiElementLayoutParameters
    {

        public Rectangle OuterBounds => Bounds + Margin;
        public Rectangle Bounds => InnerBounds + Padding;
        public Rectangle InnerBounds => new Rectangle(new Point(Position.X + Padding.Left + Margin.Left, Position.Y + Padding.Top + Margin.Left), new Point((int)Math.Round(Size.X), (int)Math.Round(Size.Y)));

        public Point Position { get; set; }

        public Thickness BorderSize { get; set; } = Thickness.Zero;

        public Thickness Margin { get; set; }
        public Thickness Padding { get; set; }

        private Vector2 _size = Vector2.Zero;
        public Vector2 Size
        {
            get
            {
                if (_size == Vector2.Zero)
                {
                    var val = new Vector2(Math.Max(MinSize.X, ContentSize.X), Math.Max(MinSize.Y, ContentSize.Y));

                    if (MaxSize.HasValue)
                    {
                        val = new Vector2(Math.Min(MaxSize.Value.X, val.X), Math.Min(MaxSize.Value.Y, val.Y));
                    }

                    return val;
                }

                return _size;
            }
            set
            {
                var val = new Vector2(Math.Max(MinSize.X, value.X), Math.Max(MinSize.Y, value.Y));

                if (MaxSize.HasValue)
                {
                    val = new Vector2(Math.Min(MaxSize.Value.X, val.X), Math.Min(MaxSize.Value.Y, val.Y));
                }

                _size = val;
            }
        }

        public Vector2 ContentSize { get; set; } = Vector2.Zero;

        public Point MinSize { get; set; } = Point.Zero;
        public Point? MaxSize { get; set; } = null;

        public Vector2? PositionAnchor { get; set; } = null;
        public Vector2? SizeAnchor { get; set; } = null;
        public Vector2 SizeAnchorOrigin { get; set; } = Vector2.Zero;
        public Vector2 PositionAnchorOrigin { get; set; } = Vector2.Zero;

    }
}
