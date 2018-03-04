using System;
using System.Collections.Generic;
using System.Text;
using Alex.Gui.Common;
using Alex.Gui.Enums;
using Alex.Gui.Input.Listeners;
using Microsoft.Xna.Framework;

namespace Alex.Gui.Controls.Menu
{
	public class UiMenuItem : UiControl
	{

		public UiLabel Label { get; }

		public Action Action { get; }

		public UiMenuItem(string text, Action action = null)
		{
			Label = new UiLabel(text);
			Action = action;
			
			HorizontalContentAlignment = HorizontalAlignment.Center;
			VerticalContentAlignment = VerticalAlignment.Center;
			Margin = new Thickness(5);

			Controls.Add(Label);
		}

		//protected override void OnMouseEnter(MouseEventArgs args)
		//{
		//	BackgroundColor = Color.LightSlateGray;
		//}

		//protected override void OnMouseLeave(MouseEventArgs args)
		//{
		//	BackgroundColor = Color.Gray;
		//}

		//protected override void OnMouseDown(MouseEventArgs args)
		//{
		//	BackgroundColor = Color.DarkSlateGray;
		//}

		//protected override void OnMouseUp(MouseEventArgs args)
		//{
		//	BackgroundColor = IsMouseOver ? Color.LightSlateGray : Color.Gray;
		//}
	}
}
