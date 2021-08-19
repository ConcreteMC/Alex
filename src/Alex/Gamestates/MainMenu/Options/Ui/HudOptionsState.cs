using Alex.Gui;

namespace Alex.Gamestates.MainMenu.Options.Ui
{
	public class HudOptionsState : OptionsStateBase
	{
		/// <inheritdoc />
		public HudOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
		{
			Title = "Hud";
		}
	}
}