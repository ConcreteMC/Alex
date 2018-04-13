using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui;
using Alex.API.Gui.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Gui.Elements
{
    public class GuiTextureElement : GuiElement
    {
		public Texture2D Texture { get; set; }
	    public TextureRepeatMode RepeatMode { get; set; } = TextureRepeatMode.Stretch;
		public GuiTextureElement()
	    {

	    }

	    protected override void OnDraw(GuiRenderArgs args)
	    {
		    base.OnDraw(args);

		    if (Texture != null)
		    {
			    args.DrawNinePatch(Bounds, Texture, RepeatMode);
		    }
	    }
    }
}
