using Alex.Common.Data;
using Alex.Gui;
using RocketUI;

namespace Alex.Gamestates.MainMenu.Options.Ui
{
	public class UserInterfaceOptionsState : OptionsStateBase
	{
		/// <inheritdoc />
		public UserInterfaceOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
		{
			Title = "User Interface";
		}

		/// <inheritdoc />
		protected override void OnInit(IGuiRenderer renderer)
		{
			base.OnInit(renderer);

			var scoreboardButton = CreateLinkButton<ScoreboardOptionsState>("options.ui.scoreboard", "Scoreboard");
			var hudButton = CreateLinkButton<HudOptionsState>("options.ui.hud", "Hud");
			var minimapButton = CreateLinkButton<MinimapOptionsState>("options.ui.minimap", "Minimap");

			scoreboardButton.Enabled = false;
			hudButton.Enabled = false;
			
			AddGuiRow(minimapButton, hudButton);
			AddGuiRow(scoreboardButton);
		}
	}
}