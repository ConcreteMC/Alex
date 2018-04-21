using System.Collections.Generic;
using System.Linq;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Rendering;
using Alex.API.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Alex.API.Gui
{
    public class GuiScreen : GuiElement, IGuiScreen
    {
        protected Game Game { get; }

        private List<IGuiElement3D> _3DElements = new List<IGuiElement3D>();
        public bool IsLayoutInProgress { get; protected set; } = false;

        public override IGuiFocusContext FocusContext
        {
            get { return this; }
        }

        public GuiScreen(Game game)
        {
            AutoSizeMode = AutoSizeMode.None;
            Anchor = Alignment.Fill;

            BackgroundRepeatMode = TextureRepeatMode.Tile;
            Game = game;
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

        public void Draw3D(GuiRenderArgs renderArgs)
        {
            var elements3D = _3DElements.ToArray();
            if (elements3D.Any())
            {
                foreach (IGuiElement3D element3D in elements3D)
                {
                    element3D.Draw3D(renderArgs);
                }
            }
        }

        public void RegisterElement(IGuiElement3D element)
        {
            _3DElements.Add(element);
        }

        public void UnregisterElement(IGuiElement3D element)
        {
            _3DElements.Remove(element);
        }
        
        public void HandleContextActive()
        {

        }

        public void HandleContextInactive()
        {

        }
    }
}
