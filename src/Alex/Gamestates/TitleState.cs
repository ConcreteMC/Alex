using System;
using System.Collections.Generic;
using System.Text;
using Alex.Gui.Common;
using Alex.Gui.Controls;
using Microsoft.Xna.Framework;

namespace Alex.Gamestates
{
	public class TitleState : Gamestate
	{
		public TitleState(Game game) : base(game)
		{
		}

		public override void Init(RenderArgs args)
		{
			var button = new UiButton("Play", () => { });
			button.Position = new Point(200, 200);
			button.Width = 200;
			button.Height = 60;
			button.BackgroundColor = Color.SteelBlue;
			button.BorderColor = Color.LightSteelBlue;
			button.BorderWidth = new Thickness(5);

			button.Label.Font = Alex.Font;

			Gui.Root.Controls.Add(button);
		}
	}
}
