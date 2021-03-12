using Alex.API.Gui.Graphics;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.API.Gui.Elements
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