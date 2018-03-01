using Alex.Rendering.UI;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
    public class SplashScreen : Gamestate
    {
	    public SplashScreen(GraphicsDevice graphics) : base(graphics)
	    {

	    }

	    public override void Init(RenderArgs args)
	    {
		    Controls.Add("logo", new Logo()
		    {
				DrawMotd = false,
				Center = true
		    });
			base.Init(args);
	    }
    }
}
