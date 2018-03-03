using Alex.Rendering.UI;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
	public class SplashScreen : Gamestate
	{
		public SplashScreen(Alex alex) : base(alex)
		{

		}

		public override void Init(RenderArgs args)
		{
			Controls.Add("logo", new Logo()
			{
				DrawMotd = false,
				Center = true
			});
		}
	}
}
