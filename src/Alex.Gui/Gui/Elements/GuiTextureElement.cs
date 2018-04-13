using System;
using System.Collections.Generic;
using System.Text;
using Alex.Graphics.Gui.Rendering;
using Alex.Graphics.UI.Common;
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
