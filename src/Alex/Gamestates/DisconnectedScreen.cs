using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Utils;
using Alex.GameStates;
using Alex.GameStates.Gui.Common;

namespace Alex.Gamestates
{
    public class DisconnectedScreen : GuiMenuStateBase
    {
	    public string Reason { get; set; } = "disconnect.lost";
	    public GuiTextElement DisconnectedTextElement { get; private set; }
		public DisconnectedScreen()
		{
			TitleTranslationKey = "multiplayer.disconnect.generic";

			Body.ChildAnchor = Alignment.MiddleCenter;
			Body.AddChild(DisconnectedTextElement = new GuiTextElement()
			{
				Text = Reason,
				TextColor = TextColor.Red,
				Anchor = Alignment.MiddleCenter
			});

			Footer.AddChild(new GuiButton(MenuButtonClicked)
			{
				TranslationKey = "gui.toTitle",
				Anchor = Alignment.MiddleCenter,
				Modern = false
			});
		}

		private void MenuButtonClicked()
		{
			Alex.GameStateManager.SetActiveState<TitleState>("title");
		}

	    protected override void OnShow()
	    {
			
		    base.OnShow();
	    }
    }
}
