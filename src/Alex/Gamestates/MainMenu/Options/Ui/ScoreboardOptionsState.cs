using System;
using Alex.Common.Gui;
using Alex.Gui;

namespace Alex.Gamestates.MainMenu.Options.Ui
{
	public class ScoreboardOptionsState : OptionsStateBase
	{
		/// <inheritdoc />
		public ScoreboardOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
		{
			Title = "Scoreboard";

			var position = CreateSlider<ElementPosition>(
				elementPosition =>
				{
					string friendly = elementPosition.ToString();
					switch (elementPosition)
					{
						case ElementPosition.Default:
							friendly = "Middle Right (Default)";
							break;

						case ElementPosition.LeftTop:
							friendly = "Top Left";
							break;

						case ElementPosition.LeftMiddle:
							friendly = "Middle Left";
							break;

						case ElementPosition.LeftBottom:
							friendly = "Bottom Left";
							break;

						case ElementPosition.RightTop:
							friendly = "Top Right";
							break;

						case ElementPosition.RightBottom:
							friendly = "Bottom Right";
							break;
					}
					return $"Position: {friendly}";
				},
				o => o.UserInterfaceOptions.Scoreboard.Position, ElementPosition.LeftTop, ElementPosition.RightBottom);

			position.StepInterval = ElementPosition.LeftMiddle;
			position.Enabled = false;
			
			//var position = CreateSwitch("Position: {0}", o => o.UserInterfaceOptions.Scoreboard.Position);
			
			var enabled = CreateToggle("Enabled: {0}", o => o.UserInterfaceOptions.Scoreboard.Enabled);
			
			AddGuiRow(enabled, position);
			
			AddDescription(position, "Position", "The position of the scoreboard");
			AddDescription(enabled, "Enabled", "Toggles the visibility of the scoreboard.");
		}
	}
}