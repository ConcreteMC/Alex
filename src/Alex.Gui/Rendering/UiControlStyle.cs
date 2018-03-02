using System;
using System.Collections.Generic;
using System.Text;
using Alex.Gui.Common;
using Microsoft.Xna.Framework;

namespace Alex.Gui.Rendering
{
	public class UiControlStyle
	{
		public UiElementStyle Default { get; set; }
		public UiElementStyle Hover { get; set; }
		public UiElementStyle Active { get; set; }

		public UiElementStyle Focus { get; set; }
		public UiElementStyle Disabled { get; set; }
	}

	public class UiElementStyle
	{
		public Color Background { get; set; }
		public Color Foreground { get; set; }

		public Color BorderColor { get; set; }
		public Thickness BorderWidth { get; set; }

		public Thickness Thickness { get; set; }
		public Thickness Margin { get; set; }
	}
}
