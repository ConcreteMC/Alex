using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Layout;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gui.Elements
{
	public class LoadingOverlay : GuiScreen
	{
		public LoadingOverlay()
		{
			Anchor = Alignment.Fill;
			BackgroundOverlay = Color.Black * 0.5f;

			var container = new GuiStackContainer()
			{
				Anchor = Alignment.MiddleCenter,
				Orientation = Orientation.Vertical
			};
			
			container.AddChild(new GuiTextElement("Please wait...")
			{
				Anchor = Alignment.MiddleCenter
			});
			
			container.AddChild(new LoadingIndicator()
			{
				Anchor = Alignment.MiddleCenter,
				Width = 300,
				Height = 10,
				ForegroundColor = Color.Red,
				BackgroundColor = Color.Black,
				Margin = new Thickness(30, 30),
				Padding = new Thickness(0, 25, 0, 0)
			});
			
			AddChild(container);
		}
	}
}