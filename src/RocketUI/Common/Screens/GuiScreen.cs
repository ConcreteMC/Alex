using System.Drawing;
using Microsoft.Xna.Framework;
using RocketUI.Elements;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace RocketUI.Screens
{
    public class GuiScreen : VisualElement, IGuiScreen
    {
        public bool IsLayoutInProgress { get; protected set; } = false;

        public override IGuiFocusContext FocusContext
        {
            get { return this; }
        }

        public GuiScreen()
        {
            AutoSizeMode = AutoSizeMode.None;
            Anchor = RocketUI.Anchor.Fill;
        }

        public void UpdateSize(int width, int height)
        {
            Width = width;
            Height = height;

            OnUpdateSize();

            InvalidateLayout();
        }

        protected virtual void OnUpdateSize()
        {

        }
        
        public void UpdateLayout()
        {
            if (!IsLayoutDirty || IsLayoutInProgress) return;
            IsLayoutInProgress = true;
            
            // Pass 1 - Update the Preferred size for all elements with
            //          fixed sizes
            DoLayoutSizing();

            // Pass 2 - Update the actual sizes for all children based upon their
            //          parent sizes.
            BeginLayoutMeasure();
            Measure(new Size(Width, Height));

            // Pass 3 - Arrange all child elements based on the LayoutManager for
            //          the current element.
            BeginLayoutArrange();
            Arrange(new Rectangle(Point.Zero, new Size(Width, Height)));
            
            OnUpdateLayout();

            IsLayoutDirty      = false;
            IsLayoutInProgress = false;
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            if (IsLayoutDirty)
            {
                UpdateLayout();
            }

            base.OnUpdate(gameTime);
        }
        
        public void HandleContextActive()
        {

        }

        public void HandleContextInactive()
        {

        }
    }
}
