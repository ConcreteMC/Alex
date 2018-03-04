using Alex.Rendering.UI;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
	public class SplashScreen : GameState
	{
		public SplashScreen(Alex alex) : base(alex)
		{

		}

		protected override void OnLoad(RenderArgs args)
		{
			Controls.Add("logo", new Logo()
			{
				DrawMotd = false,
				Center = true
			});
		}
	}
}
