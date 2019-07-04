using System.Collections.Generic;
using System.Linq;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Alex.API.Gui
{
    public class GuiScreen : GuiElement, IGuiScreen
    {
        public bool IsLayoutInProgress { get; protected set; } = false;

        public override IGuiFocusContext FocusContext
        {
            get { return this; }
        }

        public override IGuiScreen Screen
        {
            get => this;
        }

        public IGuiControl FocusedControl { get; private set; }


        public GuiScreen()
        {
            AutoSizeMode = AutoSizeMode.None;
            Anchor = Alignment.Fill;
            ClipToBounds = true;
        }

        public void UpdateSize(int width, int height)
        {
            Width = width;
            Height = height;

            InvalidateLayout(true);
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

        public bool Focus(IGuiControl control)
        {
            FocusedControl?.InvokeFocusDeactivate();
            FocusedControl = control;
            FocusedControl?.InvokeFocusActivate();
            return true;
        }

        public void ClearFocus(IGuiControl control)
        {
            if (FocusedControl == control)
            {
                FocusedControl?.InvokeFocusDeactivate();
                FocusedControl = null;
            }
        }

        public void HandleContextActive()
        {

        }

        public void HandleContextInactive()
        {

        }
    }
}
