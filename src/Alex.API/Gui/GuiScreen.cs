using System.Collections.Generic;
using System.Linq;
using Alex.API.Gui.Rendering;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui
{
    public class GuiScreen : GuiElement, IGuiScreen
    {
        protected Game Game { get; }

        private List<IGuiElement3D> _3DElements = new List<IGuiElement3D>();

        public GuiScreen(Game game)
        {
            BackgroundRepeatMode = TextureRepeatMode.Tile;
            Game = game;
        }

        public void UpdateSize(int width, int height)
        {
            Width = width;
            Height = height;

            UpdateLayout();
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
    }
}
