using Alex.Common.Data;
using Alex.Gui;
using RocketUI;

namespace Alex.Gamestates.MainMenu.Options
{
	public class UserInterfaceOptionsState : OptionsStateBase
	{
		private ToggleButton Minimap            { get; set; }
		private Slider MinimapSize { get; set; }
		private Slider<ZoomLevel> MinimapZoomLevel { get; set; }
		
		/// <inheritdoc />
		public UserInterfaceOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
		{
			Title = "User Interface";
		}

		/// <inheritdoc />
		protected override void OnInit(IGuiRenderer renderer)
		{
			base.OnInit(renderer);
			
			Minimap = CreateToggle("Show Minimap: {0}", o => o.UserInterfaceOptions.Minimap.Enabled);

			MinimapSize = CreateSlider(
				"Minimap Size: {0}", o => o.UserInterfaceOptions.Minimap.Size, 0.125d, 2d, 0.1d);

			MinimapZoomLevel = CreateSlider((v) =>
			{
				string display = v.ToString();

				if (v == ZoomLevel.Maximum)
				{
					display = "Max";
				}
				else if (v == ZoomLevel.Minimum)
				{
					display = "Min";
				}
				else if (v == ZoomLevel.Default)
				{
					display = "Default";
				}
				
				return $"Zoom Level: {display}";
			}, o => o.UserInterfaceOptions.Minimap.DefaultZoomLevel, ZoomLevel.Minimum, ZoomLevel.Maximum);
			
			AddGuiRow(Minimap, MinimapSize);
			AddGuiRow(MinimapZoomLevel);
			
			AddDescription(Minimap, "Minimap", "Adds a minimap", "May impact performance");
			AddDescription(MinimapSize, "Minimap Size", "The size of the minimap");
			AddDescription(MinimapZoomLevel, "Minimap Zoom", "The zoomlevel used for the minimap");
		}
	}
}