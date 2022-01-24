using System;
using Alex.Common.Data;
using Alex.Common.Gui;
using Alex.Common.Utils;
using Alex.Gui;
using MiNET.UI;
using RocketUI;
using Slider = RocketUI.Slider;

namespace Alex.Gamestates.MainMenu.Options.Ui
{
	public class MinimapOptionsState : OptionsStateBase
	{
		private ToggleButton Minimap { get; set; }
		private Slider MinimapSize { get; set; }
		private Slider<ZoomLevel> MinimapZoomLevel { get; set; }
		private ToggleButton AlphaBlending { get; set; }

		/// <inheritdoc />
		public MinimapOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
		{
			Title = "Minimap";
		}

		/// <inheritdoc />
		protected override void Initialize(IGuiRenderer renderer)
		{
			base.Initialize(renderer);

			AlphaBlending = CreateToggle("Alpha Blending: {0}", o => o.UserInterfaceOptions.Minimap.AlphaBlending);

			Minimap = CreateToggle("Show Minimap: {0}", o => o.UserInterfaceOptions.Minimap.Enabled);

			MinimapSize = CreateSlider("Minimap Size: {0}", o => o.UserInterfaceOptions.Minimap.Size, 0.125d, 2d, 0.1d);

			MinimapZoomLevel = CreateSlider(
				(v) =>
				{
					string display = v.ToString();

					switch (v)
					{
						case ZoomLevel.Level1:
							display = "-4";

							break;

						case ZoomLevel.Level2:
							display = "-3";

							break;

						case ZoomLevel.Level3:
							display = "-2";

							break;

						case ZoomLevel.Level4:
							display = "-1";

							break;

						case ZoomLevel.Level5:
							display = "Default";

							break;

						case ZoomLevel.Level6:
							display = "+1";

							break;

						case ZoomLevel.Level7:
							display = "+2";

							break;

						case ZoomLevel.Level8:
							display = "+3";

							break;

						case ZoomLevel.Level9:
							display = "+4";

							break;

						case ZoomLevel.Level10:
							display = "+5";

							break;
					}

					return $"Zoom Level: {display}";
				}, o => o.UserInterfaceOptions.Minimap.DefaultZoomLevel, ZoomLevel.Minimum, ZoomLevel.Maximum);

			MinimapZoomLevel.StepInterval = ZoomLevel.Level1;

			var position = CreateSlider<ElementPosition>(
				elementPosition =>
				{
					string friendly = elementPosition.ToString();

					switch (elementPosition)
					{
						case ElementPosition.Default:
							friendly = "Middle Right";

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
							friendly = "Top Right (Default)";

							break;

						case ElementPosition.RightBottom:
							friendly = "Bottom Right";

							break;
					}

					return $"Position: {friendly}";
				}, o => o.UserInterfaceOptions.Minimap.Position, ElementPosition.LeftTop, ElementPosition.RightBottom);

			position.StepInterval = ElementPosition.LeftMiddle;
			position.Enabled = false;

			AddGuiRow(Minimap, MinimapSize);
			AddGuiRow(AlphaBlending, MinimapZoomLevel);
			AddGuiRow(position, new RocketElement());

			AddDescription(Minimap, "Minimap", "Enabled: Show a minimap in the HUD");
			AddDescription(MinimapSize, "Minimap Size", "The size of the minimap");
			AddDescription(MinimapZoomLevel, "Minimap Zoom", "The zoomlevel used for the minimap");

			AddDescription(
				AlphaBlending, "Alpha Blending", "Enabled: Materials like water are transparent",
				$"{TextColor.Red}May impact chunk loading performance!");

			AddDescription(position, "Position", "The position of the minimap on your screen");
		}
	}
}