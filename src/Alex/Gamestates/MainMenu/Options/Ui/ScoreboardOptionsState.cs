using Alex.Gui;

namespace Alex.Gamestates.MainMenu.Options.Ui
{
	public class ScoreboardOptionsState : OptionsStateBase
	{
		/// <inheritdoc />
		public ScoreboardOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
		{
			Title = "Scoreboard";
		}
	}
}