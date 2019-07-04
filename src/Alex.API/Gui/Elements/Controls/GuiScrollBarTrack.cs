using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui.Graphics;

namespace Alex.API.Gui.Elements.Controls
{
	public class GuiScrollBarTrack : GuiButton
	{
		public GuiScrollBarTrack()
		{
			Background            = GuiTextures.ScrollBarTrackDefault;
			HighlightedBackground = GuiTextures.ScrollBarTrackHover;
			FocusedBackground     = GuiTextures.ScrollBarTrackFocused;
			DisabledBackground    = GuiTextures.ScrollBarTrackDisabled;

			Background.RepeatMode            = TextureRepeatMode.NoScaleCenterSlice;
			HighlightedBackground.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;
			FocusedBackground.RepeatMode     = TextureRepeatMode.NoScaleCenterSlice;
			DisabledBackground.RepeatMode    = TextureRepeatMode.NoScaleCenterSlice;
		}
	}
}