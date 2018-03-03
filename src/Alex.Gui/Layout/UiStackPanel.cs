using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.Gui.Enums;
using Microsoft.Xna.Framework;

namespace Alex.Gui.Layout
{
	public class UiStackPanel : UiContainer
	{

		public Orientation Orientation { get; set; } = Orientation.Vertical;

		public UiStackPanel()
		{

		}

		protected override void OnUpdateLayout()
		{
			base.OnUpdateLayout();

			var offset = 0;

			foreach (var control in Controls.ToArray())
			{
				if (Orientation == Orientation.Horizontal)
				{
					// Increase X
					control.Offset = new Point(offset, 0);
					offset += control.Bounds.Width;
				}
				else if (Orientation == Orientation.Vertical)
				{
					// Increase Y
					control.Offset = new Point(0, offset);
					offset += control.Bounds.Height;
				}
			}
		}
	}
}
