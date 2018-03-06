using System;
using System.Collections.Generic;
using System.Text;
using Alex.Graphics.UI.Themes;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.UI.Rendering
{
    public class UiElementRenderArgs
    {

        public UiElement Element { get; }

        public Rectangle LayoutBounds { get; }
        public Rectangle Bounds { get; }
        public Rectangle ContentBounds { get; }

        public Vector2 Position { get; }

        public UiElementStyle Style { get; }

        public UiElementRenderArgs(UiElement element)
        {
            Element = element;

            Bounds = Element.Bounds;
            ContentBounds = Element.ClientBounds;
            LayoutBounds = Element.OuterBounds;

            Position = ContentBounds.Location.ToVector2();

            Style = Element.Style;
        }

    }
}
