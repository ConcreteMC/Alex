using System;
using System.Linq;

namespace Alex.API.Gui.Elements
{
    public class GuiStackContainer : GuiContainer
    {
        public Orientation Orientation { get; set; } = Orientation.Vertical;

        public HorizontalAlignment HorizontalContentAlignment { get; set; } = HorizontalAlignment.None;
        public VerticalAlignment VerticalContentAlignment { get; set; } = VerticalAlignment.None;

        public int Spacing = 5;

        public GuiStackContainer()
        {

        }

        protected override void OnUpdateLayout()
        {
            base.OnUpdateLayout();

            ForEachChild(c =>
            {
                if (VerticalContentAlignment != VerticalAlignment.None)
                {
                    c.VerticalAlignment = VerticalAlignment.None;
                }

                if (HorizontalContentAlignment != HorizontalAlignment.None)
                {
                    c.HorizontalAlignment = HorizontalAlignment.None;
                }
            });

            AlignContentVertically();
            AlignContentHorizontally();
        }


        protected void AlignContentVertically()
        {
            if (!HasChildren || VerticalContentAlignment == VerticalAlignment.None) return;

            int contentHeight = 0, offset = 0;

            contentHeight = Orientation == Orientation.Vertical
                                ? Children.Sum(c => c.Height) + (Children.Count - 1) * Spacing
                                : Children.Max(c => c.Height);

            if (VerticalContentAlignment == VerticalAlignment.Stretch)
            {
                if (Orientation == Orientation.Vertical)
                {
                    var contentLayoutHeight = (int)Math.Floor((Height - ((Children.Count - 1) * Spacing)) / (float)Children.Count);
                    
                    ForEachChild(c =>
                    {
                        c.LayoutHeight  = contentLayoutHeight;
                        c.LayoutOffsetY = offset;

                        offset += contentLayoutHeight + Spacing;
                    });
                }
                else
                {
                    ForEachChild(c =>
                    {
                        c.LayoutHeight  = Height;
                        c.LayoutOffsetY = 0;
                    });
                }
                
                return;;
            }

            if (VerticalContentAlignment == VerticalAlignment.Top)
            {
                // Offset is 0
            }
            else if (VerticalContentAlignment == VerticalAlignment.Center)
            {
                offset = (int)((Height - contentHeight) / 2f);
            }
            else if (VerticalContentAlignment == VerticalAlignment.Bottom)
            {
                offset = Height - contentHeight;
            }
            
            
            ForEachChild(c =>
            {
                c.LayoutHeight = 0;
                c.LayoutOffsetY = offset;

                offset += c.Height + Spacing;
            });
        }
        protected void AlignContentHorizontally()
        {
            if (!HasChildren || HorizontalContentAlignment == HorizontalAlignment.None) return;

            int contentWidth = 0, offset = 0;

            contentWidth = Orientation == Orientation.Horizontal
                                    ? Children.Sum(c => c.Width) + (Children.Count - 1) * Spacing
                                    : Children.Max(c => c.Width);


            if (HorizontalContentAlignment == HorizontalAlignment.Stretch)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    var contentLayoutWidth = (int)Math.Floor((Width - ((Children.Count - 1) * Spacing)) / (float)Children.Count);
                    
                    ForEachChild(c =>
                    {
                        c.LayoutWidth  = contentLayoutWidth;
                        c.LayoutOffsetX = offset;

                        offset += contentLayoutWidth + Spacing;
                    });
                }
                else
                {
                    ForEachChild(c =>
                    {
                        c.LayoutWidth = Width;
                        c.LayoutOffsetX = 0;
                    });
                }

                return;
            }

            if (HorizontalContentAlignment == HorizontalAlignment.Left)
            {
                // Offset is 0
            }
            else if (HorizontalContentAlignment == HorizontalAlignment.Center)
            {
                offset = (int)((Width - contentWidth) / 2f);
            }
            else if (HorizontalContentAlignment == HorizontalAlignment.Right)
            {
                offset = Width - contentWidth;
            }

            ForEachChild(c =>
            {
                c.LayoutWidth = 0;
                c.LayoutOffsetX = offset;
                offset += c.Width + Spacing;
            });
        }
    }
}
