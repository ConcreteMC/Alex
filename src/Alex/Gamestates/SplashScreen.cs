using Alex.Rendering.UI;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
    public class SplashScreen : Gamestate
    {
	    private Logo _logo;
	    public SplashScreen(GraphicsDevice graphics) : base(graphics)
	    {

	    }

	    public override void Init(RenderArgs args)
	    {
		    _logo = new Logo()
		    {
			    DrawMotd = false,
			    Center = true
		    };

			Controls.Add("logo", _logo);
			base.Init(args);
	    }
    }
}
