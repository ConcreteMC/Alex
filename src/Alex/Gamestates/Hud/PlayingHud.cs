using System;
using System.Collections.Generic;
using System.Text;
using Alex.Gamestates.Gui;
using Alex.Graphics.Gui;
using Alex.Graphics.Gui.Rendering;
using Microsoft.Xna.Framework;

namespace Alex.Gamestates.Hud
{
    public class PlayingHud : GuiScreen
    {
        private GuiItemHotbar _hotbar;

        public PlayingHud(Game game) : base(game)
        {
            _hotbar = new GuiItemHotbar();
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            AddChild(_hotbar);
        }

        protected override void OnUpdateLayout()
        {
            _hotbar.X = (int)((Width / (float)_hotbar.Width) / 2f);
            _hotbar.Y = Height - _hotbar.Height;
            //_hotbar.X = new GuiScalar(0f, (int)((AbsoluteWidth - _hotbar.AbsoluteWidth) / 2f));
            //_hotbar.Y = new GuiScalar(1f, -_hotbar.AbsoluteHeight);
        }
    }
}
