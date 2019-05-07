using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Gui.Elements
{
    public class GuiTextureElement : GuiElement
    {
		public TextureSlice2D Texture { get; set; }
	    public TextureRepeatMode RepeatMode { get; set; } = TextureRepeatMode.Stretch;
		public GuiTextureElement()
	    {

	    }

	    protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
	    {
		    base.OnDraw(graphics, gameTime);

		    if (Texture != null)
		    {
			    graphics.FillRectangle(RenderBounds, Texture, RepeatMode);
		    }
	    }
    }
}
