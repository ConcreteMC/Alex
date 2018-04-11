using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.Graphics.UI.Common;

namespace Alex.Graphics.Gui.Elements
{
    public class GuiStackMenu : GuiContainer
    {
        public Orientation Orientation { get; set; } = Orientation.Vertical;
        
        public HorizontalAlignment HorizontalContentAlignment { get; set; } = HorizontalAlignment.None;
        public VerticalAlignment   VerticalContentAlignment   { get; set; } = VerticalAlignment.None;

        public int Spacing = 8;
        private int _width;
        
        public GuiStackMenu()
        {

        }

        protected override void OnUpdateLayout()
        {
            base.OnUpdateLayout();

            ForEachChild(c =>
            {
                c.VerticalAlignment = VerticalAlignment.None;
                c.HorizontalAlignment = HorizontalAlignment.None;
            });

            AlignContentVertically();
            AlignContentHorizontally();
        }

        
        protected void AlignContentVertically()
        {
            if (!HasChildren || VerticalContentAlignment == VerticalAlignment.None) return;

            var contentHeight = Orientation == Orientation.Vertical
                                    ? Children.Sum(c => c.Height) + (Children.Count - 1) * Spacing
                                    : Children.Max(c => c.Height);

            var offset = 0;
            if (VerticalContentAlignment == VerticalAlignment.Top)
            {
                // Offset is 0
            }
            else if (VerticalContentAlignment == VerticalAlignment.Center)
            {
                offset = (int) ((Height - contentHeight) / 2f);
            }
            else if(VerticalContentAlignment == VerticalAlignment.Bottom)
            {
                offset = Height - contentHeight;
            }

            ForEachChild(c =>
            {
                c.LayoutOffsetY = offset;
                offset += c.Height + Spacing;
            });
        }
        protected void AlignContentHorizontally()
        {
            if (!HasChildren || HorizontalContentAlignment == HorizontalAlignment.None) return;
            
            var contentWidth = Orientation == Orientation.Horizontal
                                    ? Children.Sum(c => c.Width) + (Children.Count - 1) * Spacing
                                    : Children.Max(c => c.Width);

            var offset = 0;
            if (HorizontalContentAlignment == HorizontalAlignment.Left)
            {
                // Offset is 0
            }
            else if (HorizontalContentAlignment == HorizontalAlignment.Center)
            {
                offset = (int) ((Width - contentWidth) / 2f);
            }
            else if(HorizontalContentAlignment == HorizontalAlignment.Right)
            {
                offset = Width - contentWidth;
            }

            ForEachChild(c =>
            {
                c.LayoutOffsetY =  offset;
                offset          += c.Height + Spacing;
            });
        }
    }
}
