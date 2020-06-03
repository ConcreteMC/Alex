using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Utils;
using Alex.Gamestates.Common;
using RocketUI;

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
			Alex.IsMouseVisible = true;
		}

	    protected override void OnShow()
	    {
		    Alex.IsMouseVisible = true;
		    
		    base.OnShow();
	    }
    }
}
