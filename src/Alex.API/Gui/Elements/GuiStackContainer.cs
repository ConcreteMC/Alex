using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Alex.API.Gui.Elements
{
    public class GuiStackContainer : GuiContainer
    {
        public Orientation Orientation { get; set; } = Orientation.Vertical;

        public HorizontalAlignment HorizontalContentAlignment { get; set; } = HorizontalAlignment.None;
        public VerticalAlignment VerticalContentAlignment { get; set; } = VerticalAlignment.Top;

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

            int offset = 0;

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


			ForEachChild(c =>
			{
	            if (HorizontalContentAlignment == HorizontalAlignment.Left)
	            {
		            c.LayoutOffsetX = 0;
	            }
	            else if (HorizontalContentAlignment == HorizontalAlignment.Center)
	            {
		            c.LayoutOffsetX = (int)((Width - c.Width) / 2f);
	            }
	            else if (HorizontalContentAlignment == HorizontalAlignment.Right)
	            {
		            c.LayoutOffsetX = (Width - (c.Width));
	            }
            });
		}
    }
}
