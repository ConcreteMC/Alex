using Alex.Common.Gui.Graphics;
using RocketUI;

namespace Alex.Common.Gui.Elements
{
	public class AlexCrosshair : RocketElement
	{
		public AlexCrosshair()
		{
			Anchor = Alignment.MiddleCenter;
			Width = 15;
			Height = 15;
		}

		protected override void OnInit(IGuiRenderer renderer)
		{
			Background = AlexGuiTextures.Crosshair;
		}
	}
}