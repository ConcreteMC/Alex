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
		public DisconnectedScreen()
		{
			TitleTranslationKey = "multiplayer.disconnect.generic";
			
			Footer.AddChild(new GuiButton(MenuButtonClicked)
			{
				TranslationKey = "gui.toTitle",
				Anchor = Alignment.MiddleCenter
			});
		}

		private void MenuButtonClicked()
		{
			Alex.GameStateManager.SetActiveState<TitleState>();
		}

	    protected override void OnShow()
	    {
			AddChild(new GuiTextElement()
			{
				Text = Reason,
				TextColor = TextColor.Red
			});
		    base.OnShow();
	    }
    }
}
